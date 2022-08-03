using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class Curve : MonoBehaviour, ITravelableCurve
{
  [SerializeField]
  [HideInInspector]
  protected bool timeFromLength;
  public virtual bool TimeFromLength
  {
    get { return timeFromLength; }
    set
    {
      timeFromLength = value;
      if (value && LengthTimes.Count < ControlPointCount)
      {
        CalculateLength();
      }
    }
  }

  [SerializeField]
  protected List<float> LengthTimes;

  // public float GetTimeAtIndex(int index)
  // {
  //   return LengthTimes[index];
  // }

  [SerializeField]
  [HideInInspector]
  protected float length = 0.0f;
  public virtual float Length
  {
    get { return length; }
    set { length = value; }
  }
  [SerializeField]
  [HideInInspector]
  protected float totalTime = 1.0f;
  public virtual float TotalTime
  {
    get
    {
      // if we're using length as time, return the length.
      if (TimeFromLength)
      {
        return Length;
      }
      return totalTime;
    }
    set
    {
      if (value > 0.0f)
      {
        totalTime = value;
      }
    }
  }

  [SerializeField]
  [HideInInspector]
  protected bool loop = false;
  public virtual bool Loop
  {
    get { return loop; }
    set
    {
      if (loop != value)
      {
        loop = value;
        if (value == true)
        {
          points.Add(points[0]);
        }
        else
        {
          points.RemoveAt(ControlPointCount - 1);
        }
        CalculateLength();
        OnPointsChange();
      }
    }
  }

  [SerializeField]
  [HideInInspector]
  protected List<Vector3> points;

  public int ControlPointCount
  {
    get { return points.Count; }
  }

  public delegate void PointsChangedDelegate();
  public PointsChangedDelegate pointsChangedDelegate;
  public virtual void OnPointsChange()
  {
    // Debug.Log("Curve opc");
    if (pointsChangedDelegate != null)
    {
      pointsChangedDelegate();
    }
  }

  public delegate void SetPointDelegate(int index, Vector3 point, Curve curve);
  public SetPointDelegate setPointDelegate;
  public virtual void SetPoint(int index, Vector3 point)
  {
    if (index >= 0 && index < ControlPointCount)
    {
      points[index] = point;
    }
    if (loop && index == 0)
    {
      points[ControlPointCount - 1] = point;
    }
    CalculateLength();
    if (setPointDelegate != null)
    {
      setPointDelegate(index, point, this);
    }
    OnPointsChange();
  }

  public virtual Vector3 GetControlPoint(int index)
  {
    return points[index];
  }

  public virtual Vector3 GetControlPointWorld(int index)
  {
    return this.transform.TransformPoint(points[index]);

  }

  public delegate void AddPointAtEndDelegate(Vector3 point, Curve curve);
  public AddPointAtEndDelegate addPointAtEndDelegate;
  public virtual void AddPointAtEnd()
  {
    int endIndex = ControlPointCount - 1;
    if (loop)
    {
      endIndex = ControlPointCount - 2;
    }
    Vector3 endPoint = points[endIndex];
    endPoint.x += 0.5f;
    if (loop)
    {
      points.Insert(ControlPointCount - 1, endPoint);
    }
    else
    {
      points.Add(endPoint);
    }
    CalculateLength();
    if (addPointAtEndDelegate != null)
    {
      addPointAtEndDelegate(endPoint, this);
    }
    OnPointsChange();
  }
  public virtual void AddPointAtIndex(int index)
  {
    // add a point at the end if we're adding after the last point.
    if (index + 1 > ControlPointCount - 1 || index < 0)
    {
      AddPointAtEnd();
    }
    else if (index >= 0)
    {
      // Duplicate the point & insert it after the selected point.
      Vector3 point = points[index];
      point.x -= 0.5f;
      points.Insert(index, point);
    }
    CalculateLength();
  }

  public delegate void RemovePointDelegate(int index, Curve curve);
  public RemovePointDelegate removePointDelegate;
  public virtual void RemovePoint(int index)
  {
    // Make sure index is within bounds
    if (index < ControlPointCount && index >= 0)
    {
      // If it's a loop curve, don't delete when < 3 points.
      if (loop && ControlPointCount > 3)
      {
        // Remove the index
        points.RemoveAt(index);
        if (removePointDelegate != null)
        {
          removePointDelegate(index, this);
        }
        // In a loop, if we've removed the start index, need to update the last
        // point to also be at the start.
        if (index == 0)
        {
          points[ControlPointCount - 1] = points[0];
        }
      }
      // otherwise we only need 2 control points minimum, and things can just be removed.
      else if (!loop && ControlPointCount > 2)
      {
        points.RemoveAt(index);
        if (removePointDelegate != null)
        {
          removePointDelegate(index, this);
        }
      }
      CalculateLength();
    }
    OnPointsChange();
  }

  public virtual float GetTimeAtIndex(int index)
  {
    if (TimeFromLength)
    {
      return LengthTimes[index];
    }
    return (index / (float)(ControlPointCount - 1)) * TotalTime;
  }

  public virtual TimedPoint GetTimedPoint(float time)
  {
    return new TimedPoint(GetPoint(time), time);
  }

  public abstract Vector3 GetPoint(float time);

  public virtual Vector3 GetPointWorld(float time)
  {
    return this.transform.TransformPoint(GetPoint(time));
  }
  public abstract Vector3 GetTangent(float time);

  public virtual Vector3 GetTangentWorld(float time)
  {
    return this.transform.TransformDirection(GetTangent(time));
  }

  protected virtual TimedPoint[] GetTimedPoints(Vector3 localPos, TimedPoint[] timedPoints)
  {
    // what we are doing
    // find which point in time points localPos is closest to.
    // if it's the start or end, use that point, the mid point, and the mid between those points (in the correct order)
    // if its the middle, use the middle, and the 2 midpoints between start,mid and mid,end.
    float dp0 = (localPos - timedPoints[0].Point).sqrMagnitude;
    float dp1 = (localPos - timedPoints[1].Point).sqrMagnitude;
    float dp2 = (localPos - timedPoints[2].Point).sqrMagnitude;
    if (dp0 < dp1 && dp0 < dp2)
    {
      // closest to start, use [start, newMid, mid]
      float midT = timedPoints[0].Time + (timedPoints[1].Time - timedPoints[0].Time) / 2;
      Vector3 midP = GetPoint(midT);
      timedPoints[2] = timedPoints[1];
      timedPoints[1] = new TimedPoint(midP, midT);
    }
    else if (dp1 < dp0 && dp1 < dp2)
    {
      // closest to mid, use [startMid, mid, endMid]
      float startMidT = timedPoints[0].Time + (timedPoints[1].Time - timedPoints[0].Time) / 2;
      Vector3 startMidP = GetPoint(startMidT);
      float endMidT = timedPoints[1].Time + (timedPoints[2].Time - timedPoints[1].Time) / 2;
      Vector3 endMidP = GetPoint(endMidT);
      timedPoints[0] = new TimedPoint(startMidP, startMidT);
      timedPoints[2] = new TimedPoint(endMidP, endMidT);
    }
    else
    {
      // closest to end, use [mid, newMid, end]
      float midT = timedPoints[1].Time + (timedPoints[2].Time - timedPoints[1].Time) / 2;
      Vector3 midP = GetPoint(midT);
      timedPoints[0] = timedPoints[1];
      timedPoints[1] = new TimedPoint(midP, midT);
    }
    return timedPoints;
  }

  public virtual List<TimedPoint> GetClosestTimesAtPoint(Vector3 worldPos)
  {
    List<TimedPoint> closestPoints = new List<TimedPoint>();
    int subdivisions = 100;
    Vector3 localPos = transform.InverseTransformPoint(worldPos);
    Vector3 p0 = GetPoint(0);
    float p0t = 0f;
    // we want to go through each set of control points
    for (int i = 1; i < ControlPointCount; i++)
    {
      float p2t = GetTimeAtIndex(i);
      Vector3 p2 = GetPoint(p2t);
      float p1t = p0t + ((p2t - p0t) / 2);
      Vector3 p1 = GetPoint(p1t);
      // Set up the initial timed poitns array for each pair of ctrl points.
      TimedPoint[] timedPoints = new TimedPoint[]{
        new TimedPoint(p0, p0t),
        new TimedPoint(p1, p1t),
        new TimedPoint(p2, p2t)
      };
      // then subdivide until we find the closest point.
      for (int j = 0; j < subdivisions; j++)
      {
        timedPoints = GetTimedPoints(localPos, timedPoints);
      }
      // the mid point should be the interpolated closest point for that line segment.
      closestPoints.Add(timedPoints[1]);
      // Vector3 cp = transform.TransformPoint(timedPoints[1].Point);
      // Debug.DrawLine(cp, cp + Vector3.up, Color.red, 0.01f);
      p0 = p2;
      p0t = p2t;
    }
    return closestPoints;
  }


  public virtual TimedPoint GetClosestTimeAtPoint(Vector3 worldPos)
  {
    List<TimedPoint> closestPoints = GetClosestTimesAtPoint(worldPos);
    Vector3 localPos = transform.InverseTransformPoint(worldPos);
    float minDistance = Mathf.Infinity;
    TimedPoint closest = closestPoints[0];
    foreach (TimedPoint p in closestPoints)
    {
      float d = Vector3.Distance(localPos, p.Point);
      if (d < minDistance)
      {
        closest = p;
        minDistance = d;
      }
    }
    return closest;
  }

  public virtual TimedPoint GetClosestTimeAtPointWorld(Vector3 worldPos)
  {
    TimedPoint tp = GetClosestTimeAtPoint(worldPos);
    return new TimedPoint(transform.TransformPoint(tp.Point), tp.Time);
  }


  public virtual Bounds CalculateBounds()
  {
    Vector3 min = Vector3.positiveInfinity;
    Vector3 max = Vector3.negativeInfinity;
    float m = Length / 100;
    for (float i = 0; i < Length; i += m)
    {
      Vector3 p = GetPointWorld(i);
      min.x = Mathf.Min(p.x, min.x);
      min.y = Mathf.Min(p.y, min.y);
      min.z = Mathf.Min(p.z, min.z);
      max.x = Mathf.Max(p.x, max.x);
      max.y = Mathf.Max(p.y, max.y);
      max.z = Mathf.Max(p.z, max.z);
    }
    Bounds b = new Bounds((min + max) / 2, (max - min));
    return b;
  }

  public abstract void CalculateLength();

  // need to remember to write a reset.
  public abstract void Reset();
  // Base OnEnable calls reset, which allows for in-game adding of curves.
  // But doesn't do anything if the line is already set up and has points.
  public virtual void OnEnable()
  {
    if (this.points == null)
    {
      Reset();
    }
  }

  private void OnDrawGizmosSelected()
  {

    Gizmos.color = Color.green;
    Bounds b = CalculateBounds();
    Gizmos.DrawWireCube(b.center, b.size);

  }
}

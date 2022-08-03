using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class MultiCurve : MonoBehaviour, ITravelableCurve
{
  [SerializeField]
  [HideInInspector]
  private bool loop;
  public bool Loop
  {
    get { return loop; }
    set
    {
      loop = value;
      if (value == true)
      {
        // add a point to last curve
        Curves[CurveCount - 1].addPointAtEndDelegate -= AddPointAtEndHandler;
        Curves[CurveCount - 1].AddPointAtEnd();
        Curves[CurveCount - 1].addPointAtEndDelegate += AddPointAtEndHandler;
        Curves[CurveCount - 1].SetPoint(Curves[CurveCount - 1].ControlPointCount - 1, Curves[0].GetControlPoint(0));
      }
      else
      {
        // remove a point from last curve
        Curves[CurveCount - 1].RemovePoint(Curves[CurveCount - 1].ControlPointCount - 1);
      }
      CalculateTotalLength();
    }
  }
  [SerializeField]
  private bool autoJoinCurves = true;
  [SerializeField]
  [HideInInspector]
  private bool timesFromLength;
  public bool TimesFromLength
  {
    get { return timesFromLength; }
    set
    {
      timesFromLength = value;
      foreach (Curve c in Curves)
      {
        c.TimeFromLength = value;
      }
      CalculateTotalTime();
    }
  }
  [SerializeField]
  [HideInInspector]
  private float totalTime;
  public float TotalTime
  {
    get { return totalTime; }
    set { totalTime = value; }
  }

  [SerializeField]
  [HideInInspector]
  private float length;
  public float Length
  {
    get { return length; }
    set
    {
      length = value;
      if (TimesFromLength)
      {
        TotalTime = length;
      }
    }
  }

  void OnDisable()
  {
    UnregisterDelegates();
  }

  public void CheckRegisterDelegates()
  {
    foreach (Curve curve in Curves)
    {
      if (curve.setPointDelegate == null)
      {
        curve.setPointDelegate += SetPointHandler;
      }
      if (curve.addPointAtEndDelegate == null)
      {
        curve.addPointAtEndDelegate += AddPointAtEndHandler;
      }
      if (curve.removePointDelegate == null)
      {
        curve.removePointDelegate += RemovePointHandler;
      }
    }
  }
  public void UnregisterDelegates()
  {
    foreach (Curve curve in Curves)
    {
      curve.setPointDelegate -= SetPointHandler;
      curve.addPointAtEndDelegate -= AddPointAtEndHandler;
      curve.removePointDelegate -= RemovePointHandler;
    }
  }

  [SerializeField]
  private List<Curve> Curves = new List<Curve>();

  public int CurveCount { get { return Curves.Count; } }
  void Reset()
  {
    // Might want to move this to inspector so we can record & undo.
    Curves = new List<Curve>();
    // Lets one visual the default inspector & correctly reset if desired
    // If we just destroy from the curves list, and it is serialized, it does not work.
    Curve[] cs = this.GetComponents<Curve>();
    for (int i = 0; i < cs.Length; i++)
    {
      DestroyImmediate(cs[i]);
    }
  }


  public void RemoveCurve(Curve curve)
  {
    if (Curves.Contains(curve))
    {
      curve.setPointDelegate -= SetPointHandler;
      curve.addPointAtEndDelegate -= AddPointAtEndHandler;
      curve.removePointDelegate -= RemovePointHandler;
      Curves.Remove(curve);
    }
  }
  public void AddPreConfiguredCurve(Curve curve)
  {
    Curves.Add(curve);
    curve.setPointDelegate += SetPointHandler;
    curve.addPointAtEndDelegate += AddPointAtEndHandler;
    curve.removePointDelegate += RemovePointHandler;
    CalculateTotalTime();
    CalculateTotalLength();
  }
  public void AddCurve(Curve curve)
  {
    Curves.Add(curve);
    if (autoJoinCurves && CurveCount > 1)
    {
      // If we're looping, need to remove the last point of the last curve
      if (Loop)
      {
        Curves[CurveCount - 2].RemovePoint(Curves[CurveCount - 2].ControlPointCount - 1);
        // if it was a hermite spline, and we removed 1 point from it's 3 points, we need to add another one or else it's just a line.
        if (Curves[CurveCount - 2].ControlPointCount == 2 && Curves[CurveCount - 2] is HermiteSpline)
        {
          // so re-add a point at it's end.
          Curves[CurveCount - 2].addPointAtEndDelegate -= AddPointAtEndHandler;
          Curves[CurveCount - 2].AddPointAtEnd();
          Curves[CurveCount - 2].addPointAtEndDelegate += AddPointAtEndHandler;
        }
        else if (curve is Line)
        {
          // If it's a line and we're looping, add an extra point (otherwise the line is just the looped section)
          // But don't do this if the previous line is a hermite curve. (just make a straight section at the end)
          curve.AddPointAtEnd();
        }
      }
      // Get point to join curves at (end point of previous curve)
      Vector3 joinPoint = Curves[CurveCount - 2].GetControlPoint(Curves[CurveCount - 2].ControlPointCount - 1);
      // Join the new curve at that point
      curve.SetPoint(0, joinPoint);
      // Set other new curves points by an offset so they are easily selectable.
      for (int i = 1; i < curve.ControlPointCount; i++)
      {
        curve.SetPoint(i, joinPoint + new Vector3(0.5f * i, 0f, 0f));
      }
      // if we're looping, set the last point of the last curve to the first point of the first curve.
      if (Loop)
      {
        Curves[CurveCount - 1].SetPoint(Curves[CurveCount - 1].ControlPointCount - 1, Curves[0].GetControlPoint(0));
      }
    }
    curve.setPointDelegate += SetPointHandler;
    curve.addPointAtEndDelegate += AddPointAtEndHandler;
    curve.removePointDelegate += RemovePointHandler;
    CalculateTotalTime();
    CalculateTotalLength();
  }

  public void SetPointHandler(int index, Vector3 point, Curve curve)
  {
    int curveIndex = Curves.IndexOf(curve);
    if (index == 0)
    {
      // if it's not the first curve
      if (curveIndex > 0)
      {
        // set previous curves last point to the new point.
        Curves[curveIndex - 1].SetPoint(Curves[curveIndex - 1].ControlPointCount - 1, point);
      }
      // if we're looping, and it's the first curve & index
      else if (Loop && curveIndex == 0)
      {
        // Set the last curves last point to the new point
        Curves[CurveCount - 1].SetPoint(Curves[CurveCount - 1].ControlPointCount - 1, point);
      }
    }
    // As the last curve selector gets drawn after the first curve, it's possible
    // to select it as well, so we are handling that
    // if we're looping, and it's the last curve's last point
    else if (Loop && curveIndex == CurveCount - 1 && index == Curves[CurveCount - 1].ControlPointCount - 1)
    {
      // change the first curves first point to the new point
      // this will call the delegate again & overflow if we dont unregister, set the point, then register again.
      Curves[0].setPointDelegate -= SetPointHandler;
      Curves[0].SetPoint(0, point);
      Curves[0].setPointDelegate += SetPointHandler;
    }
    CalculateTotalLength();
  }

  public void AddPointAtEndHandler(Vector3 point, Curve curve)
  {
    // index of curve where point was added.
    int curveIndex = Curves.IndexOf(curve);
    int nextCurveIndex = curveIndex + 1;
    if (nextCurveIndex > CurveCount - 1 && Loop)
    {
      // Only need to fix the loop when it's the last curve & it loops.
      // The point is already added after the loop point.
      // so we need to adjust the last, and 2nd last points of the last curve.
      int count = Curves[curveIndex].ControlPointCount;
      // set the last point to the 2nd last point so it still loops.
      Curves[curveIndex].SetPoint(count - 1, Curves[curveIndex].GetControlPoint(count - 2));
      // the last "good" point, ie has nothing to do with the looping, using it as the vector from which to offset.
      Vector3 previousPoint = Curves[curveIndex].GetControlPoint(count - 3);
      previousPoint.x += 0.5f;
      Curves[curveIndex].SetPoint(count - 2, previousPoint);
    }
    else if (nextCurveIndex < CurveCount)
    {
      // Set the next curves first point to the point.
      Curves[nextCurveIndex].SetPoint(0, point);
    }
    // Dont need to do anything special for a non-loop last-curve end point. just re-calc length.
    CalculateTotalLength();
  }

  public void RemovePointHandler(int index, Curve curve)
  {
    int curveIndex = Curves.IndexOf(curve);
    // if it's the first point
    if (index == 0)
    {
      // get the previous curve
      int prevCurve = curveIndex - 1;
      if (Loop && prevCurve < 0)
      {
        // loop prev curve if we're looping
        prevCurve = CurveCount - 1;
      }
      // set the previous curves last point, to the curves new first point.
      if (prevCurve >= 0)
      {
        Curves[prevCurve].SetPoint(Curves[prevCurve].ControlPointCount - 1, Curves[curveIndex].GetControlPoint(0));
      }
    }
    else if (index == Curves[curveIndex].ControlPointCount)
    {
      // if it's the last point. (not easily selectable in-editor other than the looped point location, but possible to remove still)
      // get the next curve
      int nextCurve = curveIndex + 1;
      if (Loop && nextCurve == CurveCount)
      {
        nextCurve = 0;
      }
      // set the next curves first point, to the curves new last point.
      if (nextCurve < CurveCount)
      {
        Curves[nextCurve].SetPoint(0, Curves[curveIndex].GetControlPoint(Curves[curveIndex].ControlPointCount - 1));
      }
    }
  }

  private void CalculateTotalTime()
  {
    float total = 0.0f;
    foreach (Curve c in Curves)
    {
      total += c.TotalTime;
    }
    TotalTime = total;
  }

  private void CalculateTotalLength()
  {
    float total = 0.0f;
    foreach (Curve c in Curves)
    {
      total += c.Length;
    }
    Length = total;
  }

  private Tuple<int, float> GetIndexTimeFromTotalTime(float time)
  {
    if (time >= totalTime)
    {
      return new Tuple<int, float>(CurveCount - 1, Curves[CurveCount - 1].TotalTime);
    }
    else
    {
      float timeBeforeCurve = 0.0f;
      for (int i = 0; i < CurveCount; i++)
      {
        if (timeBeforeCurve + Curves[i].TotalTime >= time)
        {
          return new Tuple<int, float>(i, time - timeBeforeCurve);
        }
        else
        {
          timeBeforeCurve += Curves[i].TotalTime;
        }
      }
    }
    return new Tuple<int, float>(CurveCount - 1, Curves[CurveCount - 1].TotalTime);
  }
  public Vector3 GetPoint(float time)
  {
    if (time > TotalTime)
    {
      time = totalTime;
    }
    // Get the curve index in the multi-curve curve list
    // and the time on that curve for the passed in time.
    // as a tuple: Item1: curve index, Item2: time on that curve
    Tuple<int, float> curveTuple = GetIndexTimeFromTotalTime(time);
    // Return that curve's get point value.
    return Curves[curveTuple.Item1].GetPoint(curveTuple.Item2);
  }

  public Vector3 GetPointWorld(float time)
  {
    return transform.TransformPoint(GetPoint(time));
  }

  public Vector3 GetTangent(float time)
  {
    if (time > TotalTime)
    {
      time = totalTime;
    }
    Tuple<int, float> curveTuple = GetIndexTimeFromTotalTime(time);
    return Curves[curveTuple.Item1].GetTangent(curveTuple.Item2);
  }

  public Vector3 GetTangentWorld(float time)
  {
    return this.transform.TransformDirection(GetTangent(time));
  }

  public List<TimedPoint> GetClosestTimesAtPoint(Vector3 worldPos)
  {
    Vector3 local = transform.InverseTransformPoint(worldPos);
    List<TimedPoint> closestPoints = new List<TimedPoint>();
    float prevCurveTime = 0.0f;
    foreach (Curve c in Curves)
    {
      TimedPoint tp = c.GetClosestTimeAtPoint(worldPos);
      TimedPoint adjusted = new TimedPoint(tp.Point, tp.Time + prevCurveTime);
      closestPoints.Add(adjusted);
      prevCurveTime += c.TotalTime;
    }
    return closestPoints;
  }

  public TimedPoint GetClosestTimeAtPoint(Vector3 worldPos)
  {
    Vector3 local = transform.InverseTransformPoint(worldPos);
    List<TimedPoint> closestPoints = GetClosestTimesAtPoint(worldPos);
    TimedPoint closest = closestPoints[0];
    float minDist = Mathf.Infinity;
    foreach (TimedPoint t in closestPoints)
    {
      float d = Vector3.Distance(local, t.Point);
      if (d < minDist)
      {
        closest = t;
        minDist = d;
      }
    }
    return closest;
  }

  public TimedPoint GetClosestTimeAtPointWorld(Vector3 worldPos)
  {
    TimedPoint tp = GetClosestTimeAtPoint(worldPos);
    TimedPoint worldTP = new TimedPoint(this.transform.TransformPoint(tp.Point), tp.Time);
    return worldTP;
  }
}

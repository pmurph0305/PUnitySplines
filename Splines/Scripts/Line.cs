using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : Curve
{

  public override void Reset()
  {
    this.LengthTimes = new List<float>() {
      0f, 1f
    };
    this.points = new List<Vector3>(){
      new Vector3(1,0,0),
      new Vector3(2,0,0)
    };
    this.Length = 1.0f;
  }

  private int GetIndexBeforeTime(float time)
  {
    // get the index right before time.
    for (int c = 0; c < ControlPointCount; c++)
    {
      if (GetTimeAtIndex(c) > time)
      {
        // the index before the time param.
        return c - 1;
      }
    }
    // time > max return 2nd last point
    return ControlPointCount - 2;
  }

  private float GetTimeFactorForIndex(int index, float time)
  {
    float iTime = GetTimeAtIndex(index);
    // Debug.Log(iTime);
    return (time - iTime) / (GetTimeAtIndex(index + 1) - iTime);
  }

  public override Vector3 GetPoint(float time)
  {
    // Debug.Log("1." + time);
    time = Mathf.Clamp(time, 0.0f, TotalTime);
    // Debug.Log("2." + time);
    int i = GetIndexBeforeTime(time);
    // use i if the time is over the max.
    Vector3 p0 = GetControlPoint(i);
    Vector3 p1 = GetControlPoint(i + 1);
    float t = GetTimeFactorForIndex(i, time);
    // Debug.Log(t + " : " + p0 + " : " + p1 + " : " + time + " : " + TotalTime);
    return p0 + t * (p1 - p0);
  }

  public override Vector3 GetTangent(float time)
  {
    time = Mathf.Clamp(time, 0, TotalTime);
    int i = GetIndexBeforeTime(time);
    return (GetControlPoint(i + 1) - GetControlPoint(i)).normalized;
  }

  public override void CalculateLength()
  {
    List<float> distances = new List<float>();
    float distance = 0;
    distances.Add(distance);
    for (int i = 0; i < ControlPointCount - 1; i++)
    {
      distance += Vector3.Distance(GetControlPoint(i), GetControlPoint(i + 1));
      distances.Add(distance);
    }
    LengthTimes = distances;
    Length = distance;
  }

  private bool IsBetweenPoints(Vector3 p, Vector3 p0, Vector3 p1)
  {
    // Check if its between or equal to the x and y and z coords.
    if (((p.x >= p0.x && p.x <= p1.x) || (p.x <= p0.x && p.x >= p1.x))
    && ((p.y >= p0.y && p.y <= p1.y) || (p.y <= p0.y && p.y >= p1.y))
    && ((p.z >= p0.z && p.z <= p1.z) || (p.z <= p0.z && p.z >= p1.z)))
    {
      return true;
    }
    return false;
  }

  public override List<TimedPoint> GetClosestTimesAtPoint(Vector3 worldPos)
  {
    List<TimedPoint> tps = new List<TimedPoint>();
    Vector3 localPos = transform.InverseTransformPoint(worldPos);
    // Go through each line segment.
    for (int i = 0; i < ControlPointCount - 1; i++)
    {
      // Line formed from point i to i+1 as a a vector
      Vector3 lineVector = points[i + 1] - points[i];
      // Normalized line as a vector.
      Vector3 normalizedVector = lineVector.normalized;
      // Vector from start of the line to the point
      Vector3 vectorToPoint = localPos - points[i];
      // Find the magnitude of projecting the vector to the point on the normalized vector of the line.
      float dist = Vector3.Dot(vectorToPoint, normalizedVector);
      // Calculate the closest point.
      Vector3 closest = points[i] + normalizedVector * dist;
      // if it contains the closest point, calculate the time and add.
      if (IsBetweenPoints(closest, points[i], points[i + 1]))
      {
        float time = GetTimeAtIndex(i) + (GetTimeAtIndex(i + 1) - GetTimeAtIndex(i)) *
                    ((closest - points[i]).magnitude / lineVector.magnitude);
        tps.Add(new TimedPoint(closest, time));
      }
      else
      {
        // add the closest control point for this segment.
        if (Vector3.Distance(localPos, points[i]) < Vector3.Distance(localPos, points[i + 1]))
        {
          tps.Add(new TimedPoint(points[i], GetTimeAtIndex(i)));
        }
        else
        {
          tps.Add(new TimedPoint(points[i + 1], GetTimeAtIndex(i + 1)));
        }
      }
    }
    return tps;
  }

  public override TimedPoint GetClosestTimeAtPoint(Vector3 worldPos)
  {
    Vector3 localPos = transform.InverseTransformPoint(worldPos);
    // Get the closest point for each line segment
    List<TimedPoint> tps = GetClosestTimesAtPoint(worldPos);
    TimedPoint timedPoint = tps[0];
    float minDistance = Mathf.Infinity;
    // Go through all the points and find the one with the minimum distance.
    foreach (TimedPoint tp in tps)
    {
      float d = Vector3.Distance(tp.Point, localPos);
      if (d < minDistance)
      {
        minDistance = d;
        timedPoint = tp;
      }
    }
    return timedPoint;
  }
}

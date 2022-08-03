using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HermiteSpline : Curve
{
  [SerializeField]
  private float tension = 0.5f;   // 0.5 is a catmull-rom
  public float Tension
  {
    get { return tension; }
    set { tension = value; }
  }

  public override void Reset()
  {
    loop = false;
    points = new List<Vector3>(){
          new Vector3(1f, 0f, 0f),
          new Vector3(2f, 0f, 0f),
        new Vector3(3f, 0f, 0f),

      };
    CalculateApproximateLength();
  }

  public override Vector3 GetPoint(float time)
  {
    time = Mathf.Clamp(time, 0, TotalTime);
    // get the index before the time param.
    int index = GetIndexBeforeTime(time);
    // get the time factor (0 -> 1) between index and index + 1
    time = GetTimeFactorForIndex(index, time);
    // get the points for the index
    Vector3[] ps = GetPointsForIndex(index);
    // get the point at time
    return Hermite.GetPoint(ps[0], ps[1], ps[2], ps[3], time, tension);
  }

  public override Vector3 GetTangent(float time)
  {
    time = Mathf.Clamp(time, 0, TotalTime);
    // index before time param
    int index = GetIndexBeforeTime(time);
    // time factor
    time = GetTimeFactorForIndex(index, time);
    // points for index
    Vector3[] ps = GetPointsForIndex(index);
    // return a normalized tangent
    return Hermite.GetTangent(ps[0], ps[1], ps[2], ps[3], time, tension).normalized;
  }

  protected float GetTimeFactorForIndex(int index, float time)
  {
    float iTime = GetTimeAtIndex(index);
    // time at the next point is
    float nTime = GetTimeAtIndex(index + 1);
    // proportion of total time in index -> c range.
    return (time - iTime) / (nTime - iTime);
  }
  private Vector3[] GetPointsForIndex(int index)
  {
    Vector3 p0, p1, p2, p3;
    if (index == 0)
    {
      if (loop)
      {
        p0 = points[ControlPointCount - 2];
      }
      else
      {
        // at 0 we use the first point as p0.
        p0 = points[index];
      }
    }
    else
    {
      p0 = points[index - 1];
    }
    // p1 is always the index
    p1 = points[index];
    // p2 is always the next index;
    p2 = points[index + 1];
    if (index + 2 > ControlPointCount - 1)
    {
      if (loop)
      {
        p3 = points[1];
      }
      else
      {
        // at the end we duplicate the last point
        p3 = points[index + 1];
      }
    }
    else
    {
      p3 = points[index + 2];
    }
    return new Vector3[]{
        p0,p1,p2,p3
    };
  }

  protected int GetIndexBeforeTime(float time)
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
    // time > totaltime return 2nd last point..
    return ControlPointCount - 2;
  }

  public override void CalculateLength()
  {
    CalculateApproximateLength();
  }
  // Also calculates Timed distances.
  // As length and timed distances can be done together.
  // Should seperate into different functions?
  // Or rename this function?
  private void CalculateApproximateLength()
  {
    // we want to internally not use distance times while calculating the distances.
    bool isUsingDistanceTime = TimeFromLength;
    TimeFromLength = false;
    List<float> distances = new List<float>();
    float calcedLength = 0;
    distances.Add(0.0f);
    // number of segments between control points to use
    int segCount = 1000;
    // time between control points normally.
    float segTime = (1 / (float)(ControlPointCount - 1)) * TotalTime;
    for (int i = 0; i < ControlPointCount - 1; i++)
    {
      float t0 = (i / (float)(ControlPointCount - 1)) * TotalTime;
      Vector3 p0 = GetPoint(t0);
      for (int j = 1; j <= segCount; j++)
      {
        // time of previous ie: index + segTime * 1/segCount for first
        Vector3 p1 = GetPoint(t0 + segTime * j / (float)segCount);
        calcedLength += Vector3.Distance(p0, p1);
        p0 = p1;
      }
      distances.Add(calcedLength);
    }
    Length = calcedLength;
    LengthTimes = distances;
    // set time from length to what it was previous to calculating length.
    TimeFromLength = isUsingDistanceTime;
  }
}
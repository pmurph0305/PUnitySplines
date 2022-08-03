using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicBezier : Curve
{
  public override bool Loop
  {
    set
    {
      loop = value;
      if (value == true)
      {
        points[ControlPointCount - 1] = points[0];
      }
      else
      {
        Vector3 shiftPt = points[ControlPointCount - 1];
        shiftPt.x += 0.5f;
        points[ControlPointCount - 1] = shiftPt;
      }
      CalculateLength();
    }
  }
  public override void AddPointAtEnd()
  {
    List<Vector3> newPoints = new List<Vector3>();
    foreach (Vector3 p in points)
    {
      newPoints.Add(p);
    }
    for (int i = 1; i <= 3; i++)
    {
      Vector3 p = points[ControlPointCount - 1];
      p.x += 0.5f * i;
      newPoints.Add(p);
    }
    points = newPoints;
  }

  public override void RemovePoint(int index)
  {
    if (CurveCount == 1)
    {
      return;
    }
    int startIndex = 0;
    if (index == ControlPointCount - 1)
    {
      startIndex = ControlPointCount - 4;
    }
    else
    {
      startIndex = (index / 3) * 3;
    }
    Debug.Log("start index:" + startIndex);
    List<Vector3> newPoints = new List<Vector3>();
    for (int i = 0; i < startIndex; i++)
    {
      newPoints.Add(points[i]);
    }
    if (startIndex != 0)
    {
      newPoints.Add(points[startIndex]);
    }
    else
    {
      newPoints.Add(points[3]);
    }
    for (int i = startIndex + 4; i < ControlPointCount; i++)
    {
      newPoints.Add(points[i]);
    }
    points = newPoints;
  }

  public override void Reset()
  {
    loop = false;
    points = new List<Vector3>(){
      new Vector3(1f,0f,0f),
       new Vector3(2f,0f,2f),
        new Vector3(3f,0f,2f),
         new Vector3(4f,0f,0f),
    };
    CalculateLength();
  }

  public int CurveCount
  {
    get
    {
      return (points.Count - 1) / 3;
    }
  }
  public override Vector3 GetPoint(float time)
  {
    int i = GetIndexForTime(time);
    time = GetTimeFactorForIndex(i, time);
    return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], time);
  }

  private float GetTimeFactorForIndex(int index, float time)
  {
    if (time >= TotalTime)
    {
      return 1.0f;
    }
    else
    {
      return ((time / TotalTime) * CurveCount) - (index / 3);
    }
  }
  private int GetIndexForTime(float time)
  {
    if (time >= TotalTime)
    {
      return points.Count - 4;
    }
    else
    {
      return (int)((time / TotalTime) * CurveCount) * 3;
    }
  }

  public override Vector3 GetTangent(float time)
  {
    int i = GetIndexForTime(time);
    time = GetTimeFactorForIndex(i, time);
    return Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], time).normalized;
  }

  public override void CalculateLength()
  {
    float calcedLength = 0;
    int segCount = 2 * (ControlPointCount - 1);
    float segTime = TotalTime / segCount;
    Vector3 p0 = GetPoint(0);
    for (int i = 0; i <= segCount; i++)
    {
      Vector3 p1 = GetPoint(i * segTime);
      calcedLength += Vector3.Distance(p0, p1);
      p0 = p1;
    }
    Length = calcedLength;
  }
}

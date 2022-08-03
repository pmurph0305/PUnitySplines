using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HermiteSplineThick : HermiteSpline
{
  [SerializeField] protected List<float> thicknesses;

  public override void Reset()
  {
    base.Reset();
    thicknesses = new List<float>() { 1f, 1f, 1f };
  }
  public override void AddPointAtEnd()
  {
    base.AddPointAtEnd();
    thicknesses.Add(1f);
  }

  public override void AddPointAtIndex(int index)
  {
    base.AddPointAtIndex(index);
    thicknesses.Insert(index, 1f);
  }

  public override void RemovePoint(int index)
  {
    base.RemovePoint(index);
    thicknesses.RemoveAt(index);
  }

  public virtual void SetThickness(int index, float thickness)
  {
    thicknesses[index] = thickness;
  }

  public virtual float GetControlPointThickness(int index)
  {
    return thicknesses[index];
  }

  public virtual float GetThickness(float time)
  {
    time = Mathf.Clamp(time, 0, TotalTime);
    // get the index before the time param.
    int index = GetIndexBeforeTime(time);
    // get the time factor (0 -> 1) between index and index + 1
    time = GetTimeFactorForIndex(index, time);
    // get the points for the index
    return Mathf.Lerp(thicknesses[index], thicknesses[index + 1], time);
  }


  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    for (float t = 0; t < Length; t++)
    {
      Vector3 tangent = GetTangentWorld(t);
      float thick = GetThickness(t);
      Vector3 cross = Vector3.Cross(Vector3.forward, tangent);
      Vector3 p = GetPointWorld(t);
      Gizmos.DrawLine(p - cross * thick, p + cross * thick);
    }
  }
}

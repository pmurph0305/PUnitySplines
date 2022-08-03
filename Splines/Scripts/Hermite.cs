using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Actual a cardinal spline since we don't manually control the tangents like a hermite spline.
// https://www.cubic.org/docs/hermite.htm
public static class Hermite
{

  public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float time, float tension)
  {
    float t2 = time * time;
    float t3 = time * time * time;
    // basis functions
    float H1 = 2 * t3 - 3 * t2 + 1;
    float H2 = -2 * t3 + 3 * t2;
    float H3 = t3 - 2 * t2 + time;
    float H4 = t3 - t2;
    Vector3 T1 = tension * (p2 - p0);
    Vector3 T2 = tension * (p3 - p1);
    return H1 * p1 + H2 * p2 + H3 * T1 + H4 * T2;
  }
  public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float time, float tension)
  {
    float t2 = time * time;
    float H1 = 6 * t2 - 6 * time;
    float H2 = -6 * t2 + 6 * time;
    float H3 = 3 * t2 - 4 * time + 1;
    float H4 = 3 * t2 - 2 * time;
    Vector3 T1 = tension * (p2 - p0);
    Vector3 T2 = tension * (p3 - p1);
    return H1 * p1 + H2 * p2 + H3 * T1 + H4 * T2;
  }
}
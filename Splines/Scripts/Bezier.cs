using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{

  // Quadratic Bezier Curves (3 points);
  public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float time)
  {
    //return Vector3.Lerp(Vector3.Lerp(p0, p1, time), Vector3.Lerp(p1, p2, time), time);
    // alternatively the curve is p(t) = (1-t)^2 * P0  + 2(1-t)t*P1 + t^2P2
    time = Mathf.Clamp01(time);
    float oneMinusTime = 1.0f - time;
    return oneMinusTime * oneMinusTime * p0 + 2 * oneMinusTime * time * p1 + time * time * p2;
  }

  public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float time)
  {
    // B'(t) = 2(1-t)(P1-P0) + 2t(P2-P1)
    return 2 * (1.0f - time) * (p1 - p0) + 2 * time * (p2 - p1);
  }

  // Cubic Linear Curves (4 points);
  public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float time)
  {

    //the curve is  p(t) = (1 - t)^3 P0 + 3 (1 - t)^2 t P1 + 3 (1 - t) t^2 P2 + t^3 P3
    time = Mathf.Clamp01(time);
    float oneMinusTime = 1.0f - time;
    return Mathf.Pow(oneMinusTime, 3) * p0 +
            3.0f * Mathf.Pow(oneMinusTime, 2) * time * p1 +
            3.0f * oneMinusTime * Mathf.Pow(time, 2) * p2 +
            Mathf.Pow(time, 3) * p3;
  }

  public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float time)
  {
    // P'(t) = 3*(1-t)^2*(p1-p0) + 6*(1-t)*t*(p2-p1) + 3t^2(p3 - p2);
    time = Mathf.Clamp01(time);
    float oneMinusTime = 1.0f - time;
    return 3.0f * oneMinusTime * oneMinusTime * (p1 - p0) +
           6.0f * oneMinusTime * time * (p2 - p1) +
           3.0f * time * time * (p3 - p2);
  }

}

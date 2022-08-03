using UnityEngine;
public struct TimedPoint
{
  public Vector3 Point { get; }
  public float Time { get; }
  public TimedPoint(Vector3 point, float time)
  {
    Point = point;
    Time = time;
  }

  public override string ToString()
  {
    return "Time:" + Time + " Point:" + Point;
  }
}
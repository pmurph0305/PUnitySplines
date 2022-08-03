using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITravelableCurve
{
  float Length { get; }
  float TotalTime { get; }
  Vector3 GetPoint(float time);
  Vector3 GetPointWorld(float time);
  Vector3 GetTangent(float time);
  Vector3 GetTangentWorld(float time);
  List<TimedPoint> GetClosestTimesAtPoint(Vector3 worldPos);
  TimedPoint GetClosestTimeAtPoint(Vector3 worldPos);
  TimedPoint GetClosestTimeAtPointWorld(Vector3 worldPos);
}

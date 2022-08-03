using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(CubicBezier))]
public class CubicBezierInspector : CurveInspector
{
  private CubicBezier cubicBezier;


  public override void OnInspectorGUI()
  {
    tempObj = (GameObject)EditorGUILayout.ObjectField("Test Closest Point To:", tempObj, typeof(GameObject), true);
    cubicBezier = target as CubicBezier;
    EditorGUILayout.FloatField("Calced Approx Length", cubicBezier.Length);
    // Tangents UI Toggle.
    EditorGUI.BeginChangeCheck();
    bool tangents = EditorGUILayout.Toggle("Show Tangents", showTangents);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(cubicBezier, "Show tangents");
      EditorUtility.SetDirty(cubicBezier);
      showTangents = tangents;
    }
    // Close / Loop Spline Toggle UI
    EditorGUI.BeginChangeCheck();
    bool loop = EditorGUILayout.Toggle("Loop", cubicBezier.Loop);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(cubicBezier, "Toggle Curve Loop");
      EditorUtility.SetDirty(cubicBezier);
      cubicBezier.Loop = loop;
    }
    // UI for selected point.
    if (selectedPointIndex >= 0 && selectedPointIndex < cubicBezier.ControlPointCount)
    {
      DrawSelectedPointInspector();
    }
    // Add a point at the end of the curve.
    if (GUILayout.Button("Add Curve At End"))
    {
      Undo.RecordObject(cubicBezier, "Add End Curve");
      EditorUtility.SetDirty(cubicBezier);
      cubicBezier.AddPointAtEnd();
    }
    // Remove the selected point.
    if (GUILayout.Button("Remove Selected Curve"))
    {
      Undo.RecordObject(cubicBezier, "Remove point");
      EditorUtility.SetDirty(cubicBezier);
      cubicBezier.RemovePoint(selectedPointIndex);
    }
  }
  public override void DrawInterpolatedCurve()
  {
    stepsPerPoint = 5;
    cubicBezier = target as CubicBezier;
    // number of steps to interpolate on curve.
    int curveSteps = stepsPerPoint * (cubicBezier.ControlPointCount - 1);
    // Interpolate curve
    Vector3 ip0 = transform.TransformPoint(cubicBezier.GetControlPoint(0));
    for (int i = 0; i <= curveSteps; i++)
    {
      Vector3 ip1 = transform.TransformPoint(cubicBezier.GetPoint(((float)i / (float)curveSteps) * cubicBezier.TotalTime));
      // draw the tagent for point ip1.
      if (showTangents)
      {
        DrawTangent(ip1, (float)i / (float)curveSteps * curve.TotalTime);
      }
      // Draw the line between interpolated points
      Handles.color = handleColors[2];
      Handles.DrawLine(ip0, ip1);
      ip0 = ip1;
    }
  }
}


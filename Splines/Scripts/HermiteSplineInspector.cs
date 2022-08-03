using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(HermiteSpline), true)]
public class HermiteSplineInspector : CurveInspector
{
  private HermiteSpline spline;

  public override void OnInspectorGUI()
  {
    spline = target as HermiteSpline;
    // Tension UI Float Field.
    EditorGUI.BeginChangeCheck();
    float tension = EditorGUILayout.FloatField("Tension", spline.Tension);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(spline, "Change tension");
      EditorUtility.SetDirty(spline);
      spline.Tension = tension;
    }

    base.OnInspectorGUI();
  }

  public override void DrawInterpolatedCurve()
  {
    stepsPerPoint = 20;
    spline = target as HermiteSpline;
    // number of steps to interpolate on curve.
    int curveSteps = stepsPerPoint * (spline.ControlPointCount - 1);
    // Interpolate curve
    Vector3 ip0 = transform.TransformPoint(spline.GetControlPoint(0));
    for (int i = 0; i <= curveSteps; i++)
    {
      Vector3 ip1 = transform.TransformPoint(spline.GetPoint(((float)i / (float)curveSteps) * spline.TotalTime));
      // draw the tagent for point ip1.
      if (showTangents)
      {
        DrawTangent(ip1, (float)i / (float)curveSteps * spline.TotalTime);
      }
      // Draw the line between interpolated points
      Handles.color = handleColors[2];
      Handles.DrawLine(ip0, ip1);
      ip0 = ip1;
    }
  }

}

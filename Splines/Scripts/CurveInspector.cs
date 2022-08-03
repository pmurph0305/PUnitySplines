using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Curve), true)]
public class CurveInspector : Editor
{
  public GameObject tempObj;
  protected Curve curve;
  protected Transform transform;
  protected Quaternion rotation;
  protected float tangentScale = 0.2f;
  protected int selectedPointIndex = -1;

  protected bool showTangents = false;
  protected static Color[] handleColors = {
    Color.blue, // Selectable points
    Color.gray, // Straight lines between points.
    Color.yellow, // Curved spline line
    Color.cyan, // Selected point
    Color.green, // Tagent Lines.
    Color.red // Tagent Point lines.
  };

  protected float handleSize = 0.1f;
  protected float pickSize = 0.1f;

  protected int stepsPerPoint = 1;
  public virtual void OnSceneGUI()
  {
    curve = target as Curve;
    transform = curve.transform;
    rotation = curve.transform.rotation;
    if (Tools.pivotRotation == PivotRotation.Global)
    {
      rotation = Quaternion.identity;
    }
    // Display curve points
    Vector3 p0 = DisplayCurvePoint(0);
    if (curve.Loop)
    {
      Vector3 p1 = transform.TransformPoint(curve.GetControlPoint(curve.ControlPointCount - 2));
      Handles.color = handleColors[1];
      Handles.DrawLine(p0, p1);
    }
    int totalPoints = curve.Loop ? curve.ControlPointCount - 1 : curve.ControlPointCount;
    for (int i = 1; i < totalPoints; i++)
    {
      Vector3 p1 = DisplayCurvePoint(i);
      // Draw line between previous and current point.
      Handles.color = handleColors[1];
      Handles.DrawLine(p0, p1);
      p0 = p1;
    }
    DrawInterpolatedCurve();

    if (tempObj != null)
    {
      TimedPoint t = curve.GetClosestTimeAtPoint(tempObj.transform.position);
      Handles.color = Color.magenta;
      Vector3 p = transform.TransformPoint(t.Point);
      Handles.DrawLine(tempObj.transform.position, p);
      Handles.color = Color.black;
      Handles.DrawLine(p, p + Vector3.down);
    }
  }

  public override void OnInspectorGUI()
  {
    curve = target as Curve;
    tempObj = (GameObject)EditorGUILayout.ObjectField("Test Closest Point To:", tempObj, typeof(GameObject), true);
    // Time from distance field.
    EditorGUI.BeginChangeCheck();
    bool timeFromLength = EditorGUILayout.Toggle("Time from Length", curve.TimeFromLength);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(curve, "Change time from distance");
      EditorUtility.SetDirty(curve);
      curve.TimeFromLength = timeFromLength;
    }
    // Total time field.
    EditorGUI.BeginChangeCheck();
    float totalTime = EditorGUILayout.FloatField("Total Time", curve.TotalTime);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(curve, "Change Total Time");
      EditorUtility.SetDirty(curve);
      curve.TotalTime = totalTime;
    }
    // Calculated Length Float Uneditable Field.
    EditorGUILayout.FloatField("Calced Approx Length", curve.Length);
    // Tangents UI Toggle.
    EditorGUI.BeginChangeCheck();
    bool tangents = EditorGUILayout.Toggle("Show Tangents", showTangents);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(curve, "Show tangents");
      EditorUtility.SetDirty(curve);
      showTangents = tangents;
    }
    // Close / Loop Spline Toggle UI
    EditorGUI.BeginChangeCheck();
    bool loop = EditorGUILayout.Toggle("Loop", curve.Loop);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(curve, "Toggle Curve Loop");
      EditorUtility.SetDirty(curve);
      curve.Loop = loop;
    }
    // UI for selected point.
    if (selectedPointIndex >= 0 && selectedPointIndex < curve.ControlPointCount)
    {
      DrawSelectedPointInspector();
    }
    // Add a point after the selected point.
    if (GUILayout.Button("Add Point"))
    {
      Undo.RecordObject(curve, "Add Point");
      EditorUtility.SetDirty(curve);
      curve.AddPointAtIndex(selectedPointIndex);
    }
    // Add a point at the end of the curve.
    if (GUILayout.Button("Add Point At End"))
    {
      Undo.RecordObject(curve, "Add End point");
      EditorUtility.SetDirty(curve);
      curve.AddPointAtEnd();
    }
    // Remove the selected point.
    if (GUILayout.Button("Remove Selected Point"))
    {
      Undo.RecordObject(curve, "Remove point");
      EditorUtility.SetDirty(curve);
      curve.RemovePoint(selectedPointIndex);
    }
  }

  public virtual Vector3 DisplayCurvePoint(int index)
  {
    Vector3 point = transform.TransformPoint(curve.GetControlPoint(index));
    Handles.color = index == selectedPointIndex ? handleColors[3] : handleColors[0];
    // Size values. size is a fixed pixel size
    float size = HandleUtility.GetHandleSize(point);
    // if it's the start point draw it twice as large.
    float startSize = index == 0 ? 2f : 1.0f;
    // if it's the selected index (and not the already larger start index, make it larger) 
    float selectedSize = (selectedPointIndex == index && index != 0) ? 1.5f : 1.0f;
    if (Handles.Button(point, rotation, size * startSize * handleSize * selectedSize, size * startSize * pickSize, Handles.DotHandleCap))
    {
      // Set the index and repaint to update the UI
      selectedPointIndex = index;
      Repaint();
    }
    // If this is the selected point, then draw the handles to move it.
    if (selectedPointIndex == index)
    {
      EditorGUI.BeginChangeCheck();
      point = Handles.DoPositionHandle(point, rotation);
      // Move the point if it's changed.
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(curve, "Move Point " + index);
        curve.SetPoint(index, transform.InverseTransformPoint(point));
      }
    }
    return point;
  }

  public virtual void DrawSelectedPointInspector()
  {
    // Selected point UI.
    GUILayout.Label("Selected Point");
    EditorGUI.BeginChangeCheck();
    Vector3 point = EditorGUILayout.Vector3Field("Position", curve.GetControlPoint(selectedPointIndex));
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(curve, "Change Point Inspector");
      EditorUtility.SetDirty(curve);
      curve.SetPoint(selectedPointIndex, point);
    }
  }

  public virtual void DrawInterpolatedCurve()
  {
    // number of steps to interpolate on curve.
    int curveSteps = stepsPerPoint * (curve.ControlPointCount - 1);

    Vector3 ip0 = transform.TransformPoint(curve.GetControlPoint(0));
    for (int i = 1; i <= curveSteps; i++)
    {
      Vector3 ip1 = transform.TransformPoint(curve.GetControlPoint(i));
      if (showTangents)
      {
        DrawTangent(ip1, (float)i / (float)curveSteps * curve.TotalTime);
      }
      Handles.color = handleColors[2];
      Handles.DrawLine(ip0, ip1);
      ip0 = ip1;
    }
  }


  public virtual void DrawTangent(Vector3 point, float time)
  {
    Handles.color = handleColors[4];
    Vector3 tangent = curve.GetTangent(time);
    Handles.DrawLine(point, point + tangent * tangentScale);
    Handles.color = handleColors[5];
    Handles.DrawLine(point, point + Vector3.up * tangentScale);
  }


}

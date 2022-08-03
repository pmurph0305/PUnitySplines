using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MultiCurve))]
public class MultiCurveInspector : Editor
{

  // These make sure delegates are registered/unregistered
  // after entering/exiting play mode
  void OnEnable()
  {
    if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
    {
      multiCurve = (MultiCurve)target;
      if (multiCurve != null)
      {
        multiCurve.CheckRegisterDelegates();
      }
    }
  }

  void OnDisable()
  {
    if (!EditorApplication.isPlaying && !EditorApplication.isPaused)
    {
      multiCurve = (MultiCurve)target;
      if (multiCurve != null)
      {
        multiCurve.UnregisterDelegates();
      }
    }
  }

  private MultiCurve multiCurve;
  private Transform transform;
  private GameObject gameObject;
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    multiCurve = target as MultiCurve;
    transform = multiCurve.transform;
    gameObject = multiCurve.gameObject;
    EditorGUILayout.LabelField("---MultiCurve Custom Inspector---");
    // Uneditable total time & length fields
    EditorGUILayout.FloatField("Total Length", multiCurve.Length);
    EditorGUILayout.FloatField("Total Time", multiCurve.TotalTime);
    // Loop toggle
    EditorGUI.BeginChangeCheck();
    bool loop = EditorGUILayout.Toggle("Loop", multiCurve.Loop);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(multiCurve, "Toggle Multi Curve Loop");
      EditorUtility.SetDirty(multiCurve);
      multiCurve.Loop = loop;
    }
    // Times from length toggle.
    EditorGUI.BeginChangeCheck();
    bool timesFromLength = EditorGUILayout.Toggle("Times from Length", multiCurve.TimesFromLength);
    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(multiCurve, "Toggle Times from Length");
      EditorUtility.SetDirty(multiCurve);
      multiCurve.TimesFromLength = timesFromLength;
    }
    // Add a line.
    if (GUILayout.Button("Add Line"))
    {
      Line line = gameObject.AddComponent<Line>();
      multiCurve.AddCurve(line);
    }
    // Add a hermite spline.
    if (GUILayout.Button("Add Hermite Spline"))
    {
      HermiteSpline hermiteSpline = gameObject.AddComponent<HermiteSpline>();
      multiCurve.AddCurve(hermiteSpline);
    }
    if (GUILayout.Button("Check del"))
    {
      multiCurve.CheckRegisterDelegates();
    }
  }
}

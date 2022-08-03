using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SplineToMesh))]
public class SplineToMeshInspector : Editor
{
  protected SplineToMesh splineToMesh;

  protected Material material;


  // Update is called once per frame
  void Update()
  {

  }

  public void OnSceneGUI()
  {
    splineToMesh = target as SplineToMesh;
  }


  public void OnDestroy()
  {
    if (splineToMesh.curve != null)
    {
      splineToMesh.curve.pointsChangedDelegate -= splineToMesh.OnPointsChangedHandler;
    }
  }
  public override void OnInspectorGUI()
  {
    // base.OnInspectorGUI();
    splineToMesh = target as SplineToMesh;
    if (splineToMesh.curve == null)
    {

      splineToMesh.SetCurve(splineToMesh.GetComponent<Curve>());
    }
    splineToMesh.curve.pointsChangedDelegate -= splineToMesh.OnPointsChangedHandler;
    splineToMesh.curve.pointsChangedDelegate += splineToMesh.OnPointsChangedHandler;

    splineToMesh.AutoGenerate = EditorGUILayout.ToggleLeft("Auto Generate Mesh", splineToMesh.AutoGenerate);
    if (GUILayout.Button("Generate"))
    {
      splineToMesh.GenerateMesh();
    }
    // EditorGUI.BeginChangeCheck();
    splineToMesh.Material = (Material)EditorGUILayout.ObjectField("Material", splineToMesh.Material, typeof(Material), false);
    splineToMesh.PathWidth = EditorGUILayout.FloatField("Path Width:", splineToMesh.PathWidth);
    splineToMesh.FlipNormal = EditorGUILayout.ToggleLeft("Flip Normals", splineToMesh.FlipNormal);
    splineToMesh.FlipUV = EditorGUILayout.ToggleLeft("Flip UVs", splineToMesh.FlipUV);
    splineToMesh.Segments = EditorGUILayout.IntField("Length Segments:", splineToMesh.Segments);
    splineToMesh.WidthSegments = EditorGUILayout.IntField("Width Segments:", splineToMesh.WidthSegments);
    splineToMesh.UseTangents = EditorGUILayout.ToggleLeft("Use Tangents", splineToMesh.UseTangents);
    splineToMesh.SmoothTangents = EditorGUILayout.ToggleLeft("SmoothTangents", splineToMesh.SmoothTangents);
    splineToMesh.DirectionalTilingAmount = EditorGUILayout.FloatField("Directional Tiling", splineToMesh.DirectionalTilingAmount);
    splineToMesh.NormalTilingAmount = EditorGUILayout.FloatField("Normal Tiling", splineToMesh.NormalTilingAmount);
    splineToMesh.WeldDistance = EditorGUILayout.FloatField("Weld Distance", splineToMesh.WeldDistance);
    splineToMesh.WeldDistance = EditorGUILayout.Slider("WeldDistance", splineToMesh.WeldDistance, 0f, 0.1f);
    splineToMesh.WeldAngle = EditorGUILayout.Slider("WeldAngle", splineToMesh.WeldAngle, 0f, 180f);
    splineToMesh.WeldVertices = EditorGUILayout.ToggleLeft("Weld Vertices", splineToMesh.WeldVertices);
    splineToMesh.UVAverageWeldedVerts = EditorGUILayout.ToggleLeft("Smooth Welded UVs", splineToMesh.UVAverageWeldedVerts);
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ReparentCurve))]
public class ReparentCurveInspector : Editor
{

  private ReparentCurve reparent;
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    reparent = target as ReparentCurve;
    if (GUILayout.Button("Move up to parent"))
    {
      Reparent(reparent);
    }
  }

  public void Reparent(ReparentCurve rc)
  {
    MultiCurve mc = rc.transform.GetComponent<MultiCurve>();
    if (mc != null)
    {
      ReparentMultiCurve(mc);
    }
    else
    {
      Curve c = rc.transform.GetComponent<Curve>();
      if (c != null)
      {
        ReparentRegularCurve(c);
      }
    }
  }

  private void ReparentMultiCurve(MultiCurve mc)
  {
    Curve[] curves = mc.transform.GetComponents<Curve>();
    MultiCurve NewMC = mc.transform.parent.gameObject.AddComponent<MultiCurve>();
    // Copy all serialized fields on the original multicurve to the new multicurve.
    EditorUtility.CopySerialized(mc, NewMC);
    foreach (Curve c in curves)
    {
      // Remove the copied curve from copy serialized.
      NewMC.RemoveCurve(c);
      // Reparent the curve
      Curve c2 = ReparentRegularCurve(c);
      // Add it to the multicurve as a preconfigured curve.
      NewMC.AddPreConfiguredCurve(c2);
    }
    // Destroy the old curve.
    if (reparent.DestroyOldCurve)
    {
      // Uses the delay call, otherwise you get inspector errors.
      EditorApplication.delayCall += () => DestroyImmediate(mc);
    }
  }

  private Curve ReparentRegularCurve(Curve c)
  {
    // add a new curve
    Curve c2 = (Curve)c.transform.parent.gameObject.AddComponent(c.GetType());
    // copy the serialized properties of the old curve
    EditorUtility.CopySerialized(c, c2);
    // Transform all the points of the old curve to world space, then to the new parents local space, and set that as that point on the new curve.
    List<Vector3> ctrlPts = new List<Vector3>();
    for (int i = 0; i < c.ControlPointCount; i++)
    {
      c2.SetPoint(i, c.transform.parent.InverseTransformPoint(c.transform.TransformPoint(c.GetControlPoint(i))));
    }
    // for (int i = 0; i < ctrlPts.Count; i++)
    // {
    //   c2.SetPoint(i, ctrlPts[i]);
    // }
    if (reparent.DestroyOldCurve)
    {
      EditorApplication.delayCall += () => DestroyImmediate(c);
    }
    return c2;
  }
}

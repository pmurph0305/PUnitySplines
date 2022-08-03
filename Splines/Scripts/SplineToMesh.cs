using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineToMesh : MonoBehaviour
{
  //TODOs:
  //required fuctionality:
  // 1. UVs. (done?)

  // 2. welding of vertices that go "backwards" / overlap previous vertices..
  // ideas: keep previous added vertices in an array, compare position and direction to see if they overlap


  //3. seperate meshes for the edges, center, and full width with materials for each
  // would help with creating a road with seperate meshes for curbs, line in center, tileable ashphalt texture etc.

  // helpful features:
  // project mesh onto objects below.
  // split paths? (But.. how to properly uv?)

  // additional:
  // 1. interp more around drastic tangent changes and less around straight areas.


  /// <summary>
  /// Curve we are turning into a mesh
  /// </summary>
  public Curve curve { get; private set; }

  /// <summary>
  /// Mesh filter on this gameobject
  /// </summary>
  public MeshFilter meshFilter { get; private set; }

  /// <summary>
  /// Mesh renderer on this game object,
  /// </summary>
  public MeshRenderer meshRenderer { get; private set; }


  public bool autoGenerate = true;
  /// <summary>
  /// should we regenerate the mesh as the parameters change or use the button?
  /// </summary>
  /// <value></value>
  public bool AutoGenerate
  {
    get { return autoGenerate; }
    set
    {
      if (value != autoGenerate)
      {
        autoGenerate = value;
        if (value)
        {
          AutoGenerateMesh();
        }
      }
    }
  }


  private bool flipNormal;
  /// <summary>
  /// Should up be flipped to Vector3.down or not?
  /// </summary>
  public bool FlipNormal
  {
    get { return flipNormal; }
    set
    {
      if (value != flipNormal)
      {
        flipNormal = value;
        AutoGenerateMesh();
      }
    }
  }

  private bool useTangents;
  /// <summary>
  /// Should tangents be used when calculating the direction of each segment?
  /// </summary>
  public bool UseTangents
  {
    get { return useTangents; }
    set
    {
      if (value != useTangents)
      {
        useTangents = value;
        AutoGenerateMesh();
      }
    }
  }

  private bool smoothTangents;
  /// <summary>
  /// Should the tangents (if we are using them) be smoothed?
  /// </summary>
  /// <value></value>
  public bool SmoothTangents
  {
    get { return smoothTangents; }
    set
    {
      if (value != smoothTangents)
      {
        smoothTangents = value;
        AutoGenerateMesh();
      }
    }
  }

  private float pathWidth = 1;
  /// <summary>
  /// Width of the path generated.
  /// </summary>
  /// <value></value>
  public float PathWidth
  {
    get { return pathWidth; }
    set
    {
      if (value <= 0)
      {
        pathWidth = 0;
      }
      else
      {
        if (pathWidth != value)
        {
          pathWidth = value;
          AutoGenerateMesh();
        }
      }
    }
  }

  private int segments = 1;
  /// <summary>
  /// Number of segments for the whole lenght of the curve.
  /// </summary>
  public int Segments
  {
    get { return segments; }
    set
    {
      if (segments != value)
      {
        segments = value;
        AutoGenerateMesh();
      }
    }
  }

  private int widthSegments = 1;
  /// <summary>
  /// Number of division width-wise
  /// </summary>
  /// <value></value>
  public int WidthSegments
  {
    get { return widthSegments; }
    set
    {
      if (widthSegments != value)
      {
        widthSegments = value;
        AutoGenerateMesh();
      }
    }
  }

  private float weldDistance = 0.0f;
  /// <summary>
  /// Min distance to weld path-directional companion vertices.
  /// </summary>
  public float WeldDistance
  {
    get { return weldDistance; }
    set
    {
      if (weldDistance != value)
      {
        weldDistance = value;
        AutoGenerateMesh();
      }
    }
  }

  private bool weldVertices = false;
  /// <summary>
  /// Should vertices be welded?
  /// </summary>
  public bool WeldVertices
  {
    get { return weldVertices; }
    set
    {
      if (weldVertices != value)
      {
        weldVertices = value;
        AutoGenerateMesh();
      }
    }
  }

  public float weldAngle = 90f;
  public float WeldAngle
  {
    get { return weldAngle; }
    set
    {
      if (weldAngle != value)
      {
        weldAngle = value;
        AutoGenerateMesh();
      }
    }
  }

  private bool uvAverageWeldedVerts = false;
  /// <summary>
  /// Should vertices that are welded have their UV's averaged as well? (If false, just uses the base UV position a vertex was welded with)
  /// </summary>
  public bool UVAverageWeldedVerts
  {
    get { return uvAverageWeldedVerts; }
    set
    {
      if (uvAverageWeldedVerts != value)
      {
        uvAverageWeldedVerts = value;
        AutoGenerateMesh();
      }
    }
  }

  private Material material;

  /// <summary>
  /// Material set on the mesh after generating
  /// </summary>
  public Material Material
  {
    get { return material; }
    set
    {
      if (material != value)
      {
        material = value;
        AutoGenerateMesh();
      }
    }
  }



  private bool flipUV = false;
  /// <summary>
  /// Should UV's be flipped? (Rotated for vertical tiling vs horizontal tiling)
  /// </summary>
  /// <value></value>
  public bool FlipUV
  {
    get { return flipUV; }
    set
    {
      if (flipUV != value)
      {
        flipUV = value;
        AutoGenerateMesh();
      }
    }
  }

  private float directionalTilingAmount = 1.0f;
  /// <summary>
  /// Amount of tiling to do on U OR V for the texture. (depends if uv is flipped.)
  /// </summary>
  /// <value></value>
  public float DirectionalTilingAmount
  {
    get
    {
      return directionalTilingAmount;
    }
    set
    {
      if (directionalTilingAmount != value)
      {
        directionalTilingAmount = value;
        AutoGenerateMesh();
      }
    }
  }

  private float normalTilingAmount = 1.0f;
  /// <summary>
  /// Amount to tile on both U AND V
  /// </summary>
  /// <value></value>
  public float NormalTilingAmount
  {
    get { return normalTilingAmount; }
    set
    {
      if (normalTilingAmount != value)
      {
        normalTilingAmount = value;
        AutoGenerateMesh();
      }
    }
  }

  /// <summary>
  /// Sets the curve to be used to generate
  /// </summary>
  public void SetCurve(Curve c)
  {
    if (c == null)
    {
      Debug.LogError("Trying to set null curve.");
    }
    else
    {
      curve = c;
    }
  }


  /// <summary>
  /// Event handler for when the curve adds/removes/moves points, regenerates the mesh.
  /// </summary>
  public void OnPointsChangedHandler()
  {
    Debug.Log("OnPointsChangedHandler");
    AutoGenerateMesh();
  }

  private void AutoGenerateMesh()
  {
    if (AutoGenerate)
    {
      GenerateMesh();
    }
  }

  /// <summary>
  /// Generates a mesh along a curve.
  /// </summary>
  public void GenerateMesh()
  {
    // we need at least 2 control points to generate a mesh for.
    if (curve.ControlPointCount < 2)
    {
      Debug.LogWarning("SplineToMesh: Needs at least 2 control points");
      return;
    }
    if (WidthSegments <= 0)
    {
      Debug.LogWarning("SplineToMesh: Need at least 1 width segment, setting to 1.");
      WidthSegments = 1;
      return;
    }

    // variables we use throughout
    Vector3 direction = Vector3.zero;
    float distance = 0.0f;
    Vector3 p0 = Vector3.zero;
    Vector3 p1 = Vector3.zero;
    Vector3 up = Vector3.zero;
    Vector3 right = Vector3.zero;
    Vector3[] vertices = new Vector3[widthSegments * 2 + 2];
    Vector3 totalWidth = Vector3.zero;
    // vertex index offset for new end point vertices
    int newVertOffset = widthSegments + 1;
    Debug.Log("CtrlPtCount: " + curve.ControlPointCount + " Verts per segment:" + (widthSegments * 2 + 2));
    // calculate our up vector that we will use
    up = Vector3.up;
    if (FlipNormal)
    {
      up = Vector3.down;
    }

    // Create our lists and hashsets that we will use to create the mesh.
    List<Vector3> verticesList = new List<Vector3>();
    List<int> trianglesList = new List<int>();
    // the hashset makes it much quicker to check if we already have used a vertex.
    HashSet<Vector3> verticesSet = new HashSet<Vector3>();
    List<Vector2> uvsList = new List<Vector2>();
    // the total time of the curve.
    float totalTimeOfCurve = curve.TotalTime;
    // the current time of the curve at the control point we are at.
    float currentTime = 0.0f;

    Dictionary<int, int> VertWeldTimes = new Dictionary<int, int>();

    // go through all the control points (starting at 1.)
    for (int i = 0; i < curve.ControlPointCount - 1; i++)
    {
      // at first control point, set up the initial points
      if (i == 0)
      {
        // get the first 2 control points
        p0 = curve.GetControlPoint(0);
        // direction at p0 is it's tangent.
        direction = curve.GetTangent(0);
        // distance is 0 as this is the initial start-points.
        distance = 0.0f;
        // create the initial array of each "segment" of the path we are making.
        vertices = new Vector3[widthSegments * 2 + 2];
        // use the up vector to calculate the right vector (used to place vertices by multiplying with path width)
        right = Vector3.Cross(direction, up);
        // place the first vertex.
        vertices[0] = p0 + right * (PathWidth / 2);
        // vertices[1] = p0 - right * (PathWidth / 2);
        totalWidth = right * (PathWidth);
        // place width segment vertices
        for (int j = 1; j <= widthSegments; j++)
        {
          vertices[j] = vertices[0] - totalWidth * (j / (float)widthSegments);
        }
        // set up uvs for initial segment.
        for (int k = 0; k <= widthSegments; k++)
        {
          // uv val needs to be offset by k.
          float uvOffset = (k / (float)widthSegments);
          if (!flipUV)
          {
            uvsList.Add(new Vector2(0, uvOffset));
          }
          else
          {
            uvsList.Add(new Vector2(uvOffset, 0));
          }
        }
      }

      // calculate the time between the current control point and the next.
      float t0 = curve.GetTimeAtIndex(i);
      float t1 = curve.GetTimeAtIndex(i + 1);
      // calculate the total time for this segment
      float segmentTime = t1 - t0;
      // calculate the number of interpolation steps this segment of the curve should have.
      int interpolationSteps = Mathf.RoundToInt(Segments * (segmentTime / totalTimeOfCurve));
      // make sure there is at least 1 interpolation step.
      interpolationSteps = interpolationSteps < 1 ? 1 : interpolationSteps;
      // we already have the vertices at j=0 so we want to go from j=1 to the # of interpolation steps.
      for (int j = 1; j <= interpolationSteps; j++)
      {
        #region time direction distance from curve
        // the current time at this interpolation step.
        float currentInterpTime = currentTime + segmentTime * (j / (float)interpolationSteps);

        // get the new end point for this interpolation step.
        p1 = curve.GetPoint(currentInterpTime);
        // calc direction and distance of the control points
        if (UseTangents)
        {
          // smoothing tangents?
          if (SmoothTangents)
          {
            // simple basic smoothing by getting the new tangent, adding with old adn dividing by 2 to get the avg
            Vector3 newDirection = curve.GetTangent(currentInterpTime);
            // Debug.Log(segmentTime * (j + 1) / (float)interpolationSteps);
            Vector3 nextDirection = curve.GetTangent(currentInterpTime + segmentTime * 1 / (float)interpolationSteps);
            direction = (direction + newDirection + nextDirection) / 3;
            direction.Normalize();
          }
          else
          {
            // no smoothing? just use the tangent at that time.
            direction = curve.GetTangent(currentInterpTime);
          }
        }
        else
        {
          // no tangents? use the direction from p1 to p0.
          direction = (p1 - p0).normalized;
        }
        distance = (p1 - p0).magnitude;

        // calc right by direction and up.
        right = Vector3.Cross(direction, up).normalized;
        // calc new total width.
        totalWidth = right * PathWidth;
        // point at which all other points in the new end segment get displaced from. (rightmost point.)
        Vector3 point = p1 + right * PathWidth / 2;

        // lets try displacing from center.
        point = p1;

        DrawLineWorld(point, point + direction * distance, Color.cyan, 0.1f);
        #endregion

        #region vertex positions
        // calc vertex positions
        // Debug.Log(widthSegments / 2);
        //old method uses first point to base future points off of.
        // for (int k = newVertOffset; k < widthSegments * 2 + 2; k++)
        // {
        //   vertices[k] = point - totalWidth * (k - newVertOffset) / (float)widthSegments;
        // }




        // new method goes from center out in both directions. (more complicated, doesn't change output)
        float totalPointsPerSide = (widthSegments / (float)2);
        // new vertex halfway point.
        float centerVertexFloat = (newVertOffset + widthSegments / (float)2);
        // offset left half
        for (int k = newVertOffset; k < centerVertexFloat; k++)
        {
          vertices[k] = point + totalWidth / 2 * (centerVertexFloat - k) / totalPointsPerSide;
        }
        // only adjust the center point if the with segments are an even number. (a vertex would lay on the curve line.)
        if (widthSegments % 2 == 0)
        {
          vertices[newVertOffset + widthSegments / 2] = point;
        }
        // offset right half.
        for (int k = newVertOffset + 1 + widthSegments / 2; k < vertices.Length; k++)
        {
          vertices[k] = point - totalWidth / 2 * (k - centerVertexFloat) / totalPointsPerSide;
        }
        #endregion

        #region vertex welding
        //hashset of new vertices that were welded to "old" ones.
        HashSet<Vector3> newVerticesWelded = new HashSet<Vector3>();
        // indexs of the vertices that the new vertices were welded to (index of the old verts)
        Queue<int> weldedVertexIndexs = new Queue<int>();
        if (WeldVertices)
        {


          //TODO: vertex welding works to a point, but eventually breaks again.
          //I think because: as you weld over 90 degrees, the vertex moves backwards, which can overlap
          // over 90 degrees of the previous vertices which are no longer in the vertices array?
          // but increasing the angle cause the same issue even though the vertices should then move "forward"

          // float maxAng = 45f;
          // go through the start verts.
          for (int k = 0; k < newVertOffset; k++)
          {
            int companionIndex = k + newVertOffset;
            // try distance welding.
            if (Vector3.Distance(vertices[k], vertices[companionIndex]) < WeldDistance)
            {
              if (verticesSet.Contains(vertices[k]))
              {
                // get the index in the vertex list to update.
                int index = verticesList.IndexOf(vertices[k]);
                weldedVertexIndexs.Enqueue(index);
                // remove it from the vertex set
                verticesSet.Remove(vertices[k]);
                // if it's been welded before..
                if (VertWeldTimes.ContainsKey(index))
                {
                  // avg = (T * A + V) / (T+1)
                  vertices[k] = (VertWeldTimes[index] * vertices[k] + vertices[companionIndex]) / (VertWeldTimes[index] + 1);
                  VertWeldTimes[index] += 1;
                }
                else
                {
                  // add the number of vertices that were welded for this index as a new kvp
                  VertWeldTimes.Add(index, 2);
                  // update the vertex position by averaging
                  vertices[k] = (vertices[k] + vertices[companionIndex]) / 2;
                }
                // add the vertex back to the set.
                verticesSet.Add(vertices[k]);
                vertices[companionIndex] = vertices[k];
                // add it to our list of verts to skip as we don't want to add a UV for it.
                newVerticesWelded.Add(vertices[k]);
                // average the position with it's position and the companion vertex.
                verticesList[index] = vertices[k];
              }
            }
            else // angle weld.
            {
              // 
              // it's companion vertex is k + vert offset.
              // direction to it's companion.
              // Vector3 dirToCompanion = vertices[companionIndex] - vertices[k];
              // // DrawLineWorld(vertices[k], vertices[k] + dirToCompanion, Color.yellow, 0.1f);
              // float angle = Vector3.Angle(direction, dirToCompanion);
              // // angle > 90 -> pointing the other direction?
              // // TODO: improve somehow.
              // if (angle >= WeldAngle)
              // {

              //   Debug.Log(angle);

              //   // weld vertices.
              //   // weld vertices avg.
              //   // vertices[k] = (vertices[k] + vertices[companionIndex]) / 2;
              //   // check if it exists in our list.
              //   if (verticesSet.Contains(vertices[k]))
              //   {
              //     DrawLineWorld(vertices[k], vertices[k] + direction, Color.yellow, 0.1f);
              //     DrawLineWorld(vertices[k], vertices[companionIndex], Color.red, 0.1f);
              //     DrawLineWorld(vertices[companionIndex], vertices[companionIndex] + Vector3.up, Color.magenta, 0.1f);
              //     DrawLineWorld(vertices[k], vertices[k] + Vector3.up, Color.cyan, 0.1f);
              //     Debug.Log(i + " : " + j + " : " + direction);
              //     // return;
              //     // get the index in the vertex list to update.
              //     int index = verticesList.IndexOf(vertices[k]);
              //     weldedVertexIndexs.Enqueue(index);
              //     // remove it from the vertex set
              //     verticesSet.Remove(vertices[k]);
              //     // if it's been welded before..
              //     if (!VertWeldTimes.ContainsKey(index))
              //     {
              //       VertWeldTimes.Add(index, 2);
              //     }
              //     // add the vertex back to the set.
              //     verticesSet.Add(vertices[k]);
              //     vertices[companionIndex] = vertices[k];
              //     // add it to our list of verts to skip as we don't want to add a UV for it.
              //     newVerticesWelded.Add(vertices[k]);
              //     // average the position with it's position and the companion vertex.
              //     verticesList[index] = vertices[k];
              //   }
              // }
            }
          }
        }
        #endregion

        #region vertex adding
        // get the index / add the vertex to the list and set and get it's index
        int[] vertexIndexs = new int[widthSegments * 2 + 2];
        for (int k = 0; k < vertices.Length; k++)
        {
          vertexIndexs[k] = GetAddVertexIndex(vertices[k], verticesList, verticesSet);
          if (curve.Loop && i == curve.ControlPointCount - 2 && j == interpolationSteps)
          {
            // on the last set on a looped curve, vertices at the end 
            // can match positions of vertices at the start and not be added as seperate vertices
            // so we check if we're at the end vertices, and if it wasn't actually added
            // alternatively: we could use the vertex indexs to see if the UV has to be added in the UV portion?
            if (k > newVertOffset && vertexIndexs[k] < verticesSet.Count - 1)
            {
              // then we add it
              verticesList.Add(vertices[k]);
              // and update the vertex index.
              vertexIndexs[k] = verticesList.Count - 1;
            }
          }
        }

        // handle loooped curve vertex positioning by placing the first vertices where the last vertices were placed when looping.
        if (curve.Loop && i == curve.ControlPointCount - 2 && j == interpolationSteps)
        {
          // when looping, adjust the first vertices to match the last ones
          // this is better for the uv's?
          for (int k = 0; k <= widthSegments; k++)
          {
            verticesList[k] = vertices[newVertOffset + k];
          }
        }

        #endregion

        #region triangles

        // set up the triangles for the new section.
        int[] triangles = new int[widthSegments * 2 * 3];
        for (int k = 0; k < widthSegments; k++)
        {
          // triangle offset is 2 * k * 3.
          // or k * 6.
          // 6 index's per segment.
          triangles[2 * k * 3] = vertexIndexs[k];
          triangles[2 * k * 3 + 1] = vertexIndexs[k + newVertOffset];
          triangles[2 * k * 3 + 2] = vertexIndexs[k + 1];
          triangles[2 * k * 3 + 3] = vertexIndexs[k + newVertOffset];
          triangles[2 * k * 3 + 4] = vertexIndexs[k + newVertOffset + 1];
          triangles[2 * k * 3 + 5] = vertexIndexs[k + 1];
        }
        // add the triangles to our list.
        trianglesList.AddRange(triangles);

        #endregion


        #region flipped tri welding

        // need to do this in the weld section?
        if (WeldVertices)
        {
          for (int k = 0; k < triangles.Length; k += 3)
          {
            // calc triangle normal.
            Vector3 a0 = verticesList[triangles[k]];
            Vector3 a1 = verticesList[triangles[k + 1]];
            Vector3 a2 = verticesList[triangles[k + 2]];
            Vector3 n = Vector3.Cross(a0 - a1, a1 - a2).normalized;
            Vector3 c = (a0 + a1 + a2) / 3;
            if (Vector3.Angle(n, (FlipNormal ? Vector3.down : Vector3.up)) > 90f)
            {
              // this triangle is flipped in the wrong direction as the other normals.
              DrawLineWorld(c, c + n, Color.blue, 0.1f);
              // need to merge the 2 "edge" vertices that are flipped to remove the triangle.
              // but which two are they?
            }
          }
        }
        #endregion

        #region UVs

        // calculate uv positions for the 2 new vertices.
        // depending on if the uv's are flipped (rotated) we need to use the previous uv's x or y value.
        Vector2 nextUVPosition = uvsList[uvsList.Count - 1];
        // add the distance * the tiling amount from the previous segment end to this segments end
        nextUVPosition.x += distance * directionalTilingAmount * normalTilingAmount / pathWidth;
        nextUVPosition.y += distance * directionalTilingAmount * normalTilingAmount / pathWidth;
        for (int k = 0; k <= widthSegments; k++)
        {
          // calculate the offset for this vertex
          float uvOffset = k / (float)widthSegments;
          Vector2 newUV = Vector3.zero;
          // here we add widthsegments + 1 UV locations, 1 for each new vertex added on the end of the curve
          if (!flipUV)
          {
            // calculate the new (u,v) coordinate based on if the UV is flipped or not.
            newUV = new Vector2(nextUVPosition.x, uvOffset * normalTilingAmount);
          }
          else
          {
            newUV = new Vector2(uvOffset * normalTilingAmount, nextUVPosition.y);
          }

          // vertices were merged
          if (newVerticesWelded.Contains(vertices[k + newVertOffset]))
          {
            // if we're smoothing the uvs of the merged vertices out.
            if (UVAverageWeldedVerts)
            {
              // get the index of the vert that the current one was merged with. (this is the index of the old vertex the new vert was welded onto)
              int baseIndex = weldedVertexIndexs.Dequeue();
              // get the number of times the vert has been welded
              int timesWelded = VertWeldTimes[baseIndex];
              // modify the UV to average.
              uvsList[baseIndex] = (timesWelded * uvsList[baseIndex] + newUV) / (timesWelded + 1);
            }
            else
            {
              continue; // don't add this UV position if it was welded and we're not uv smoothing.
            }
          }
          else
          {
            // non-welded vertex? just add it to the list.
            uvsList.Add(newUV);
          }
        }

        #endregion

        // set p0 to p1. (the new start point is the last end point)
        p0 = p1;

        // with width segments, the new end vertices become the next start vertices.
        for (int k = 0; k <= widthSegments; k++)
        {
          vertices[k] = vertices[k + newVertOffset];
        }
      }
      // increase the current time by the segment time.
      currentTime += segmentTime;
    }
    Debug.Log("Vert Count:" + verticesList.Count + " TriangleCount:" + trianglesList.Count / 3 + " UV Count:" + uvsList.Count);

    #region Mesh Creation

    // were done generating the data, time to create the mesh:
    // we need to make sure we have a mesh renderer and mesh filter.
    CheckCreateMeshComponents();
    meshRenderer.sharedMaterial = material;
    if (meshFilter.sharedMesh != null)
    {
      DestroyImmediate(meshFilter.sharedMesh);
    }
    // foreach (Vector3 v in verticesList)
    // {
    //   Debug.Log(v);
    // }
    // create a mesh, assign the verts, tris and recalc normals.
    Mesh m = new Mesh();
    m.vertices = verticesList.ToArray();
    m.triangles = trianglesList.ToArray();
    m.uv = uvsList.ToArray();
    m.RecalculateNormals();
    // set this mesh as the mesh fitler's shared mesh.
    meshFilter.sharedMesh = m;

    #endregion
  }



  /// <summary>
  /// /// Gets the index of the vertex if it already exists, if not it adds it and returns the index
  /// </summary>
  /// <param name="vertex"></param>
  /// <param name="vertexList"></param>
  /// <param name="vertexSet"></param>
  /// <returns></returns>
  public int GetAddVertexIndex(Vector3 vertex, List<Vector3> vertexList, HashSet<Vector3> vertexSet)
  {
    if (vertexSet.Contains(vertex))
    {
      return vertexList.IndexOf(vertex);
    }
    else
    {
      int i = vertexList.Count;
      vertexList.Add(vertex);
      vertexSet.Add(vertex);
      return i;
    }
  }

  /// <summary>
  /// draws a line in world space.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  /// <param name="color"></param>
  /// <param name="duration"></param>
  public void DrawLineWorld(Vector3 start, Vector3 end, Color color, float duration)
  {
    Debug.DrawLine(this.transform.TransformPoint(start), this.transform.TransformPoint(end), color, duration);
  }


  /// <summary>
  /// Gets or adds a component
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T GetAddComponent<T>() where T : Component
  {
    T c = this.GetComponent<T>();
    if (c == null)
    {
      c = this.gameObject.AddComponent<T>();
    }
    return c;
  }

  /// <summary>
  /// Gets or adds the mesh filter and mesh renderer components
  /// </summary>
  public void CheckCreateMeshComponents()
  {
    meshFilter = GetAddComponent<MeshFilter>();
    meshRenderer = GetAddComponent<MeshRenderer>();
  }


}

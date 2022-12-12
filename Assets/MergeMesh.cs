using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeMesh : MonoBehaviour

{
    // The input meshes to merge
    public MeshFilter[] inputMeshes;

    public GameObject FilterBox;
    // The bounds of the bounding box
    private Bounds bounds;

    // The resulting merged mesh
    public MeshFilter outputMesh;

    // Flag to indicate whether to discard triangles outside of the bounding box
    public bool cullTriangles = true;

    private void Start()
    {
        bounds = FilterBox.GetComponent<BoxCollider>().bounds;
    }

    private void Update()
    {
        MergeMeshCombine();
        //MergeMeshPiecewise();
    }

    void MergeMeshCombine()
    {
        CombineInstance[] combine = new CombineInstance[inputMeshes.Length];   
        
        int i = 0;
        while (i < inputMeshes.Length)
        {
            combine[i].mesh = inputMeshes[i].sharedMesh;
            
            //meshFilters[i].gameObject.SetActive(false);
            
            
            //here starts the fun part
            
            //here the fun part stops
            
            combine[i].transform = inputMeshes[i].transform.localToWorldMatrix;
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
    }

    void MergeMeshPiecewise()
    {
        // Create a new mesh to store the merged result
        Mesh mergedMesh = new Mesh();

        // Create lists to store the merged vertices, triangles, and normals
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Loop through each input mesh
        foreach (var meshFilter in inputMeshes)
        {
            // Get the input mesh
            var mesh = meshFilter.sharedMesh;

            // Loop through each triangle in the input mesh
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                // Check if we need to discard the triangle
                if (cullTriangles && !IsTriangleInsideBounds(mesh, i))
                {
                    // Skip this triangle if it is outside of the bounding box
                    continue;
                }

                // Add the triangle vertices to the list of merged vertices
                vertices.Add(mesh.vertices[mesh.triangles[i]]);
                vertices.Add(mesh.vertices[mesh.triangles[i + 1]]);
                vertices.Add(mesh.vertices[mesh.triangles[i + 2]]);

                // Add the triangle indices to the list of merged triangles
                int vertexCount = vertices.Count;
                triangles.Add(vertexCount - 3);
                triangles.Add(vertexCount - 2);
                triangles.Add(vertexCount - 1);

                // Add the triangle normals to the list of merged normals
                normals.Add(mesh.normals[mesh.triangles[i]]);
                normals.Add(mesh.normals[mesh.triangles[i + 1]]);
                normals.Add(mesh.normals[mesh.triangles[i + 2]]);
            }
        }

        // Set the merged mesh data
        mergedMesh.SetVertices(vertices);
        mergedMesh.SetTriangles(triangles, 0);
        mergedMesh.SetNormals(normals);

        // Assign the merged mesh to the output mesh filter
        outputMesh.mesh = mergedMesh;
        outputMesh.mesh.RecalculateBounds();
    }

    // Method to check if a triangle is inside the bounding box
// Method to check if a triangle is inside the bounding box
    private bool IsTriangleInsideBounds(Mesh mesh, int triangleIndex)
    {
        // Get the triangle vertices
        var v0 = mesh.vertices[mesh.triangles[triangleIndex]];
        var v1 = mesh.vertices[mesh.triangles[triangleIndex + 1]];
        var v2 = mesh.vertices[mesh.triangles[triangleIndex + 2]];

        // Check if any of the triangle vertices are inside the bounding box
        return bounds.Contains(v0) & bounds.Contains(v1) & bounds.Contains(v2);
    }
}

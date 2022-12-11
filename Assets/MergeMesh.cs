using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeMesh : MonoBehaviour

{
    // The list of mesh filters to process
    public MeshFilter[] meshFilters;

    // The box GameObject that defines the volume of interest
    public GameObject boxObject;

    // The output mesh
    public Mesh outputMesh;

    // Update is called once per frame
    void Update()
    {
        // Get the bounds of the volume of interest from the box GameObject
        Bounds bounds = boxObject.GetComponent<BoxCollider>().bounds;

        // Create a list to hold the filtered vertices
        List<Vector3> filteredVertices = new List<Vector3>();

        // Create a list to hold the filtered triangles
        List<int> filteredTriangles = new List<int>();

        // Loop through all the mesh filters
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Get the mesh from the mesh filter
            Mesh mesh = meshFilter.mesh;

            // Get the world-space position of the mesh
            Vector3 meshPosition = meshFilter.transform.position;

            // Get the world-space rotation of the mesh
            Quaternion meshRotation = meshFilter.transform.rotation;

            // Loop through all the vertices in the mesh
            foreach (Vector3 vertex in mesh.vertices)
            {
                // Transform the vertex from local space to world space
                Vector3 worldVertex = meshPosition + meshRotation * vertex;

                // Check if the vertex is inside the volume of interest
                if (bounds.Contains(worldVertex))
                {
                    // Add the vertex to the list of filtered vertices
                    filteredVertices.Add(worldVertex);
                }
            }

            // Loop through all the triangles in the mesh
            foreach (int triangleIndex in mesh.triangles)
            {
                // Add the triangle index to the list of filtered triangles
                filteredTriangles.Add(triangleIndex);
            }
        }

        // Create a new mesh for the output
        outputMesh = new Mesh();

        // Set the filtered vertices as the mesh's vertices
        outputMesh.vertices = filteredVertices.ToArray();

        // Set the filtered triangles as the mesh's triangles
        outputMesh.triangles = filteredTriangles.ToArray();

        // Recalculate the normals and tangents of the output mesh
        outputMesh.RecalculateNormals();
        outputMesh.RecalculateTangents();
    }
}

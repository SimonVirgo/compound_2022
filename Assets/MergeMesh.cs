using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeMesh : MonoBehaviour
{
    public List<MeshFilter> meshFilters; // The list of mesh filters to merge

    // The combined mesh and its corresponding mesh filter
    private Mesh combinedMesh;
    private MeshFilter combinedMeshFilter;

    void Start()
    {
        // Create a new mesh to store the merged meshes
        combinedMesh = new Mesh();

        // Assign the combined mesh to the mesh filter on this game object
        combinedMeshFilter = GetComponent<MeshFilter>();
        combinedMeshFilter.mesh = combinedMesh;

        // Merge the meshes from the input mesh filters
        MergeMeshes();
    }

    void Update()
    {
        // Check if any of the input mesh filters have changed
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (!meshFilter.mesh.isReadable )
            {
                return;
            }
        }
        MergeMeshes();
    }

    void MergeMeshes()
    {
        // Create lists to store the combined vertices, normals, and triangles
        List<Vector3> combinedVertices = new List<Vector3>();
        List<Vector3> combinedNormals = new List<Vector3>();
        List<int> combinedTriangles = new List<int>();

        // Loop through each mesh filter in the list
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Get the mesh from the mesh filter
            Mesh mesh = meshFilter.mesh;

            // Combine the vertices, normals, and triangles from the mesh into the lists
            combinedVertices.AddRange(mesh.vertices);
            combinedNormals.AddRange(mesh.normals);
            combinedTriangles.AddRange(mesh.triangles);
        }

        // Set the combined vertices, normals, and triangles to the mesh
        combinedMesh.SetVertices(combinedVertices);
        combinedMesh.SetNormals(combinedNormals);
        combinedMesh.SetTriangles(combinedTriangles, 0);
    }
}
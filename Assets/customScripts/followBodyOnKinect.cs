using System;
using System.Collections;
using System.Collections.Generic;
using Akvfx;
using UnityEngine;

public class followBodyOnKinect : MonoBehaviour
{
    public GameObject EventTriggerKinect;
    private MeshRenderer BoxRenderer;
    public MeshFilter inputMesh;
    public MergePointClouds inputPointCloud;
    private Vector3[] vertices;
    public float toleranceDistance;
    public float minimumSizeX;
    public float minimumSizeY;
    public float minimumSizeZ;
    
    /// <summary>
    /// Caution: this currently only works with worldspace meshes. The transform of the meshes is not taken into account!
    /// </summary>
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (EventTriggerKinect)
        {
            EventTriggerKinect.GetComponent<DeviceController>().NewMasterFrameAvailable += AdaptColliderBounds;
        }
       
        //get the box collider from the game Object.
        BoxRenderer = GetComponent<MeshRenderer>();
        

    }
    private void AdaptColliderBounds(object sender, EventArgs e)
    {
        AdaptColliderBounds();
    }
    void AdaptColliderBounds()
    {
        //get vertices from the mesh   
        if (inputMesh)
        {
            vertices = inputMesh.mesh.vertices;
        }
        else if (inputPointCloud)
        {
            vertices = inputPointCloud.combinedPointCloud;
        }
        else
        {
            Debug.LogError("No input mesh or point cloud assigned to the script!");
        }
        
       // BoxRenderer.ResetBounds();
        bool includesVertices = false; 
        //add tolerance size to new bounds
        
        var enlargedBoundsSize = BoxRenderer.bounds.size + new Vector3(toleranceDistance, toleranceDistance, toleranceDistance);    
        var enlargedBounds = new Bounds(BoxRenderer.bounds.center, enlargedBoundsSize);
        //enlarge the collider by the tolerance distance in all directions
          
        //instantiate initial bound values:
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
      
        //check which vertices of the mesh are in the collider bounds
        for (int i = 0; i < vertices.Length; i++)
        {
            if (enlargedBounds.Contains(vertices[i]))
            { 
                includesVertices = true;
                //update the bounds to incorporate the 
                if (vertices[i].x < minX) minX = vertices[i].x;
                if (vertices[i].x > maxX) maxX = vertices[i].x;
                if (vertices[i].y < minY) minY = vertices[i].y;
                if (vertices[i].y > maxY) maxY = vertices[i].y;
                if (vertices[i].z < minZ) minZ = vertices[i].z;
                if (vertices[i].z > maxZ) maxZ = vertices[i].z;
            }
        }
        
        if (includesVertices)
        {

            // Calculate the center and size of the new box collider
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            //make sure the size is not smaller than the minimum size   
            float sizeX = Mathf.Max(maxX - minX, minimumSizeX);
            float sizeY = Mathf.Max(maxY - minY, minimumSizeY);
            float sizeZ = Mathf.Max(maxZ - minZ, minimumSizeZ);
        
            Vector3 size = new Vector3(sizeX, sizeY, sizeZ);
            //set the collider bounds to the new bounds
            //boxCollider.bounds.SetMinMax(center - size / 2, center + size / 2);

            //set the center as transform position
            var transform1 = transform;
            transform1.position = center;
            //set scale
            transform1.localScale = size;
        }

        
    }

    
    

    // Update is called once per frame
    void Update()
    {
        if (EventTriggerKinect) return;

        //adapt the collider bounds
        AdaptColliderBounds();
    }
}

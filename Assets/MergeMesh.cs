using System;
using System.Collections;
using System.Collections.Generic;
using Akvfx;
using UnityEngine;

public class MergeMesh : MonoBehaviour
//This Script causes a memory leak somehere! DO NOT USE!!!
{
    // The input meshes to merge
    public MeshFilter[] inputMeshes;

    // The resulting merged mesh
    public MeshFilter outputMesh;

    // Flag to indicate whether to discard triangles outside of the bounding box
    public bool cullTriangles = true;

    public GameObject EventTriggerKinect;
    private void Start()
    {
        SubscribeEvent();
    }

    void SubscribeEvent()
    {
        EventTriggerKinect.GetComponent<DeviceController>().NewMasterFrameAvailable += MergeMeshCombine;
    }

    private void MergeMeshCombine(object sender, EventArgs e)
    {
        MergeMeshCombine();
    }

    private void Update()
    {
        //MergeMeshCombine();
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
        //transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine,false,true,false);
        transform.gameObject.SetActive(true);
    }


}

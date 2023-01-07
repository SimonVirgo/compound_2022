using System;
using System.Collections;
using System.Collections.Generic;
using Akvfx;
using UnityEngine;

public class MergePointClouds : MonoBehaviour

{


    public GameObject EventTriggerKinect;
    // This is the list of game objects that you want to get the point clouds from
    public GameObject[] gameObjects;

    // This is the array that will hold the converted point clouds
    public Vector3[] convertedPointClouds;


    private DeviceController[] deviceControllers;
    private Transform[] transforms;

    void Start()
    {
        EventTriggerKinect.GetComponent<DeviceController>().NewMasterFrameAvailable += Combine;
        deviceControllers = new DeviceController[gameObjects.Length];
        transforms = new Transform[gameObjects.Length];
        for (int i = 0; i < gameObjects.Length; i++)
        {
            deviceControllers[i] = gameObjects[i].GetComponent<DeviceController>();
            transforms[i] = gameObjects[i].transform;
        }

    }
    
    

    private void Combine(object sender, EventArgs e)
    {
        Combine();
    }
    void Combine()
    {
        // Initialize the list of converted point clouds
        convertedPointClouds = new Vector3[0];

        // Iterate through each game object in the list
        for (int i = 0; i < gameObjects.Length; i++)
        
        {
            // Convert the point cloud from local to world space
            Vector3[] worldSpacePointCloud = ConvertPointCloud(deviceControllers[i].Positions, transforms[i]);

            // Combine the converted point cloud with the existing converted point clouds
            convertedPointClouds = CombinePointClouds(convertedPointClouds, worldSpacePointCloud);
        }
    }

    Vector3[] ConvertPointCloud(Vector3[] localSpacePointCloud, Transform transform)
    {
        // This will hold the converted point cloud
        Vector3[] worldSpacePointCloud = new Vector3[localSpacePointCloud.Length];

        // Iterate through each point in the point cloud
        for (int i = 0; i < localSpacePointCloud.Length; i++)
        {
            // Convert the point from local to world space
            worldSpacePointCloud[i] = transform.TransformPoint(localSpacePointCloud[i]);
        }

        // Return the converted point cloud
        return worldSpacePointCloud;
    }

    Vector3[] CombinePointClouds(Vector3[] pointCloud1, Vector3[] pointCloud2)
    {
        // This will hold the combined point clouds
        Vector3[] combinedPointClouds = new Vector3[pointCloud1.Length + pointCloud2.Length];

        // Copy the first point cloud into the combined point clouds
        pointCloud1.CopyTo(combinedPointClouds, 0);

        // Copy the second point cloud into the combined point clouds
        pointCloud2.CopyTo(combinedPointClouds, pointCloud1.Length);

        // Return the combined point clouds
        return combinedPointClouds;
    }

}
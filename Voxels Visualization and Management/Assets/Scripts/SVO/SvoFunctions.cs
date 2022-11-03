using System.Collections.Generic;
using UnityEngine;

public static class SvoFunctions
{
    /// <summary>
    /// Add voxels with colors to SVO
    /// </summary>
    /// <param name="svo">SVO object</param>
    /// <param name="voxels">boolean array representing voxels</param>
    /// <param name="sizeX">size of voxels array in X direction</param>
    /// <param name="sizeY">size of voxels array in Y direction</param>
    /// <param name="pivotPoint">pivot point of voxels' block</param>
    /// <param name="voxelSize">voxel size in meters</param>
    public static void AddBinaryVoxelModelToSvo(SVOObject svo, bool[] voxels, int sizeX, int sizeY, Vector3 pivotPoint, float voxelSize)
    {
        svo.AddVoxelsToSVO(voxels, sizeX, sizeY, pivotPoint, voxelSize);
    }

    /// <summary>
    /// Add voxels with colors to SVO
    /// </summary>
    /// <param name="svo">SVO object</param>
    /// <param name="voxels">voxel array having a list of objects per voxel</param>
    /// <param name="sizeX">size of voxels array in X direction</param>
    /// <param name="sizeY">size of voxels array in Y direction</param>
    /// <param name="pivotPoint">pivot point of voxels' block</param>
    /// <param name="voxelSize">voxel size in meters</param>
    public static void AddColorVoxelModelToSvo(SVOObject svo, List<VoxelObject>[] voxels, int sizeX, int sizeY, Vector3 pivotPoint, float voxelSize)
    {
        svo.AddVoxelsWithColorToSVO(voxels, sizeX, sizeY, pivotPoint, voxelSize);
    }

    /// <summary>
    /// Add voxels with colors to SVO
    /// </summary>
    /// <param name="svo">SVO object</param>
    /// <param name="bounds">Bounds of the scene</param>
    /// <param name="voxelSize">voxel size in meters</param>
    public static void VisualizeSvo(SVOObject svo, Bounds bounds, float voxelSize)
    {
        svo.VisualizeOctree();
        var scale = Mathf.Pow(2, svo.Depth);
        var octreePrefab = Resources.Load("Prefabs/Octree") as GameObject;
        var halfScale = scale / 2;
        var octreePosition = new Vector3(halfScale + bounds.min.x, halfScale + bounds.min.y, halfScale + bounds.min.z);
        var voxelLayer = Object.Instantiate(octreePrefab, octreePosition, new Quaternion());
        voxelLayer.name = "SVO_" + voxelSize;
        voxelLayer.transform.localScale = new Vector3(scale, scale, scale);
    }
}

using System.Collections.Generic;
using UnityEngine;

public static class SvoFunctions
{
    /// <summary>
    /// Visualize voxel model using SVO
    /// </summary>
    /// <param name="voxels">boolean array representing voxels.</param>
    /// <param name="sizeX">size of voxels array in X direction</param>
    /// <param name="sizeY">size of voxels array in Y direction</param>
    /// <param name="bounds">bounds of the model</param>
    /// <param name="voxelSize">voxel size in meters</param>
    /// <param name="voxelColor">voxel color</param>
    public static void VisualizeBinaryVoxelModelWithSVO(bool[] voxels, int sizeX, int sizeY, Bounds bounds, float voxelSize, Color voxelColor)
    {
        var octree = new VoxelPlacer();
        octree.BuildSVO(voxels, sizeX, sizeY, voxelSize, bounds, voxelColor);
        var scale = Mathf.Pow(2, octree.Depth);
        var octreePrefab = Resources.Load("Prefabs/Octree") as GameObject;
        var halfScale = scale / 2;
        var octreePosition = new Vector3(halfScale + bounds.min.x, halfScale + bounds.min.y, halfScale + bounds.min.z);
        var voxelLayer = Object.Instantiate(octreePrefab, octreePosition, new Quaternion());
        voxelLayer.name = "SVO_" + voxelSize;
        voxelLayer.transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Visualize voxel model using SVO
    /// </summary>
    /// <param name="voxels">voxel array having a list of objects per voxel</param>
    /// <param name="sizeX">size of voxels array in X direction</param>
    /// <param name="sizeY">size of voxels array in Y direction</param>
    /// <param name="bounds">bounds of the model</param>
    /// <param name="voxelSize">voxel size in meters</param>
    public static void VisualizeColorVoxelModelWithSVO(List<VoxelObject>[] voxels, int sizeX, int sizeY, Bounds bounds, float voxelSize)
    {
        var octree = new VoxelPlacer();
        octree.BuildSVOWithColor(voxels, sizeX, sizeY, voxelSize, bounds);
        var scale = Mathf.Pow(2, octree.Depth);
        var octreePrefab = Resources.Load("Prefabs/Octree") as GameObject;
        var halfScale = scale / 2;
        var octreePosition = new Vector3(halfScale + bounds.min.x, halfScale + bounds.min.y, halfScale + bounds.min.z);
        var voxelLayer = Object.Instantiate(octreePrefab, octreePosition, new Quaternion());
        voxelLayer.name = "SVOColor_" + voxelSize;
        voxelLayer.transform.localScale = new Vector3(scale, scale, scale);
    }
}

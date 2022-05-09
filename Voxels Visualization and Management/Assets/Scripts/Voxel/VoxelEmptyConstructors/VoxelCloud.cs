using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem
{
    public class VoxelCloud<T>
    {
        public Vector3 PivotPoint { get; }
        public float VoxelSize { get; }
        public IEnumerable<List<T>> Voxels { get; }

        public VoxelCloud(Vector3 pivotPoint, IEnumerable<List<T>> voxels, float voxelSize)
        {
            PivotPoint = pivotPoint;
            Voxels = voxels;
            VoxelSize = voxelSize;
        }
    }
}
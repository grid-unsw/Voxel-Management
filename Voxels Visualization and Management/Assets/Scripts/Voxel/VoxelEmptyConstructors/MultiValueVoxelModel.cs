using System.Collections.Generic;
using UnityEngine;


namespace VoxelSystem
{
    public class MultiValueVoxelModel
    {
        public List<VoxelObject>[] Voxels { get; }
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public Vector3 PivotPoint { get; }
        public float VoxelSize { get; }

        public MultiValueVoxelModel(List<VoxelObject>[] voxels, int width, int height, int depth, Vector3 pivotPoint, float voxelSize)
        {
            Voxels = voxels;
            Width = width;
            Height = height;
            Depth = depth;
            PivotPoint = pivotPoint;
            VoxelSize = voxelSize;
        }
    }
}
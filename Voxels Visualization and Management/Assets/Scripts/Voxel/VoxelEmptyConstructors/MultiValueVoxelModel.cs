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
        public Bounds Bounds { get; }
        public float VoxelSize { get; }

        public MultiValueVoxelModel(List<VoxelObject>[] voxels, int width, int height, int depth, Bounds bounds, float voxelSize)
        {
            Voxels = voxels;
            Width = width;
            Height = height;
            Depth = depth;
            Bounds = bounds;
            VoxelSize = voxelSize;
        }
    }
}
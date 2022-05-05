using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem.PointCloud
{
    public class PointCloud<T>
    {
        public Bounds bounds { get; }
        public T[] points { get; }

        public PointCloud(Bounds Bounds, T[] Points)
        {
            bounds = Bounds;
            points = Points;
        }
    }
}
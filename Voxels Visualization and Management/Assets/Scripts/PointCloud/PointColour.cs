using UnityEngine;

namespace VoxelSystem.PointCloud
{
    public class PointColour : Point
    {
        public Color PointColor { get; }

        public PointColour(Vector3 point, Color color) : base(point)
        {
            PointColor = color;
        }
    }
}
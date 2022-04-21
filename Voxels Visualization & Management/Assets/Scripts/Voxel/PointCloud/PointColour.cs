using UnityEngine;

namespace VoxelSystem.PointCloud
{
    public class PointColour : Point
    {
        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }

        public PointColour(float x, float y, float z, float red, float green, float blue) : base(new Vector3(x, y, z))
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
}
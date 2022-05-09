using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem
{
    public class VoxelColour : VoxelIndex
    {
        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }

        public VoxelColour(uint x, uint y, uint z, float red, float green, float blue) : base(x, y, z)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public VoxelColour(uint x, uint y, uint z) : base(x, y, z)
        {

        }
    }
}
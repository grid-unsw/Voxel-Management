using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSystem
{
    public class VoxelIndex
    {
        public uint X { get; }
        public uint Y { get; }
        public uint Z { get; }

        public VoxelIndex(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Index3D
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public Index3D(int xIndex, int yIndex, int zIndex)
    {
        X = xIndex;
        Y = yIndex;
        Z = zIndex;
    }
}

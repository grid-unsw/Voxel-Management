using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Index3D
{
    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Z { get { return z; } }

    private int x, y, z;

    public Index3D(int xIndex, int yIndex, int zIndex)
    {
        x = xIndex;
        y = yIndex;
        z = zIndex;
    }
}

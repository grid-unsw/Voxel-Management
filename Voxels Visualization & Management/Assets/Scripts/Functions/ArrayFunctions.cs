using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayFunctions
{
    public static int Index3DTo1D(int x, int y, int z, int xMax, int yMax)
    {
        return (z * xMax * yMax) + (y * xMax) + x;
    }

    public static Index3D Index1DTo3D(int idx, int xMax, int yMax)
    {
        var z = idx / (xMax * yMax);
        idx -= (z * xMax * yMax);
        var y = idx / xMax;
        var x = idx % xMax;
        return new Index3D(x,y,z);
    }
}

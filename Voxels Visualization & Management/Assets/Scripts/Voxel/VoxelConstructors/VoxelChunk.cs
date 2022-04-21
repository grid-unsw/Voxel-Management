using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunk 
{
    public int XMin { get; }
    public int YMin { get; }
    public int ZMin { get; }
    public Mesh Mesh { get; }

    public VoxelChunk(int xMin, int yMin, int zMin, Mesh mesh)
    {
        XMin = xMin;
        YMin = yMin;
        ZMin = zMin;
        Mesh = mesh;
    }
}

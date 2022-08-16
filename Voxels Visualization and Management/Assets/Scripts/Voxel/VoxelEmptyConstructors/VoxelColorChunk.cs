using UnityEngine;

public class VoxelColorChunk
{
    public int XMin { get; }
    public int YMin { get; }
    public int ZMin { get; }
    public Mesh Mesh { get; }

    public Material[] Materials { get; }

    public VoxelColorChunk(int xMin, int yMin, int zMin, Mesh mesh, Material[] mat)
    {
        XMin = xMin;
        YMin = yMin;
        ZMin = zMin;
        Mesh = mesh;
        Materials = mat;
    }
}

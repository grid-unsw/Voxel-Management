using System.Collections.Generic;
using UnityEngine;

public class MeshFiltersChunk
{
    public MeshFilter[] MeshFilters { get;}
    public Bounds Bounds { get; }

    public MeshFiltersChunk(MeshFilter[] meshFilters, Bounds bounds)
    {
        MeshFilters = meshFilters;
        Bounds = bounds;
    }

}

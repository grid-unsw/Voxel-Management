using System.Collections.Generic;
using UnityEngine;

public class MeshFiltersChunk
{
    public List<MeshFilter> MeshFilters { get;}
    public Bounds Bounds { get; }
    public Index3D Extents { get; }

    public MeshFiltersChunk(List<MeshFilter> meshFilters, Bounds bounds, Index3D extents)
    {
        MeshFilters = meshFilters;
        Bounds = bounds;
        Extents = extents;
    }

}

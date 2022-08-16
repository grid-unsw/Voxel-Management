using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelSystem;

public class SceneObjectsOctree
{
    private BoundsOctree<MeshFilter> _sceneObjectsBoundsOctree;
    public Bounds SceneBounds { get; }
    public SceneObjectsOctree(MeshFilter[] meshFilters)
    {
        var meshesBounds = GetMeshesBoundsInGlobalSpace(meshFilters);

        SceneBounds = new Bounds((meshesBounds.Item1 + meshesBounds.Item2) / 2,
            meshesBounds.Item2 - meshesBounds.Item1);

        var largestBoundsSide = Mathf.Max(SceneBounds.size.x,
            Mathf.Max(SceneBounds.size.y, SceneBounds.size.z));

        _sceneObjectsBoundsOctree =
            new BoundsOctree<MeshFilter>(largestBoundsSide, SceneBounds.center, 1f, 1.25f);

        foreach (var meshFilter in meshFilters)
        {
            var bounds = meshFilter.gameObject.GetComponent<Renderer>().bounds;
            _sceneObjectsBoundsOctree.Add(meshFilter, bounds);
        }
    }
    
    public IEnumerable<MeshFiltersChunk> GetMeshFilterChunks(float voxelSize, Bounds bounds)
    {
        var extendedBounds = VoxelFunctions.GetExtendedBounds(bounds.min, bounds.max, voxelSize);
        var chunkMaxSize = (int)Mathf.Pow(2, 8);
        var xMax = Mathf.CeilToInt((float)extendedBounds.Item2.X / chunkMaxSize);
        var yMax = Mathf.CeilToInt((float)extendedBounds.Item2.Y / chunkMaxSize);
        var zMax = Mathf.CeilToInt((float)extendedBounds.Item2.Z / chunkMaxSize);
        
        var chunkLength = chunkMaxSize * voxelSize;
        var chunkHalfLength = chunkLength / 2;
        var chunkMinPointX = 0f;
        var chunkMinPointY = 0f;
        var chunkMinPointZ = 0f;
        var chunkBoundsSize = new Vector3(chunkLength, chunkLength, chunkLength);

        //initialize chunks
        for (var x = 0; x < xMax; x++)
        {
            chunkMinPointX = extendedBounds.Item1.min.x + x * chunkLength;
            for (var y = 0; y < yMax; y++)
            {
                chunkMinPointY = extendedBounds.Item1.min.y + y * chunkLength;
                for (var z = 0; z < zMax; z++)
                {
                    chunkMinPointZ = extendedBounds.Item1.min.z + z * chunkLength;
                    var chunkCentre = new Vector3(chunkMinPointX + chunkHalfLength,
                        chunkMinPointY + chunkHalfLength, chunkMinPointZ + chunkHalfLength);
                    var chunkBounds = new Bounds(chunkCentre, chunkBoundsSize);

                    var collidingWith = new List<MeshFilter>();
                    _sceneObjectsBoundsOctree.GetColliding(collidingWith, chunkBounds);

                    if (collidingWith.Any())
                    {
                        yield return new MeshFiltersChunk(collidingWith.ToArray(), chunkBounds);
                    }
                }
            }
        }
        
    }

    public static (Vector3, Vector3) GetMeshesBoundsInGlobalSpace(MeshFilter[] meshFilters)
    {
        var min = Vector3.positiveInfinity;
        var max = Vector3.negativeInfinity;

        foreach (var meshFilter in meshFilters)
        {
            var bounds = meshFilter.gameObject.GetComponent<Renderer>().bounds;

            // update min and max
            min = Vector3.Min(min, bounds.min);
            max = Vector3.Max(max, bounds.max);
        }

        return (min, max);
    }
}

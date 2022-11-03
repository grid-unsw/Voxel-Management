using System.Collections.Generic;
using System.Linq;
using MeshManagement;
using UnityEngine;
using VoxelSystem;

public class SceneObjectsOctree
{
    private BoundsOctree<MeshFilter> _sceneObjectsBoundsOctree;
    public Bounds SceneBounds { get; }
    public SceneObjectsOctree(MeshFilter[] meshFilters)
    {
        SceneBounds = MeshFunctions.GetMeshesBoundsInGlobalSpace(meshFilters);

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
        var extendedBounds = VoxelFunctions.GetExtendedBounds(bounds, voxelSize);

        //check if the size is larger than 256x256x256
        if (extendedBounds.Item2.X * extendedBounds.Item2.Y * extendedBounds.Item2.Z > Constants.MaxBlockSize)
        {
            var xMax = Mathf.CeilToInt((float) extendedBounds.Item2.X / Constants.ChunkSize);
            var yMax = Mathf.CeilToInt((float) extendedBounds.Item2.Y / Constants.ChunkSize);
            var zMax = Mathf.CeilToInt((float) extendedBounds.Item2.Z / Constants.ChunkSize);

            var chunkLength = Constants.ChunkSize * voxelSize;
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
                            var meshFilters = collidingWith.ToArray();
                            var smallestBounds = VoxelFunctions.ShirnkBoundsToFitInsideChunk(MeshFunctions.GetMeshesBoundsInGlobalSpace(meshFilters),
                                    chunkBounds);
                            yield return new MeshFiltersChunk(meshFilters, smallestBounds);
                        }
                    }
                }
            }
        }
        else
        {
            var collidingWith = new List<MeshFilter>();
            _sceneObjectsBoundsOctree.GetColliding(collidingWith, bounds);
            if (collidingWith.Any())
            {
                var meshFilters = collidingWith.ToArray();
                var smallestBounds = VoxelFunctions.ShirnkBoundsToFitInsideChunk(MeshFunctions.GetMeshesBoundsInGlobalSpace(meshFilters),
                    bounds);
                yield return new MeshFiltersChunk(meshFilters, smallestBounds);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelSystem
{
    public static class Functions
    {
        public static (Bounds, Index3D) GetExtendedBounds(Vector3 minBounds, Vector3 maxBounds, float voxelSize)
        {
            var voxelSizeX2 = voxelSize * 2;
            var start = minBounds - ExtendBound(minBounds, voxelSize);
            var end = new Vector3(voxelSizeX2, voxelSizeX2, voxelSizeX2) + maxBounds - ExtendBound(maxBounds, voxelSize);
            var size = end - start;

            var w = Mathf.RoundToInt(size.x / voxelSize);
            var h = Mathf.RoundToInt(size.y / voxelSize);
            var d = Mathf.RoundToInt(size.z / voxelSize);

            var extendedBounds = new Bounds((start + end) / 2, size);

            return (extendedBounds, new Index3D(w, h, d));
        }

        public static Vector3 ExtendBound(Vector3 point, float voxelSize)
        {
            float modX = point.x % voxelSize;
            float modY = point.y % voxelSize;
            float modZ = point.z % voxelSize;
            if (point.x < 0)
            {
                modX += voxelSize;
            }
            if (point.y < 0)
            {
                modY += voxelSize;
            }
            if (point.z < 0)
            {
                modZ += voxelSize;
            }

            return new Vector3(modX, modY, modZ);
        }
        
        public static MeshFiltersChunk[] GetMeshFiltersChunks(MeshFilter[] meshFilters, float voxelSize) {

            var minMaxBounds = MinMaxBounds(meshFilters);

            var extendedBounds = Functions.GetExtendedBounds(minMaxBounds.Item1, minMaxBounds.Item2, voxelSize);

            var voxelModelSize = extendedBounds.Item2.X * extendedBounds.Item2.Y * extendedBounds.Item2.Z;

            if (voxelModelSize > 12000000)
            {
                var chunkMaxSize = (int)Mathf.Pow(2, 9);
                var xMax = Mathf.CeilToInt((float)extendedBounds.Item2.X / chunkMaxSize);
                var yMax = Mathf.CeilToInt((float)extendedBounds.Item2.Y / chunkMaxSize);
                var zMax = Mathf.CeilToInt((float)extendedBounds.Item2.Z / chunkMaxSize);

                var meshFiltersChunks = new MeshFiltersChunk[xMax*yMax*zMax];
                var chunkMaxSizeLength = chunkMaxSize * voxelSize;
                var chunkMinPointX = extendedBounds.Item1.min.x;
                var chunkMinPointY = extendedBounds.Item1.min.y;
                var chunkMinPointZ = extendedBounds.Item1.min.z;
                var chunkMaxPointX = chunkMinPointX + chunkMaxSizeLength;
                var chunkMaxPointY = chunkMinPointY + chunkMaxSizeLength;
                var chunkMaxPointZ = chunkMinPointZ + chunkMaxSizeLength;
                var chunkLenghtX = chunkMaxSizeLength;
                var chunkLenghtY = chunkMaxSizeLength;
                var chunkLenghtZ = chunkMaxSizeLength;
                var chunkSize = new Index3D(chunkMaxSize, chunkMaxSize, chunkMaxSize);

                //initialize chunks
                for (var x = 0; x < xMax; x++)
                {
                    if (chunkMaxPointX > extendedBounds.Item1.max.x)
                    {
                        chunkMaxPointX = extendedBounds.Item1.max.x;
                        chunkLenghtX = chunkMaxPointX - chunkMinPointX;
                        chunkSize.X = Mathf.RoundToInt(chunkLenghtX);
                    }

                    for (var y = 0; y < yMax; y++)
                    {
                        if (chunkMaxPointY > extendedBounds.Item1.max.y)
                        {
                            chunkMaxPointY = extendedBounds.Item1.max.y;
                            chunkLenghtY = chunkMaxPointY - chunkMinPointY;
                            chunkSize.Y = Mathf.RoundToInt(chunkLenghtY);
                        }

                        for (var z = 0; z < zMax; z++)
                        {
                            if (chunkMaxPointZ > extendedBounds.Item1.max.z)
                            {
                                chunkMaxPointZ = extendedBounds.Item1.max.z;
                                chunkLenghtZ = chunkMaxPointZ - chunkMinPointZ;
                                chunkSize.Z= Mathf.RoundToInt(chunkLenghtZ);
                            }

                            var chunkMinPoint = new Vector3(chunkMinPointX, chunkMinPointY, chunkMinPointZ);
                            var chunkMaxPoint = new Vector3(chunkMaxPointX, chunkMaxPointY, chunkMaxPointZ);

                            var chunkIndex = ArrayFunctions.Index3DTo1D(x, y, z, xMax, yMax);
                            var chunkVectorSize = new Vector3(chunkLenghtX, chunkLenghtY, chunkLenghtZ);
                            var chunkBounds = new Bounds((chunkMaxPoint + chunkMinPoint)/2, chunkVectorSize);

                            meshFiltersChunks[chunkIndex] = new MeshFiltersChunk(new List<MeshFilter>(), chunkBounds, chunkSize);

                            chunkMinPointZ = chunkMaxSize;
                            chunkMaxPointZ += chunkMaxSizeLength;
                        }
                        chunkMinPointY = chunkMaxSize;
                        chunkMaxPointY += chunkMaxSizeLength;
                        chunkMinPointZ = extendedBounds.Item1.min.z;
                        chunkMaxPointZ = chunkMinPointZ + chunkMaxSizeLength;
                    }

                    chunkMinPointX = chunkMaxPointX;
                    chunkMaxPointX += chunkMaxSizeLength;
                    chunkMinPointY = extendedBounds.Item1.min.y;
                    chunkMaxPointY = chunkMinPointY + chunkMaxSizeLength;
                }
                    
                

                foreach (var meshFilter in meshFilters)
                {
                    var chunkIndexMinPoint = (meshFilter.sharedMesh.bounds.min - extendedBounds.Item1.min) / chunkMaxSizeLength;
                    var chunkIndexMaxPoint = (meshFilter.sharedMesh.bounds.max - extendedBounds.Item1.min) / chunkMaxSizeLength;

                    var chunkIndex3DMin = new Index3D(Mathf.FloorToInt(chunkIndexMinPoint.x),
                        Mathf.FloorToInt(chunkIndexMinPoint.y), Mathf.FloorToInt(chunkIndexMinPoint.z));
                    var chunkIndex3DMax = new Index3D(Mathf.FloorToInt(chunkIndexMaxPoint.x),
                        Mathf.FloorToInt(chunkIndexMaxPoint.y), Mathf.FloorToInt(chunkIndexMaxPoint.z));

                    //meshFilter inside one chunk 
                    if (chunkIndex3DMin.X == chunkIndex3DMax.X && chunkIndex3DMin.Y == chunkIndex3DMax.Y && chunkIndex3DMin.Z == chunkIndex3DMax.Z)
                    {
                        var chunkIndex = ArrayFunctions.Index3DTo1D(chunkIndex3DMin.X, chunkIndex3DMin.Y,
                            chunkIndex3DMin.Z, xMax, yMax);
                        meshFiltersChunks[chunkIndex].MeshFilters.Add(meshFilter);
                    }
                    else
                    {
                        var chunkIndexMin = ArrayFunctions.Index3DTo1D(chunkIndex3DMin.X, chunkIndex3DMin.Y,
                            chunkIndex3DMin.Z, xMax, yMax);
                        var chunkIndexMax = ArrayFunctions.Index3DTo1D(chunkIndex3DMax.X, chunkIndex3DMax.Y,
                            chunkIndex3DMax.Z, xMax, yMax);

                        for (int i = chunkIndexMin; i <= chunkIndexMax; i++)
                        {
                            meshFiltersChunks[i].MeshFilters.Add(meshFilter);
                        }
                    }
                }

                return meshFiltersChunks;
            }
            else
            {
                return new MeshFiltersChunk[]
                    {new MeshFiltersChunk(meshFilters.ToList(), extendedBounds.Item1, extendedBounds.Item2)};
            }
        }

        public static (Vector3, Vector3) MinMaxBounds(MeshFilter[] meshFilters)
        {
            var min = Vector3.positiveInfinity;
            var max = Vector3.negativeInfinity;

            foreach (var meshFilter in meshFilters)
            {
                // update min and max
                min = Vector3.Min(min, meshFilter.sharedMesh.bounds.min);
                max = Vector3.Max(max, meshFilter.sharedMesh.bounds.max);
            }

            return (min, max);
        }

    }
}
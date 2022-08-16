using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MeshManagement;

namespace VoxelSystem
{
    public static class VoxelFunctions
    {
        /// <summary>
        /// Transfer multi value voxel array to integer array.
        /// </summary>
        /// <param name="voxels">voxel array having a list of objects per voxel.</param>
        /// <param name="isFirstElementMain">keep only the first object.</param>
        public static int[] GetIntegerArray(List<VoxelObject>[] voxels, bool isFirstElementMain = false)
        {
            var integerVoxels = new int[voxels.Length];

            if (isFirstElementMain)
            {
                for (var i = 0; i < voxels.Length; i++)
                {
                    var voxel = voxels[i];
                    if (voxel.Any())
                    {
                        integerVoxels[i] = voxel[0].Id;
                    }
                }
            }
            else
            {
                for (var i = 0; i < voxels.Length; i++)
                {
                    var voxel = voxels[i];
                    if (voxel.Any()) 
                    {
                        if (voxel.Count > 1)
                        {
                            integerVoxels[i] = 0;
                        }
                        else
                        {
                            integerVoxels[i] = voxel[0].Id;
                        }
                    }
                }
            }

            return integerVoxels;
        }

        /// <summary>
        /// Transfer Voxel_t array to binary array.
        /// </summary>
        /// <param name="voxels">voxel_t array.</param>
        public static bool[] GetBinaryArray(Voxel_t[] voxels)
        {
            var binaryVoxels = new bool[voxels.Length];

            for (int i = 0; i < voxels.Length; i++)
            {
                var voxel = voxels[i];
                if (voxel.fill > 0)
                {
                    binaryVoxels[i] = true;
                }
            }
            return binaryVoxels;
        }

        public static IEnumerable<GPUVoxelData> GetVoxelData(SceneObjectsOctree sceneObjectsOctree, ComputeShader voxelizer, float voxelSize, VoxelizationGeomType voxelizationGeom)
        {
            foreach (var voxelModelChunk in sceneObjectsOctree.GetMeshFilterChunks(voxelSize, sceneObjectsOctree.SceneBounds))
            {
                var combinedMeshes = MeshFunctions.CombineMeshes(voxelModelChunk.MeshFilters);

                yield return GPUVoxelizer.Voxelize(voxelizer, combinedMeshes, voxelModelChunk.Bounds, voxelSize, voxelizationGeom);
            }
        }

        public static MultiValueVoxelModel GetMultiValueVoxelData(MeshFilter[] meshFilters, ComputeShader voxelizer, float voxelSize, VoxelizationGeomType voxelizationGeom, int maxColors = 10000000)
        {
            var minMaxBounds = GetMeshesBoundsInGlobalSpace(meshFilters);

            var extendedBounds = GetExtendedBounds(minMaxBounds.Item1, minMaxBounds.Item2, voxelSize);

            return VoxeliseModel(voxelizer, meshFilters.ToList(), extendedBounds.Item2, extendedBounds.Item1, voxelSize, voxelizationGeom, maxColors);
        }

        private static MultiValueVoxelModel VoxeliseModel(ComputeShader voxelizer, List<MeshFilter> meshFilters, Index3D modelSizes,
            Bounds modelBounds, float voxelSize, VoxelizationGeomType voxelizationGeom, int maxColors)
        {
            var wModel = modelSizes.X;
            var hModel = modelSizes.Y;
            var dModel = modelSizes.Z;

            var voxels = new List<VoxelObject>[wModel * hModel * dModel];

            var uniqueColors = VisualizationFunctions.GetUniqueColors(maxColors);
            var colorNum = 0;

            var pivotPointModel = modelBounds.min;

            foreach (var meshFilter in meshFilters)
            {
                var data = GPUVoxelizer.Voxelize(voxelizer, MeshFunctions.TransformMeshLocalToWorld(meshFilter), voxelSize, voxelizationGeom);
                var voxelsArray = data.GetData();

                var pivotPointObject = data.Bounds.min;
                var wDiff = Mathf.RoundToInt((pivotPointObject.x - pivotPointModel.x) / voxelSize);
                var hDiff = Mathf.RoundToInt((pivotPointObject.y - pivotPointModel.y) / voxelSize);
                var dDiff = Mathf.RoundToInt((pivotPointObject.z - pivotPointModel.z) / voxelSize);

                var voxelObject = new VoxelObject()
                {
                    Id = meshFilter.gameObject.GetInstanceID(),
                    VoxelColor = uniqueColors[colorNum]
                };

                colorNum++;
                if (maxColors == colorNum)
                {
                    colorNum = 0;
                }

                for (var i = 0; i < voxelsArray.Length; i++)
                {
                    var voxel = voxelsArray[i];
                    if (voxel.fill > 0)
                    {
                        var voxel3dIndex = ArrayFunctions.Index1DTo3D(i, data.Width, data.Height);

                        var voxelModelIndex = ArrayFunctions.Index3DTo1D(voxel3dIndex.X + wDiff,
                            voxel3dIndex.Y + hDiff, voxel3dIndex.Z + dDiff, wModel, hModel);

                        UpdateVoxel(ref voxels[voxelModelIndex], voxelObject);
                    }
                }

                data.Dispose();
            }

            return new MultiValueVoxelModel(voxels, wModel, hModel, dModel, modelBounds, voxelSize);
        }
        
        private static void UpdateVoxel(ref List<VoxelObject> voxel, VoxelObject objectId)
        {
            if (voxel == null)
            {
                voxel = new List<VoxelObject>() { objectId };
            }
            else
            {
                voxel.Add(objectId);
            }
        }

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
        /*
        public static MeshFiltersChunk[] GetMeshFiltersChunks(MeshFilter[] meshFilters, float voxelSize) {

            var meshesBounds = GetMeshesBoundsInGlobalSpace(meshFilters);

            var extendedBounds = GetExtendedBounds(meshesBounds.Item1, meshesBounds.Item2, voxelSize);

            var voxelModelSize = extendedBounds.Item2.X * extendedBounds.Item2.Y * extendedBounds.Item2.Z;

            if (voxelModelSize > Constants.MaxBlockSize)
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

                            meshFiltersChunks[chunkIndex] = new MeshFiltersChunk(new List<MeshFilter>(), chunkBounds);

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

            return new MeshFiltersChunk[]
                {new MeshFiltersChunk(meshFilters.ToList(), extendedBounds.Item1)};
        }
        */
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
}
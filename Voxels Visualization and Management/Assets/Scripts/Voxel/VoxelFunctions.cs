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
            var bounds = MeshFunctions.GetMeshesBoundsInGlobalSpace(meshFilters);

            var extendedBounds = GetExtendedBounds(bounds, voxelSize);

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

        public static (Bounds, Index3D) GetExtendedBounds(Bounds bounds, float voxelSize)
        {
            var voxelSizeX2 = voxelSize * 2;
            var start = bounds.min - ExtendBound(bounds.min, voxelSize);
            var end = new Vector3(voxelSizeX2, voxelSizeX2, voxelSizeX2) + bounds.max - ExtendBound(bounds.max, voxelSize);
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

        public static Bounds ShirnkBoundsToFitInsideChunk(Bounds bounds, Bounds chunkBounds)
        {
            var min = Vector3.Max(chunkBounds.min, bounds.min);
            var max = Vector3.Min(chunkBounds.max, bounds.max);

            return new Bounds((min + max) / 2, max - min);
        }
    }
}
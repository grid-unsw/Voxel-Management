using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using VoxelSystem.PointCloud;
using VoxelSystem.IO;
using Material = UnityEngine.Material;

namespace VoxelSystem
{
    public class VoxelisationManager : MonoBehaviour
    {
        enum voxelisationType
        {
            surface,
            volume
        }

        enum voxelVisualisationType
        {
            cube,
            quad
        }

        enum pointCloudType
        {
            pts
        }

        enum delimiter
        {
            [Description(" ")]
            space,
            [Description(",")]
            comma,
            [Description(";")]
            semicolon
        }

        [Header("Voxelisation")] 
        [SerializeField] private ComputeShader _voxelizer;
        [SerializeField] private float _voxelSize = 0.25f;
        [SerializeField] private voxelisationType _type;
        [SerializeField] private bool _useUv = false;
        [SerializeField] public bool hasColor;

        [Header("Visualisation - Mesh")] 
        public bool voxelMesh;
        [SerializeField] private int _gridSplittingSize;
        [SerializeField] private Material _voxelMaterial;
        [SerializeField] private int _materialsNumber = 16;

        [Header("Visualisation - Dots")]
        [SerializeField] private bool _dotsVisualisation;
        [SerializeField] private GameObject _quadPrefab;
        private BlobAssetStore _blobAssetStore;
        [SerializeField] private bool _turnOffModel;
        private EntityManager _entityManager;

        [Header("Visualisation - VFX")]
        public bool vfxVisualisation;
        [SerializeField] private Color _vfxColor;
        [SerializeField] private voxelVisualisationType _visType;

        [Header("Export Voxels")] [SerializeField]
        private pointCloudType _fileType;
        [SerializeField] private string _filePath;
        [SerializeField] private bool _color;
        [SerializeField] private delimiter _delimiter;

        private const int Mesh16BitBufferVertexLimit = 65535;

        void Start()
        {
            //EditorPrefs.SetBool("BurstCompilation", false);
            if (_dotsVisualisation)
            {
                var meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
                var voxelsData = GetVoxelData(meshFilters);

                CreateQuads(voxelsData);
                transform.gameObject.SetActive(false);
            }
        }

        public GPUVoxelData GetVoxelData(MeshFilter[] meshFilters)
        {
            var minMaxBounds = MinMaxBounds(meshFilters);

            var extendedBounds = GPUVoxelizer.GetExtendedBounds(minMaxBounds.Item1, minMaxBounds.Item2, _voxelSize);

            var combinedMeshes = CombineMeshes(meshFilters);

            return GPUVoxelizer.Voxelize(_voxelizer, combinedMeshes, extendedBounds, _voxelSize, false);
        }

        public MultiValueVoxelModel GetMultiValueVoxelData(MeshFilter[] meshFilters)
        {
            var minMaxBounds = MinMaxBounds(meshFilters);

            var extendedBounds = GPUVoxelizer.GetExtendedBounds(minMaxBounds.Item1, minMaxBounds.Item2, _voxelSize);

            return VoxeliseModel(_voxelizer, meshFilters.ToDictionary(x => x.gameObject.name, x => x), extendedBounds.Item2, extendedBounds.Item1.min, _voxelSize);
        }

        private (Vector3, Vector3) MinMaxBounds(MeshFilter[] meshFilters)
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

        public Mesh CombineMeshes(MeshFilter[] meshFilters)
        {
            // First MeshFilter belongs to this GameObject so we don't need it:
            var combineInstances = new CombineInstance[meshFilters.Length];

            var verticesLength = 0;
            var i = 0;
            while (i < meshFilters.Length) // Skip first MeshFilter belongs to this GameObject in this loop.
            {
                combineInstances[i].subMeshIndex = 0;
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
                verticesLength += combineInstances[i].mesh.vertices.Length;
                i++;
            }

            // Create Mesh from combineInstances:
            Mesh combinedMesh = new Mesh();
            combinedMesh.name = name;

            // If it will be over 65535 then use the 32 bit index buffer:
            if (verticesLength > Mesh16BitBufferVertexLimit)
            {
                combinedMesh.indexFormat =
                    UnityEngine.Rendering.IndexFormat.UInt32; // Only works on Unity 2017.3 or higher.
            }
            
            combinedMesh.CombineMeshes(combineInstances);

            return combinedMesh;
        }

        public MultiValueVoxelModel VoxeliseModel(ComputeShader voxelizer, Dictionary<string, MeshFilter> meshFilters, Index3D modelSizes,
            Vector3 modelPivotPoint, float voxelSize)
        {
            var wModel = modelSizes.X;
            var hModel = modelSizes.Y;
            var dModel = modelSizes.Z;

            var voxels = new List<int>[wModel * hModel * dModel];

            foreach (var meshFilter in meshFilters.Values)
            {
                var data = GPUVoxelizer.Voxelize(voxelizer, meshFilter.sharedMesh, voxelSize, false);
                var voxelsArray = data.GetData();

                var wDiff = Mathf.RoundToInt((data.PivotPoint.x - modelPivotPoint.x) / voxelSize);
                var hDiff = Mathf.RoundToInt((data.PivotPoint.y - modelPivotPoint.y) / voxelSize);
                var dDiff = Mathf.RoundToInt((data.PivotPoint.z - modelPivotPoint.z) / voxelSize);

                for (int i = 0; i < voxelsArray.Length; i++)
                {
                    var voxel = voxelsArray[i];
                    if (voxel.fill > 0)
                    {
                        var voxel3dIndex = ArrayFunctions.Index1DTo3D(i, data.Width, data.Height);

                        var voxelModelIndex = ArrayFunctions.Index3DTo1D(voxel3dIndex.X + wDiff,
                            voxel3dIndex.Y + hDiff, voxel3dIndex.Z + dDiff, wModel, hModel);

                        UpdateVoxel(ref voxels[voxelModelIndex], meshFilter.gameObject.GetInstanceID());
                    }
                }

                data.Dispose();
            }

            return new MultiValueVoxelModel(voxels, wModel, hModel, dModel, modelPivotPoint, voxelSize,
                meshFilters.Keys.ToArray());
        }

        private void UpdateVoxel(ref List<int> voxel, int objectId)
        {
            if (voxel == null)
            {
                voxel = new List<int>() { objectId };
            }
            else
            {
                voxel.Add(objectId);
            }
        }

        public void BuildMesh(GPUVoxelData voxelsData)
        {
            var parentGameObject = new GameObject("VoxelMesh_" + _voxelSize + "m");
            var voxelsChunks = VoxelMesh.Build(voxelsData, _voxelSize, _gridSplittingSize, _useUv);
            var minusHalfVoxel = -_voxelSize / 2;

            foreach (var voxelsChunk in voxelsChunks)
            {
                if (voxelsChunk == null) continue;

                var childGameObject = new GameObject(
                    "chunk_" + voxelsChunk.XMin + "_" + voxelsChunk.YMin + "_" + voxelsChunk.ZMin,
                    typeof(MeshFilter),
                    typeof(MeshRenderer));

                childGameObject.GetComponent<MeshFilter>().sharedMesh = voxelsChunk.Mesh;
                childGameObject.transform.position = new Vector3(minusHalfVoxel, minusHalfVoxel, minusHalfVoxel);
                childGameObject.GetComponent<Renderer>().material = _voxelMaterial;
                childGameObject.transform.parent = parentGameObject.transform;
            }
        }

        public void BuildColorMesh(MultiValueVoxelModel voxelsData)
        {
            var parentGameObject = new GameObject("ColorVoxelMesh_" + _voxelSize + "m");
            var voxelsChunks = VoxelMesh.BuildWithColor(voxelsData, _voxelSize, _gridSplittingSize, _materialsNumber, _useUv );
            var minusHalfVoxel = -_voxelSize / 2;

            foreach (var voxelsChunk in voxelsChunks)
            {
                if (voxelsChunk == null) continue;

                var childGameObject = new GameObject(
                    "chunk_" + voxelsChunk.XMin + "_" + voxelsChunk.YMin + "_" + voxelsChunk.ZMin,
                    typeof(MeshFilter),
                    typeof(MeshRenderer));

                childGameObject.GetComponent<MeshFilter>().sharedMesh = voxelsChunk.Mesh;
                childGameObject.transform.position = new Vector3(minusHalfVoxel, minusHalfVoxel, minusHalfVoxel);
                childGameObject.GetComponent<Renderer>().materials = voxelsChunk.Materials;
                childGameObject.transform.parent = parentGameObject.transform;
            }
        }


        public void VisualiseVfxVoxels(GPUVoxelData voxelsData)
        {
            if (_visType == voxelVisualisationType.cube)
            {
                var voxelLayerPrefab = Resources.Load("Prefabs/VoxelLayer") as GameObject;

                var voxelsLayerGameObject = Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
                voxelsLayerGameObject.name = "VFX_cubes_" + _voxelSize;
                voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>().SetVoxelParticles(voxelsData, _voxelSize, _vfxColor);
            }
            else
            {
                var voxelLayerPrefab = Resources.Load("Prefabs/QuadLayer") as GameObject;

                var voxelsLayerGameObject = Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
                voxelsLayerGameObject.name = "VFX_quads_" + _voxelSize;
                voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>()
                    .SetQuadParticles(voxelsData, _voxelSize, _vfxColor);
            }
        }

        public void VisualiseVfxColorVoxels(MultiValueVoxelModel voxelModel)
        {
            var voxelLayerPrefab = Resources.Load("Prefabs/QuadWithColorLayer") as GameObject;

            var voxelsLayerGameObject = Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
            voxelsLayerGameObject.name = "VFX_color_quads_" + _voxelSize;
            voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>()
                .SetColorQuadParticles(voxelModel, _voxelSize);
        }

        public void ExportPts(GPUVoxelData voxelsData)
        {
            var voxelsArray = voxelsData.GetData();

            var halfVoxelSize = _voxelSize / 2;
            var pivotVoxelCentroid =
                voxelsData.PivotPoint + new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

            if (_color)
            {
                var points = new List<PointColour>();

                for (int i = 0; i < voxelsArray.Length; i++)
                {
                    var voxelIndex = ArrayFunctions.Index1DTo3D(i, voxelsData.Width, voxelsData.Height);
                    var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * _voxelSize, voxelIndex.Y * _voxelSize, voxelIndex.Z * _voxelSize);
                    //points.Add(new PointColour(point));
                    ;
                }
                //Output.WritePts(points.ToArray(), _filePath, _delimiter.GetDescription())
            }
            else
            {
                var points = new List<Point>();

                var i = 0;
                foreach (var voxel in voxelsArray)
                {
                    if (voxel.fill > 0)
                    {
                        var voxelIndex = ArrayFunctions.Index1DTo3D(i, voxelsData.Width, voxelsData.Height);
                        var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * _voxelSize,
                            voxelIndex.Y * _voxelSize, voxelIndex.Z * _voxelSize);
                        points.Add(new Point(point));
                    }
                    i++;
                }
                Output.WritePts(points.ToArray(), Color.green, _filePath, _delimiter.GetDescription());
            }
        }

        private void CreateQuads(GPUVoxelData voxelsData)
        {
            _blobAssetStore = new BlobAssetStore();
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            _quadPrefab.transform.localScale = new Vector3(_voxelSize, _voxelSize, 1f);

            var leftRotation = Quaternion.Euler(0, 90f, 0);
            var rightRotation = Quaternion.Euler(0, -90f, 0);
            var downRotation = Quaternion.Euler(-90f, 0, 0);
            var upRotation = Quaternion.Euler(90f, 0, 0);
            var forwardRotation = Quaternion.Euler(0, 180f, 0);
            var backRotation = Quaternion.Euler(0, 0, 0);
            
            var quadPrefab = GameObject.Instantiate(_quadPrefab);

            var quadEntity = Unity.Entities.GameObjectConversionUtility.ConvertGameObjectHierarchy(quadPrefab, settings);
            
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //turn off lighting
            /*
            entityManager.RemoveComponent(quadEntity, typeof(BuiltinMaterialPropertyUnity_LightData));
            */
            var countQuads = 0;

            var voxels = voxelsData.GetData();
            var halfVoxelSize = _voxelSize / 2;
            var pivotVoxelPoint = voxelsData.PivotPoint - new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);
            var m = 0;
            for (int k = 0; k < voxelsData.Depth; k++)
                for (int j = 0; j < voxelsData.Height; j++)
                    for (int i = 0; i < voxelsData.Width; i++)
                    {
                        var v = voxels[m];
                        if (v.fill > 0)
                        {
                            var iLeft = i - 1;
                            var iRight = i + 1;
                            var jDown = j - 1;
                            var jUp = j + 1;
                            var kBack = k - 1;
                            var kForward = k + 1;
                            
                            if (iLeft != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(iLeft, j, k, voxelsData.Width, voxelsData.Height);
                                var voxelLeft = voxels[voxelIndex];

                                if (voxelLeft.fill == 0)
                                {
                                    var trans = new float3(pivotVoxelPoint.x + i * _voxelSize,
                                        voxelsData.PivotPoint.y + j * _voxelSize,
                                        voxelsData.PivotPoint.z + k * _voxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, leftRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(pivotVoxelPoint.x + i * _voxelSize,
                                    voxelsData.PivotPoint.y + j * _voxelSize,
                                    voxelsData.PivotPoint.z + k * _voxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, leftRotation);

                                countQuads++;
                            }
                            
                            if (iRight != voxelsData.Width)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(iRight, j, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(pivotVoxelPoint.x + (i + 1) * _voxelSize,
                                        voxelsData.PivotPoint.y + j * _voxelSize,
                                        voxelsData.PivotPoint.z + k * _voxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, rightRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(pivotVoxelPoint.x + (i + 1) * _voxelSize,
                                    voxelsData.PivotPoint.y + j * _voxelSize,
                                    voxelsData.PivotPoint.z + k * _voxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, rightRotation);

                                countQuads++;
                            }

                            if (jDown != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, jDown, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                        pivotVoxelPoint.y + j * _voxelSize,
                                        voxelsData.PivotPoint.z + k * _voxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, downRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                    pivotVoxelPoint.y + j * _voxelSize,
                                    voxelsData.PivotPoint.z + k * _voxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, downRotation);

                                countQuads++;
                            }

                            if (jUp != voxelsData.Height)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, jUp, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                        pivotVoxelPoint.y + (j + 1) * _voxelSize,
                                        voxelsData.PivotPoint.z + k * _voxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, upRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                    pivotVoxelPoint.y + (j + 1) * _voxelSize,
                                    voxelsData.PivotPoint.z + k * _voxelSize);

                                UpdateEntityPositionAndRotation(quadEntity, trans, upRotation);

                                countQuads++;
                            }

                            if (kBack != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kBack, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                        voxelsData.PivotPoint.y + j * _voxelSize,
                                        pivotVoxelPoint.z + k * _voxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, backRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                    voxelsData.PivotPoint.y + j * _voxelSize,
                                    pivotVoxelPoint.z + k * _voxelSize);

                                UpdateEntityPositionAndRotation(quadEntity, trans, backRotation);

                                countQuads++;
                            }
                            
                            if (kForward != voxelsData.Depth)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kForward, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                        voxelsData.PivotPoint.y + j * _voxelSize,
                                        pivotVoxelPoint.z + (k + 1) * _voxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, forwardRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.PivotPoint.x + i * _voxelSize,
                                    voxelsData.PivotPoint.y + j * _voxelSize,
                                    pivotVoxelPoint.z + (k + 1) * _voxelSize);

                                UpdateEntityPositionAndRotation(quadEntity, trans, forwardRotation);

                                countQuads++;
                            }
                        }

                        m++;
                    }

            Debug.Log(countQuads);

            voxelsData.Dispose();
            DestroyImmediate(quadPrefab);

            _entityManager.DestroyEntity(quadEntity);
        }

        private void UpdateEntityPositionAndRotation(Entity entity, float3 trans, Quaternion rot)
        {
            var myEntity = _entityManager.Instantiate(entity);

            _entityManager.SetComponentData(myEntity, new Translation
            {
                Value = trans
            });
            _entityManager.SetComponentData(myEntity, new Rotation
            {
                Value = rot
            });
        }
        
        private void OnDestroy()
        {
            if (_dotsVisualisation)
            {
                _blobAssetStore.Dispose();
            }
        }
    }
}
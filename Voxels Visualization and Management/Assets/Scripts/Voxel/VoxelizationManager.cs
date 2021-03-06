using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VoxelSystem.PointCloud;
using VoxelSystem.IO;
using Debug = UnityEngine.Debug;

namespace VoxelSystem
{
    public class VoxelizationManager : MonoBehaviour
    {
        [Header("Voxelisation")] 
        [HideInInspector] public ComputeShader Voxelizer;
        [SerializeField] private float _voxelSize = 0.25f;
        [SerializeField] public VoxelizationGeomType VoxelizationGeom = VoxelizationGeomType.surface;

        [SerializeField] public string FilePathImport { get; set;}
        //[SerializeField] private bool _useUv = false;
        [SerializeField] public bool KeepObjectId { get; set; }
        [SerializeField] public Color VoxelColor { get; set; } = Color.white;

        //mesh visualization
        [SerializeField] public bool VisualizeMesh { get; set; }
        [SerializeField] public int GridSplittingSize { get; set; } = 32;
        [SerializeField] public int MaxUsedColors { get; set; } = 10;

        //vfx visualization
        [SerializeField] public bool VfxVisualisation { get; set; }
        [SerializeField] public VoxelVisualizationType VfxVisType { get; set; } = VoxelVisualizationType.quad;

        //SVO
        [SerializeField] public bool SvoVisualization { get; set; }

        //export
        [SerializeField] public Vector3 GeomOffset { get; set; }
        //point clouds
        [SerializeField] public bool ExportAsPointCloud { get; set; }
        [SerializeField] public PointCloudExportType FileType { get; set; } = PointCloudExportType.pts;
        [SerializeField] public string FilePathExport { get; set; }
        [SerializeField] public DelimiterType Delimiter { get; set; } = DelimiterType.space;
        //database
        [SerializeField] public bool ExportToDatabase { get; set; }
        [SerializeField] public string TableName { get; set; } = "";
        [SerializeField] public bool Truncate { get; set; }
        [SerializeField] public DatabaseExportType DBGeomExportType { get; set; }

        //storage
        //cpu
        [SerializeField] public bool OctreeVisualisation { get; set; }

        //dots visualization octree
        [SerializeField] public bool DotsVisualisation { get; set; }
        private GameObject _quadPrefab;
        private BlobAssetStore _blobAssetStore;
        private bool _turnOffModel;
        private EntityManager _entityManager;


        private const int Mesh16BitBufferVertexLimit = 65535;

        void Start()
        {
            //EditorPrefs.SetBool("BurstCompilation", false);
            /*
            if (_dotsVisualisation)
            {
                var meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
                var voxelsData = GetVoxelData(meshFilters);

                CreateQuads(voxelsData);
                transform.gameObject.SetActive(false);
            }
            */
        }

        public IEnumerable<GPUVoxelData> GetVoxelData(MeshFilter[] meshFilters)
        {
            var voxelModelChunks = Functions.GetMeshFiltersChunks(meshFilters, _voxelSize);

            foreach (var voxelModelChunk in voxelModelChunks)
            {
                if (voxelModelChunk.MeshFilters.Any())
                {
                    var combinedMeshes = CombineMeshes(voxelModelChunk.MeshFilters.ToArray());

                    yield return GPUVoxelizer.Voxelize(Voxelizer, combinedMeshes, voxelModelChunk.Bounds, _voxelSize, VoxelizationGeom);
                }

                yield return null;
            }
        }

        public MultiValueVoxelModel GetMultiValueVoxelData(MeshFilter[] meshFilters)
        {
            var minMaxBounds = Functions.GetMeshesBoundsInGlobalSpace(meshFilters);

            var extendedBounds = Functions.GetExtendedBounds(minMaxBounds.Item1, minMaxBounds.Item2, _voxelSize);

            return VoxeliseModel(Voxelizer, meshFilters.ToList(), extendedBounds.Item2, extendedBounds.Item1.min, _voxelSize);
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
            var combinedMesh = new Mesh
            {
                name = name
            };

            // If it will be over 65535 then use the 32 bit index buffer:
            if (verticesLength > Mesh16BitBufferVertexLimit)
            {
                combinedMesh.indexFormat =
                    UnityEngine.Rendering.IndexFormat.UInt32; // Only works on Unity 2017.3 or higher.
            }
            
            combinedMesh.CombineMeshes(combineInstances);

            return combinedMesh;
        }
        public Mesh TransformMeshLocalToWorld(Mesh mesh)
        {
            var newVertices = new Vector3[mesh.vertexCount];

            for (var i = 0; i < mesh.vertexCount; i++)
            {
                var vertex = transform.localToWorldMatrix.MultiplyPoint3x4(mesh.vertices[i]);
                newVertices[i] = vertex;
            }

            var newMesh = new Mesh()
            {
                vertices = newVertices,
                triangles = mesh.triangles
            };

            return newMesh;
        }

        public MultiValueVoxelModel VoxeliseModel(ComputeShader voxelizer, List<MeshFilter> meshFilters, Index3D modelSizes,
            Vector3 modelPivotPoint, float voxelSize)
        {
            var wModel = modelSizes.X;
            var hModel = modelSizes.Y;
            var dModel = modelSizes.Z;

            var voxels = new List<VoxelObject>[wModel * hModel * dModel];

            var uniqueColors = VisualizationFunctions.GetUniqueColors(MaxUsedColors);
            var colorNum = 0;

            foreach (var meshFilter in meshFilters)
            {
                var data = GPUVoxelizer.Voxelize(voxelizer, TransformMeshLocalToWorld(meshFilter.sharedMesh), voxelSize, VoxelizationGeom);
                var voxelsArray = data.GetData();

                var wDiff = Mathf.RoundToInt((data.PivotPoint.x - modelPivotPoint.x) / voxelSize);
                var hDiff = Mathf.RoundToInt((data.PivotPoint.y - modelPivotPoint.y) / voxelSize);
                var dDiff = Mathf.RoundToInt((data.PivotPoint.z - modelPivotPoint.z) / voxelSize);

                var voxelObject = new VoxelObject()
                {
                    Id = meshFilter.gameObject.GetInstanceID(),
                    VoxelColor = uniqueColors[colorNum]
                };

                colorNum++;
                if (MaxUsedColors == colorNum)
                {
                    colorNum = 0;
                }

                for (int i = 0; i < voxelsArray.Length; i++)
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

            return new MultiValueVoxelModel(voxels, wModel, hModel, dModel, modelPivotPoint, voxelSize);
        }

        private void UpdateVoxel(ref List<VoxelObject> voxel, VoxelObject objectId)
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

        public void BuildMesh(Voxel_t[] voxels, int width, int height, int depth)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var parentGameObject = new GameObject("VoxelMesh_" + _voxelSize + "m");
            var voxelsChunks = VoxelMesh.Build(voxels, width, height, depth, _voxelSize, GridSplittingSize);
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
                childGameObject.GetComponent<Renderer>().material.color = VoxelColor;
                childGameObject.transform.parent = parentGameObject.transform;
            }
            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Debug.Log(Convert.ToInt32(stopwatchElapsed.TotalMilliseconds));
        }

        public void BuildColorMesh(MultiValueVoxelModel voxelsData)
        {

            var parentGameObject = new GameObject("ColorVoxelMesh_" + _voxelSize + "m");
            var voxelsChunks = VoxelMesh.BuildWithColor(voxelsData, _voxelSize, GridSplittingSize, MaxUsedColors );
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

        public void ExportToPostgres(Voxel_t[] voxels, int width, int height, Vector3 pivotPoint)
        {
            DBexport.ExportVoxels(voxels, width, height, pivotPoint+GeomOffset, _voxelSize, TableName, Truncate);
        }

        public void ExportToPostgres(MultiValueVoxelModel voxelModel)
        {
            DBexport.ExportVoxels(voxelModel, voxelModel.Width, voxelModel.Height, voxelModel.PivotPoint + GeomOffset, _voxelSize, TableName, Truncate);
        }


        public void VisualiseVfxVoxels(Voxel_t[] voxels, int width, int height, int depth, Vector3 pivotPoint)
        {
            if (VfxVisType == VoxelVisualizationType.cube)
            {

                var voxelLayerPrefab = Resources.Load("Prefabs/VoxelLayer") as GameObject;

                var voxelsLayerGameObject = Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
                voxelsLayerGameObject.name = "VFX_cubes_" + _voxelSize;
                voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>().SetVoxelParticles(voxels, width, height, pivotPoint, _voxelSize, VoxelColor);

            }
            else
            {

                var voxelLayerPrefab = Resources.Load("Prefabs/QuadLayer") as GameObject;

                var voxelsLayerGameObject = Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
                voxelsLayerGameObject.name = "VFX_quads_" + _voxelSize;
                voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>()
                    .SetQuadParticles(voxels, width, height, depth, pivotPoint, _voxelSize, VoxelColor);

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

        public void ExportPts(Voxel_t[] voxels, int width, int height, Vector3 pivotPoint)
        {

            var halfVoxelSize = _voxelSize / 2;
            var pivotVoxelCentroid = pivotPoint + new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

            var points = new List<Point>();

            var i = 0;
            foreach (var voxel in voxels)
            {
                if (voxel.fill > 0)
                {
                    var voxelIndex = ArrayFunctions.Index1DTo3D(i, width, height);
                    var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * _voxelSize,
                        voxelIndex.Y * _voxelSize, voxelIndex.Z * _voxelSize);
                    points.Add(new Point(point));
                }
                i++;
            }
            Output.WritePts(points, Color.green, FilePathExport, Delimiter.GetDescription(), true);
            
        }

        public void ExportPts(MultiValueVoxelModel voxelModel)
        {
            var halfVoxelSize = _voxelSize / 2;
            var pivotVoxelCentroid =
                voxelModel.PivotPoint + new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

            var points = new List<PointColour>();

            for (int i = 0; i < voxelModel.Voxels.Length; i++)
            {
                var voxelIndex = ArrayFunctions.Index1DTo3D(i, voxelModel.Width, voxelModel.Height);
                var voxel = voxelModel.Voxels[i];

                if (voxel != null)
                {
                    var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * _voxelSize, voxelIndex.Y * _voxelSize,
                        voxelIndex.Z * _voxelSize);
                    points.Add(new PointColour(point, voxel[0].VoxelColor*256));
                }
            }

            Output.WritePts(points.ToArray(), FilePathExport, Delimiter.GetDescription());
        }

        private void OptimiseMeshes(GPUVoxelData voxelsData)
        {
            var voxelsArray = voxelsData.GetData();
            var filter = new bool[3, 3] { {false, true, false}, { true, true, true }, { false, true, false }};
            //var connectedComponents = ArrayFunctions.GetConnectedComponents2D(filter);
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
            
            //entityManager.RemoveComponent(quadEntity, typeof(BuiltinMaterialPropertyUnity_LightData));
            
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
            if (DotsVisualisation)
            {
                _blobAssetStore.Dispose();
            }
        }
    }
}
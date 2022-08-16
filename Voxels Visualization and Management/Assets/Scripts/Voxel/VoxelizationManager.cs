using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VoxelSystem
{
    public class VoxelizationManager : MonoBehaviour
    {
        [Header("Voxelisation")] 
        [HideInInspector] public ComputeShader Voxelizer;
        [SerializeField] public float VoxelSize = 0.25f;
        [SerializeField] public VoxelizationGeomType VoxelizationGeom = VoxelizationGeomType.surface;

        [SerializeField] public string FilePathImport { get; set;}
        //[SerializeField] private bool _useUv = false;
        [SerializeField] public bool KeepObjectId { get; set; }
        [SerializeField] public Color VoxelColor { get; set; } = Color.white;

        //mesh visualization
        [SerializeField] public bool VisualizeMesh { get; set; }
        [SerializeField] public int GridSplittingSize { get; set; } = 32;
        [SerializeField] public int MaxUsedColors { get; set; } = 20;

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
        
        private void CreateQuads(GPUVoxelData voxelsData)
        {
            _blobAssetStore = new BlobAssetStore();
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            _quadPrefab.transform.localScale = new Vector3(VoxelSize, VoxelSize, 1f);

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
            var halfVoxelSize = VoxelSize / 2;
            var pivotVoxelPoint = voxelsData.Bounds.min - new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);
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
                                    var trans = new float3(pivotVoxelPoint.x + i * VoxelSize,
                                        voxelsData.Bounds.min.y + j * VoxelSize,
                                        voxelsData.Bounds.min.z + k * VoxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, leftRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(pivotVoxelPoint.x + i * VoxelSize,
                                    voxelsData.Bounds.min.y + j * VoxelSize,
                                    voxelsData.Bounds.min.z + k * VoxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, leftRotation);

                                countQuads++;
                            }
                            
                            if (iRight != voxelsData.Width)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(iRight, j, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(pivotVoxelPoint.x + (i + 1) * VoxelSize,
                                        voxelsData.Bounds.min.y + j * VoxelSize,
                                        voxelsData.Bounds.min.z + k * VoxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, rightRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(pivotVoxelPoint.x + (i + 1) * VoxelSize,
                                    voxelsData.Bounds.min.y + j * VoxelSize,
                                    voxelsData.Bounds.min.z + k * VoxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, rightRotation);

                                countQuads++;
                            }

                            if (jDown != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, jDown, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                        pivotVoxelPoint.y + j * VoxelSize,
                                        voxelsData.Bounds.min.z + k * VoxelSize);
                                    UpdateEntityPositionAndRotation(quadEntity, trans, downRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                    pivotVoxelPoint.y + j * VoxelSize,
                                    voxelsData.Bounds.min.z + k * VoxelSize);
                                UpdateEntityPositionAndRotation(quadEntity, trans, downRotation);

                                countQuads++;
                            }

                            if (jUp != voxelsData.Height)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, jUp, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                        pivotVoxelPoint.y + (j + 1) * VoxelSize,
                                        voxelsData.Bounds.min.z + k * VoxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, upRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                    pivotVoxelPoint.y + (j + 1) * VoxelSize,
                                    voxelsData.Bounds.min.z + k * VoxelSize);

                                UpdateEntityPositionAndRotation(quadEntity, trans, upRotation);

                                countQuads++;
                            }

                            if (kBack != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kBack, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                        voxelsData.Bounds.min.y + j * VoxelSize,
                                        pivotVoxelPoint.z + k * VoxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, backRotation);
                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                    voxelsData.Bounds.min.y + j * VoxelSize,
                                    pivotVoxelPoint.z + k * VoxelSize);

                                UpdateEntityPositionAndRotation(quadEntity, trans, backRotation);

                                countQuads++;
                            }
                            
                            if (kForward != voxelsData.Depth)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kForward, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                        voxelsData.Bounds.min.y + j * VoxelSize,
                                        pivotVoxelPoint.z + (k + 1) * VoxelSize);

                                    UpdateEntityPositionAndRotation(quadEntity, trans, forwardRotation);

                                    countQuads++;
                                }
                            }
                            else
                            {
                                var trans = new float3(voxelsData.Bounds.min.x + i * VoxelSize,
                                    voxelsData.Bounds.min.y + j * VoxelSize,
                                    pivotVoxelPoint.z + (k + 1) * VoxelSize);

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
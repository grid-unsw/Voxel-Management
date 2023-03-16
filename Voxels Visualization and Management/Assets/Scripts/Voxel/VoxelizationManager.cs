using UnityEngine;

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
       
    }
}
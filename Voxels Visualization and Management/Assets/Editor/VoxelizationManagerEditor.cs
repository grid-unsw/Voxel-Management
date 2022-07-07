using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelSystem;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(VoxelizationManager))]
public class VoxelizationManagerEditor : Editor
{
    private VoxelizationManager _voxelisationManager;
    private MeshFilter[] _meshFilters;
    private MultiValueVoxelModel _voxelColorModel;

    private void OnEnable()
    {
        _voxelisationManager = (VoxelizationManager) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DrawInspectorLayout();

        if (GUILayout.Button("Run"))
        {
            if (_meshFilters == null || _meshFilters.Length == 0)
            {
                _meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            }

            if (AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute", typeof(ComputeShader)) == null)
            {
                throw new FileLoadException("Voxelizer compute shader is not present");
            }

            _voxelisationManager.Voxelizer = (ComputeShader)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute", typeof(ComputeShader));


            if (_voxelisationManager.KeepObjectId)
            {
                if (_voxelColorModel == null)
                {
                    _voxelColorModel = _voxelisationManager.GetMultiValueVoxelData(_meshFilters);

                    Debug.Log("Voxels are successfully created!");
                }

                if (_voxelisationManager.VisualizeMesh)
                {
                    _voxelisationManager.BuildColorMesh(_voxelColorModel);
                }

                if (_voxelisationManager.VfxVisualisation)
                {
                    _voxelisationManager.VisualiseVfxColorVoxels(_voxelColorModel);
                }

                if (_voxelisationManager.ExportAsPointCloud)
                {
                    _voxelisationManager.ExportPts(_voxelColorModel);
                }

                if (_voxelisationManager.ExportToDatabase)
                {
                    _voxelisationManager.ExportToPostgres(_voxelColorModel);
                }

            }
            else
            {
                foreach (var voxelData in _voxelisationManager.GetVoxelData(_meshFilters))
                {
                    if (voxelData == null) continue;
                    
                    var voxels = voxelData.GetData();

                    if (_voxelisationManager.VisualizeMesh)
                    {
                        _voxelisationManager.BuildMesh(voxels, voxelData.Width, voxelData.Height,
                            voxelData.Depth);
                    }

                    if (_voxelisationManager.VfxVisualisation)
                    {
                        _voxelisationManager.VisualiseVfxVoxels(voxels, voxelData.Width, voxelData.Height,
                            voxelData.Depth, voxelData.PivotPoint);
                    }

                    if (_voxelisationManager.ExportAsPointCloud)
                    {
                        _voxelisationManager.ExportPts(voxels, voxelData.Width, voxelData.Height, voxelData.PivotPoint);
                    }

                    if (_voxelisationManager.ExportToDatabase)
                    {
                        _voxelisationManager.ExportToPostgres(voxels, voxelData.Width, voxelData.Height, voxelData.PivotPoint);
                    }

                    voxelData.Dispose();
                }
            }
        }
    }

    private void DrawInspectorLayout()
    {
        if (_voxelisationManager.VoxelizationGeom == VoxelizationGeomType.point)
        {
            _voxelisationManager.FilePathImport = EditorGUILayout.TextField("File Path Import", _voxelisationManager.FilePathImport);
        }

        _voxelisationManager.KeepObjectId = EditorGUILayout.Toggle("Preserve Objects Id", _voxelisationManager.KeepObjectId);

        if (!_voxelisationManager.KeepObjectId)
        {
            _voxelisationManager.VoxelColor = EditorGUILayout.ColorField("Voxel Color", _voxelisationManager.VoxelColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visualization options");
        _voxelisationManager.VisualizeMesh = EditorGUILayout.Toggle("Mesh", _voxelisationManager.VisualizeMesh);
        if (_voxelisationManager.VisualizeMesh)
        {
            _voxelisationManager.GridSplittingSize = EditorGUILayout.IntField("Grid Size", _voxelisationManager.GridSplittingSize);
            if (_voxelisationManager.KeepObjectId)
            {
                _voxelisationManager.MaxUsedColors =
                    EditorGUILayout.IntField("Max Unique Colors", _voxelisationManager.MaxUsedColors);
            }
        }

        EditorGUILayout.Space();
        _voxelisationManager.VfxVisualisation = EditorGUILayout.Toggle("VFX", _voxelisationManager.VfxVisualisation);
        if (_voxelisationManager.VfxVisualisation)
        {
            _voxelisationManager.VfxVisType = (VoxelVisualizationType) EditorGUILayout.EnumPopup("Geom Type", _voxelisationManager.VfxVisType);
        }

        EditorGUILayout.Space();
        _voxelisationManager.SvoVisualization = EditorGUILayout.Toggle("SVO", _voxelisationManager.SvoVisualization);
        if (_voxelisationManager.SvoVisualization)
        {

        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Export options");
        _voxelisationManager.GeomOffset = EditorGUILayout.Vector3Field("Geom Offset", _voxelisationManager.GeomOffset);
        EditorGUILayout.Space();
        _voxelisationManager.ExportAsPointCloud = EditorGUILayout.Toggle("Point Cloud", _voxelisationManager.ExportAsPointCloud);
        if (_voxelisationManager.ExportAsPointCloud)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Type", GUILayout.MaxWidth(57));
            _voxelisationManager.FileType = (PointCloudExportType)EditorGUILayout.EnumPopup("", _voxelisationManager.FileType, GUILayout.MaxWidth(50));
            EditorGUILayout.LabelField("Delimiter", GUILayout.MaxWidth(55));
            _voxelisationManager.Delimiter =
                (DelimiterType) EditorGUILayout.EnumPopup("", _voxelisationManager.Delimiter, GUILayout.MaxWidth(80));
            EditorGUILayout.EndHorizontal();
            _voxelisationManager.FilePathExport = EditorGUILayout.TextField("File Path Export", _voxelisationManager.FilePathExport);
        }

        EditorGUILayout.Space();
        _voxelisationManager.ExportToDatabase = EditorGUILayout.Toggle("Database", _voxelisationManager.ExportToDatabase);
        if (_voxelisationManager.ExportToDatabase)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table Name", GUILayout.MaxWidth(75));
            _voxelisationManager.TableName = EditorGUILayout.TextField("", _voxelisationManager.TableName, GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField("Truncate", GUILayout.MaxWidth(52));
            _voxelisationManager.Truncate = EditorGUILayout.Toggle("", _voxelisationManager.Truncate);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Geom Type", GUILayout.MaxWidth(75));
            _voxelisationManager.DBGeomExportType = (DatabaseExportType)EditorGUILayout.EnumPopup("", _voxelisationManager.DBGeomExportType, GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Storage options");
        _voxelisationManager.OctreeVisualisation = EditorGUILayout.Toggle("Octree", _voxelisationManager.OctreeVisualisation);
        _voxelisationManager.DotsVisualisation = EditorGUILayout.Toggle("DOTS Octree", _voxelisationManager.DotsVisualisation);

    }
}

using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelSystem;
using VoxelSystem.IO;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(VoxelizationManager))]
public class VoxelizationManagerEditor : Editor
{
    private VoxelizationManager _vManager;
    private MeshFilter[] _meshFilters;
    private MultiValueVoxelModel _voxelColorModel;

    private void OnEnable()
    {
        _vManager = (VoxelizationManager) target;
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

            _vManager.Voxelizer = (ComputeShader)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute", typeof(ComputeShader));

            var sceneObjectsOctree = new SceneObjectsOctree(_meshFilters);

            if (_vManager.KeepObjectId)
            {
                if (_voxelColorModel == null)
                {
                    if (_vManager.VisualizeMesh)
                    {
                        _voxelColorModel = VoxelFunctions.GetMultiValueVoxelData(_meshFilters, _vManager.Voxelizer,
                            _vManager.VoxelSize, _vManager.VoxelizationGeom, _vManager.MaxUsedColors);
                    }
                    else
                    {
                        _voxelColorModel = VoxelFunctions.GetMultiValueVoxelData(_meshFilters, _vManager.Voxelizer,
                            _vManager.VoxelSize, _vManager.VoxelizationGeom);
                    }

                    Debug.Log("Voxels are successfully created!");
                }

                if (_vManager.VisualizeMesh)
                {
                    VoxelMesh.BuildColorMesh(_voxelColorModel, _vManager.VoxelSize, _vManager.GridSplittingSize, _vManager.MaxUsedColors);
                }

                if (_vManager.VfxVisualisation)
                {
                    VfxFunctions.VisualiseVfxColorVoxels(_voxelColorModel, _vManager.VoxelSize, _vManager.VfxVisType);
                }

                if (_vManager.SvoVisualization)
                {
                    SvoFunctions.VisualizeColorVoxelModelWithSVO(_voxelColorModel.Voxels, _voxelColorModel.Width, _voxelColorModel.Height, _voxelColorModel.Bounds, _vManager.VoxelSize);
                }

                if (_vManager.ExportAsPointCloud)
                {
                    Output.ExportPts(_voxelColorModel,_vManager.VoxelSize, _vManager.FilePathExport, _vManager.Delimiter.GetDescription());
                }

                if (_vManager.ExportToDatabase)
                {
                    DBexport.ExportVoxels(_voxelColorModel, _voxelColorModel.Width, _voxelColorModel.Height, _voxelColorModel.Bounds.min + _vManager.GeomOffset, _vManager.VoxelSize, _vManager.TableName, _vManager.Truncate);
                }

            }
            else
            {
                foreach (var voxelData in VoxelFunctions.GetVoxelData(sceneObjectsOctree, _vManager.Voxelizer, _vManager.VoxelSize, _vManager.VoxelizationGeom))
                {
                    if (voxelData == null) continue;
                    
                    var voxels = voxelData.GetData();

                    if (_vManager.VisualizeMesh)
                    {
                        VoxelMesh.BuildMesh(voxels, voxelData.Width, voxelData.Height,
                            voxelData.Depth, _vManager.VoxelSize, _vManager.GridSplittingSize, _vManager.VoxelColor);
                    }

                    if (_vManager.VfxVisualisation)
                    {
                        VfxFunctions.VisualizeVoxels_t(voxels, voxelData.Width, voxelData.Height,
                            voxelData.Depth, voxelData.Bounds.min, _vManager.VoxelSize, _vManager.VoxelColor, _vManager.VfxVisType);
                    }

                    if (_vManager.SvoVisualization)
                    {
                        SvoFunctions.VisualizeBinaryVoxelModelWithSVO(VoxelFunctions.GetBinaryArray(voxels),voxelData.Width, voxelData.Height, voxelData.Bounds, _vManager.VoxelSize, _vManager.VoxelColor);
                    }

                    if (_vManager.ExportAsPointCloud)
                    {
                        Output.ExportPts(voxels, voxelData.Width, voxelData.Height, voxelData.Bounds.min, _vManager.VoxelSize, _vManager.FilePathExport, _vManager.Delimiter.GetDescription());
                    }

                    if (_vManager.ExportToDatabase)
                    {
                        DBexport.ExportVoxels(voxels, voxelData.Width, voxelData.Height, voxelData.Bounds.min+_vManager.GeomOffset, _vManager.VoxelSize, _vManager.TableName, _vManager.Truncate);
                    }

                    voxelData.Dispose();
                }
            }
        }
    }

    private void DrawInspectorLayout()
    {
        if (_vManager.VoxelizationGeom == VoxelizationGeomType.point)
        {
            _vManager.FilePathImport = EditorGUILayout.TextField("File Path Import", _vManager.FilePathImport);
        }

        _vManager.KeepObjectId = EditorGUILayout.Toggle("Preserve Objects Id", _vManager.KeepObjectId);

        if (!_vManager.KeepObjectId)
        {
            _vManager.VoxelColor = EditorGUILayout.ColorField("Voxel Color", _vManager.VoxelColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visualization options");
        _vManager.VisualizeMesh = EditorGUILayout.Toggle("Mesh", _vManager.VisualizeMesh);
        if (_vManager.VisualizeMesh)
        {
            _vManager.GridSplittingSize = EditorGUILayout.IntField("Grid Size", _vManager.GridSplittingSize);
            if (_vManager.KeepObjectId)
            {
                _vManager.MaxUsedColors =
                    EditorGUILayout.IntField("Max Unique Colors", _vManager.MaxUsedColors);
            }
        }

        EditorGUILayout.Space();
        _vManager.VfxVisualisation = EditorGUILayout.Toggle("VFX", _vManager.VfxVisualisation);
        if (_vManager.VfxVisualisation)
        {
            _vManager.VfxVisType = (VoxelVisualizationType) EditorGUILayout.EnumPopup("Geom Type", _vManager.VfxVisType);
        }

        EditorGUILayout.Space();
        _vManager.SvoVisualization = EditorGUILayout.Toggle("SVO", _vManager.SvoVisualization);
        if (_vManager.SvoVisualization)
        {

        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Export options");
        _vManager.GeomOffset = EditorGUILayout.Vector3Field("Geom Offset", _vManager.GeomOffset);
        EditorGUILayout.Space();
        _vManager.ExportAsPointCloud = EditorGUILayout.Toggle("Point Cloud", _vManager.ExportAsPointCloud);
        if (_vManager.ExportAsPointCloud)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Type", GUILayout.MaxWidth(57));
            _vManager.FileType = (PointCloudExportType)EditorGUILayout.EnumPopup("", _vManager.FileType, GUILayout.MaxWidth(50));
            EditorGUILayout.LabelField("Delimiter", GUILayout.MaxWidth(55));
            _vManager.Delimiter =
                (DelimiterType) EditorGUILayout.EnumPopup("", _vManager.Delimiter, GUILayout.MaxWidth(80));
            EditorGUILayout.EndHorizontal();
            _vManager.FilePathExport = EditorGUILayout.TextField("File Path Export", _vManager.FilePathExport);
        }

        EditorGUILayout.Space();
        _vManager.ExportToDatabase = EditorGUILayout.Toggle("Database", _vManager.ExportToDatabase);
        if (_vManager.ExportToDatabase)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table Name", GUILayout.MaxWidth(75));
            _vManager.TableName = EditorGUILayout.TextField("", _vManager.TableName, GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField("Truncate", GUILayout.MaxWidth(52));
            _vManager.Truncate = EditorGUILayout.Toggle("", _vManager.Truncate);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Geom Type", GUILayout.MaxWidth(75));
            _vManager.DBGeomExportType = (DatabaseExportType)EditorGUILayout.EnumPopup("", _vManager.DBGeomExportType, GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Storage options");
        _vManager.OctreeVisualisation = EditorGUILayout.Toggle("Octree", _vManager.OctreeVisualisation);
        _vManager.DotsVisualisation = EditorGUILayout.Toggle("DOTS Octree", _vManager.DotsVisualisation);

    }
}

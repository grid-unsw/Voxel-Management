using UnityEditor;
using UnityEngine;
using VoxelSystem;


[CustomEditor(typeof(VoxelisationManager))]
public class VoxelisationManagerEditor : Editor
{
    VoxelisationManager _voxelisationManager;
    private MeshFilter[] _meshFilters;
    private GPUVoxelData _voxelsData;

    private void OnEnable()
    {
        _voxelisationManager = (VoxelisationManager) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Build Voxels"))
        {
            if (_meshFilters == null || _meshFilters.Length == 0)
                _meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

            _voxelsData = _voxelisationManager.GetVoxelData(_meshFilters);

            Debug.Log("Voxels are successfully created!");
        }

        if (GUILayout.Button("Visualise voxels"))
        {
            if (_voxelsData == null)
            {
                _voxelsData = _voxelisationManager.GetVoxelData(_meshFilters);
            }

            if (_voxelisationManager.voxelMesh)
            {
                _voxelisationManager.VisualiseVoxelMesh(_voxelsData);
            }

            if (_voxelisationManager.vfxVisualisation)
            {
                _voxelisationManager.VisualiseVfxVoxels(_voxelsData);
            }
        }

        if (GUILayout.Button("Export Voxels"))
        {
            if (_voxelsData == null)
            {
                Debug.Log("Voxels are not created!");
            }

            _voxelisationManager.ExportPts(_voxelsData);

            Debug.Log("Voxels successfully exported!");
        }
    }
}

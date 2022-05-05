using UnityEditor;
using UnityEngine;
using VoxelSystem;


[CustomEditor(typeof(VoxelisationManager))]
public class VoxelisationManagerEditor : Editor
{
    VoxelisationManager _voxelisationManager;
    private MeshFilter[] _meshFilters;
    private GPUVoxelData _voxelModel;
    private MultiValueVoxelModel _voxelColorModel;

    private void OnEnable()
    {
        _voxelisationManager = (VoxelisationManager) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Build and visualise voxels"))
        {
            if (_meshFilters == null || _meshFilters.Length == 0)
                _meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];

            if (_voxelisationManager.hasColor)
            {
                if (_voxelColorModel == null)
                {
                    _voxelColorModel = _voxelisationManager.GetMultiValueVoxelData(_meshFilters);

                    Debug.Log("Voxels are successfully created!");
                }

                if (_voxelisationManager.voxelMesh)
                {
                    _voxelisationManager.BuildColorMesh(_voxelColorModel);
                }

                if (_voxelisationManager.vfxVisualisation)
                {
                    _voxelisationManager.VisualiseVfxColorVoxels(_voxelColorModel);
                }
            }
            else
            {
                if (_voxelModel == null)
                {
                    _voxelModel = _voxelisationManager.GetVoxelData(_meshFilters);

                    Debug.Log("Voxels are successfully created!");
                }

                if (_voxelisationManager.voxelMesh)
                {
                    _voxelisationManager.BuildMesh(_voxelModel);
                }

                if (_voxelisationManager.vfxVisualisation)
                {
                    _voxelisationManager.VisualiseVfxVoxels(_voxelModel);
                }
            }
        }

        if (GUILayout.Button("Export Voxels"))
        {
            if (_voxelisationManager.hasColor)
            {
                if (_voxelColorModel == null)
                {
                    Debug.Log("Voxels are not created!");
                }
                else
                {
                    _voxelisationManager.ExportPts(_voxelColorModel);
                }

            }
            else
            {
                if (_voxelModel == null)
                {
                    Debug.Log("Voxels are not created!");
                }
                else
                {
                    _voxelisationManager.ExportPts(_voxelModel);
                }
            }
        }
    }
}

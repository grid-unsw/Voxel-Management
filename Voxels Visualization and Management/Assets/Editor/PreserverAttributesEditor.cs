using System.IO;
using UnityEditor;
using UnityEngine;
using VoxelSystem;

[CustomEditor(typeof(PreserveAttributes))]
public class PreserverAttributesEditor : Editor
{
    private PreserveAttributes _manager;
    private MeshFilter[] _meshFilters;
    private MultiValueVoxelModel _voxelColorModel;

    private void OnEnable()
    {
        _manager = (PreserveAttributes) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Run"))
        {
            if (_meshFilters == null || _meshFilters.Length == 0)
            {
                _meshFilters = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            }

            if (AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute", typeof(ComputeShader)) ==
                null)
            {
                throw new FileLoadException("Voxelizer compute shader is not present");
            }

            _manager.Voxelizer = (ComputeShader) AssetDatabase.LoadAssetAtPath("Assets/Scripts/Shaders/Voxelizer.compute",
                    typeof(ComputeShader));

            if (_voxelColorModel == null)
            {
                _voxelColorModel = VoxelFunctions.GetMultiValueVoxelData(_meshFilters, _manager.Voxelizer,
                        _manager.VoxelSize, VoxelizationGeomType.surface);
                

                Debug.Log("Voxels are successfully created!");
            }

            GetFilledVoxels(_voxelColorModel);

            if (_manager.VisualizeVoxels)
            {
                VfxFunctions.VisualiseVfxColorVoxels(_voxelColorModel, _manager.VoxelSize, VoxelVisualizationType.quad);
            }
        }
    }

    private void GetFilledVoxels(MultiValueVoxelModel voxelModel)
    {
        for (int i = 0; i < voxelModel.Voxels.Length; i++)
        {
            var voxel = voxelModel.Voxels[i];
            if (voxel != null)
            {
                var index3D = ArrayFunctions.Index1DTo3D(i, voxelModel.Width, voxelModel.Height);

            }
        }
    }

}
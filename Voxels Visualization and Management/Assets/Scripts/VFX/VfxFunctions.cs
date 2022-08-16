using UnityEngine;
using VoxelSystem;

public static class VfxFunctions 
{
    public static void VisualizeBooleanVoxels(bool[] voxels, int width, int height, int depth, Vector3 pivotPoint, float voxelSize, Color voxelColor, VoxelVisualizationType vfxVisType)
    {
        if (vfxVisType == VoxelVisualizationType.cube)
        {

            Debug.Log("Not implemented VFX voxel visualization");
            //    var voxelLayerPrefab = Resources.Load("Prefabs/VoxelLayer") as GameObject;

            //    var voxelsLayerGameObject = Object.Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
            //    voxelsLayerGameObject.name = "VFX_cubes_" + voxelSize;
            //    voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>().SetVoxelParticles(voxels, width, height, pivotPoint, voxelSize, voxelColor);
        }
        else
        {
            var voxelLayerPrefab = Resources.Load("Prefabs/QuadLayer") as GameObject;
            var voxelLayer = Object.Instantiate(voxelLayerPrefab);
            voxelLayer.name = "VFX_quads_" + voxelSize;
            voxelLayer.GetComponent<VoxelsRendererDynamic>()
                .SetQuadParticles(voxels, width, height, depth, pivotPoint, voxelSize, voxelColor);
        }
    }

    public static void VisualizeVoxels_t(Voxel_t[] voxels, int width, int height, int depth, Vector3 pivotPoint, float voxelSize, Color voxelColor, VoxelVisualizationType vfxVisType)
    {
        if (vfxVisType == VoxelVisualizationType.cube)
        {
            var voxelLayerPrefab = Resources.Load("Prefabs/VoxelLayer") as GameObject;

            var voxelsLayerGameObject = Object.Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
            voxelsLayerGameObject.name = "VFX_cubes_" + voxelSize;
            voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>().SetVoxelParticles(voxels, width, height, pivotPoint, voxelSize, voxelColor);
        }
        else
        {
            var voxelLayerPrefab = Resources.Load("Prefabs/QuadLayer") as GameObject;
            var voxelLayer = Object.Instantiate(voxelLayerPrefab);
            voxelLayer.name = "VFX_quads_" + voxelSize;
            voxelLayer.GetComponent<VoxelsRendererDynamic>()
                .SetQuadParticles(voxels, width, height, depth, pivotPoint, voxelSize, voxelColor);
        }
    }


    public static void VisualiseVfxColorVoxels(MultiValueVoxelModel voxelModel, float voxelSize, VoxelVisualizationType vfxVisType)
    {
        if (vfxVisType == VoxelVisualizationType.quad)
        {
            var voxelLayerPrefab = Resources.Load("Prefabs/QuadWithColorLayer") as GameObject;

            var voxelsLayerGameObject = Object.Instantiate(voxelLayerPrefab, Vector3.zero, new Quaternion());
            voxelsLayerGameObject.name = "VFX_color_quads_" + voxelSize;
            voxelsLayerGameObject.GetComponent<VoxelsRendererDynamic>()
                .SetColorQuadParticles(voxelModel, voxelSize);
        }
        else
        {
            Debug.Log("Not implemented VFX voxel visualization");
        }
    }
}

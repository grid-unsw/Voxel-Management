using UnityEngine;

public class PreserveAttributes : MonoBehaviour
{
    [Header("Voxelisation")]
    [HideInInspector] public ComputeShader Voxelizer;
    [SerializeField] public float VoxelSize = 0.25f;
    public bool VisualizeVoxels;
}

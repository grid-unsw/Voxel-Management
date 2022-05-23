using UnityEngine;

public class MeshSimplification : MonoBehaviour
{
    void Start()
    {
        Simplify();
    }

    private void Simplify()
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            SimplifyMeshFilter(meshFilter);
        }
    }

    private void SimplifyMeshFilter(MeshFilter meshFilter)
    {
        Mesh sourceMesh = meshFilter.sharedMesh;
        if (sourceMesh == null) // verify that the mesh filter actually has a mesh
            return;

        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        /*
        // Create our mesh simplifier and setup our entire mesh in it
        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        var meshSimplifierSimplificationOptions = meshSimplifier.SimplificationOptions;
        meshSimplifierSimplificationOptions.PreserveBorderEdges = true;
        meshSimplifierSimplificationOptions.PreserveUVSeamEdges = true;
        meshSimplifierSimplificationOptions.PreserveSurfaceCurvature = true;
        meshSimplifierSimplificationOptions.PreserveUVFoldoverEdges = true;
        meshSimplifierSimplificationOptions.VertexLinkDistance = 5f;               
        */

        meshSimplifier.SimplifyMeshLossless();

        // Create our final mesh and apply it back to our mesh filter
        meshFilter.sharedMesh = meshSimplifier.ToMesh();
    }
}

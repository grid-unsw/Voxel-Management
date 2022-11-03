using UnityEngine;

public class CreatePyramid : MonoBehaviour
{
    public void Create()
    {
        var vertices = new Vector3[5]
        {
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(0.5f, 1f, 0.5f)
        };

        var triangles = new int[18] {1,0,4,4,0,2,3,1,4,4,2,3,0,1,3,0,3,2 };

        var mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };

        var gameObject = new GameObject("Pyramid", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

}

using UnityEngine;

namespace MeshManagement
{
    public static class MeshFunctions
    {
        public const int Mesh16BitBufferVertexLimit = 65535;

        /// <summary>
        /// Create one mesh from many meshes
        /// </summary>
        public static Mesh CombineMeshes(MeshFilter[] meshFilters)
        {
            // First MeshFilter belongs to this GameObject so we don't need it:
            var combineInstances = new CombineInstance[meshFilters.Length];

            var verticesLength = 0;
            var i = 0;
            while (i < meshFilters.Length) // Skip first MeshFilter belongs to this GameObject in this loop.
            {
                combineInstances[i].subMeshIndex = 0;
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
                verticesLength += combineInstances[i].mesh.vertices.Length;
                i++;
            }

            // Create Mesh from combineInstances:
            var combinedMesh = new Mesh();

            // If it will be over 65535 then use the 32 bit index buffer:
            if (verticesLength > Mesh16BitBufferVertexLimit)
            {
                combinedMesh.indexFormat =
                    UnityEngine.Rendering.IndexFormat.UInt32; // Only works on Unity 2017.3 or higher.
            }

            combinedMesh.CombineMeshes(combineInstances);

            return combinedMesh;
        }

        public static Mesh TransformMeshLocalToWorld(MeshFilter meshFilter)
        {
            var newVertices = new Vector3[meshFilter.sharedMesh.vertexCount];

            var transform = meshFilter.transform;
            var vertices = meshFilter.sharedMesh.vertices;

            for (var i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                var vertex = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i]);
                newVertices[i] = vertex;
            }

            var newMesh = new Mesh()
            {
                vertices = newVertices,
                triangles = meshFilter.sharedMesh.triangles
            };

            return newMesh;
        }

        public static Bounds GetMeshesBoundsInGlobalSpace(MeshFilter[] meshFilters)
        {
            var min = Vector3.positiveInfinity;
            var max = Vector3.negativeInfinity;

            foreach (var meshFilter in meshFilters)
            {
                var bounds = meshFilter.gameObject.GetComponent<Renderer>().bounds;

                // update min and max
                min = Vector3.Min(min, bounds.min);
                max = Vector3.Max(max, bounds.max);
            }

            return new Bounds((min+max)/2,max-min);
        }
    }
}


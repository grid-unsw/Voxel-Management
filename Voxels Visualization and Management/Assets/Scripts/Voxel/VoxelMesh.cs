using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelSystem
{
    public class VoxelMesh {

		public static Mesh Build(GPUVoxelData voxelsData, float unit, bool useUV = false) {
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();
			var normals = new List<Vector3>();
			var centers = new List<Vector4>();

			var up = Vector3.up * unit;
			var hup = up * 0.5f;
			var hbottom = -hup;

			var right = Vector3.right * unit;
			var hright = right * 0.5f;

			var left = -right;
			var hleft = left * 0.5f;

			var forward = Vector3.forward * unit;
			var hforward = forward * 0.5f;
			var back = -forward;
			var hback = back * 0.5f;

            var voxels = voxelsData.GetData();
            var m = 0;
            for (int k = 0; k < voxelsData.Depth; k++)
                for (int j = 0; j < voxelsData.Height; j++)
                    for (int i = 0; i < voxelsData.Width; i++)
                    {
                        var v = voxels[m];
                        if (v.fill > 0)
                        {
                            var iLeft = i - 1;
                            var iRight = i + 1;
                            var jDown = j - 1;
                            var jUp = j + 1;
                            var kBack = k - 1;
                            var kForward = k + 1;

                            // left
                            if (iLeft != -1)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(iLeft, j, k, voxelsData.Width, voxelsData.Height);
                                var voxelLeft = voxels[voxelIndex];

                                if (voxelLeft.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hleft, back, up, Vector3.left);
                                }
                            }
                            else
                            {
								CalculatePlane(
	                                    vertices, normals, centers, uvs, triangles,
                                        v, useUV, hleft, back, up, Vector3.left);
                            }

							// right
                            if (iRight != voxelsData.Width)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(iRight, j, k, voxelsData.Width,
                                    voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hright, forward, up, Vector3.right);
                                }
                            }
                            else
                            {
                                CalculatePlane(
                                    vertices, normals, centers, uvs, triangles,
                                    v, useUV, hright, forward, up, Vector3.right);
                            }

                            // down
                            if (jDown != -1)
                            {
                                var voxelIndex =
                                    ArrayFunctions.Index3DTo1D(i, jDown, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hbottom, right, back, Vector3.down);
                                }
                            }
                            else
                            {
                                CalculatePlane(
                                    vertices, normals, centers, uvs, triangles,
                                    v, useUV, hbottom, right, back, Vector3.down);
                            }

                            // up
                            if (jUp != voxelsData.Height)
                            {
                                var voxelIndex =
                                    ArrayFunctions.Index3DTo1D(i, jUp, k, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hup, right, forward, Vector3.up);
                                }
                            }
                            else
                            {
                                CalculatePlane(
                                    vertices, normals, centers, uvs, triangles,
                                    v, useUV, hup, right, forward, Vector3.up);
                            }

                            // back
                            if (kBack != -1)
                            {
                                var voxelIndex =
                                    ArrayFunctions.Index3DTo1D(i, j, kBack, voxelsData.Width, voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hback, right, up, Vector3.back);
                                }
                            }
                            else
                            {
                                CalculatePlane(
                                    vertices, normals, centers, uvs, triangles,
                                    v, useUV, hback, right, up, Vector3.back);
                            }

                            // forward
                            if (kForward != voxelsData.Depth)
                            {
                                var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kForward, voxelsData.Width,
                                    voxelsData.Height);
                                var voxel = voxels[voxelIndex];

                                if (voxel.fill == 0)
                                {
                                    CalculatePlane(
                                        vertices, normals, centers, uvs, triangles,
                                        v, useUV, hforward, left, up, Vector3.forward);
                                }
                            }
                            else
                            {
                                CalculatePlane(
                                    vertices, normals, centers, uvs, triangles,
                                    v, useUV, hforward, left, up, Vector3.forward);
                            }
                        }
                        m++;
                    }

			var mesh = new Mesh();
			mesh.indexFormat = IndexFormat.UInt32;
			mesh.vertices = vertices.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.normals = normals.ToArray();
			mesh.tangents = centers.ToArray();
			mesh.SetTriangles(triangles.ToArray(), 0);
			mesh.RecalculateBounds();
			return mesh;
		}

        public static IEnumerable<VoxelChunk> Build(GPUVoxelData voxelsData, float unit, int gridSplittigSize, bool useUV = false)
        {
            var chunksWidthCount = Mathf.CeilToInt((float)voxelsData.Width / gridSplittigSize);
            var chunksHeightCount = Mathf.CeilToInt((float)voxelsData.Height / gridSplittigSize);
            var chunksDepthCount = Mathf.CeilToInt((float)voxelsData.Depth / gridSplittigSize);

            var voxels = voxelsData.GetData();

            for (var d = 0; d < chunksDepthCount; d++)
                for (var h = 0; h < chunksHeightCount; h++)
                    for (var w = 0; w < chunksWidthCount; w++)
                    {
                        var vertices = new List<Vector3>();
                        var uvs = new List<Vector2>();
                        var triangles = new List<int>();
                        var normals = new List<Vector3>();
                        var centers = new List<Vector4>();

                        var up = Vector3.up * unit;
                        var hup = up * 0.5f;
                        var hbottom = -hup;

                        var right = Vector3.right * unit;
                        var hright = right * 0.5f;

                        var left = -right;
                        var hleft = left * 0.5f;

                        var forward = Vector3.forward * unit;
                        var hforward = forward * 0.5f;
                        var back = -forward;
                        var hback = back * 0.5f;

                        var minDepthIndex = d * gridSplittigSize;
                        var minHeightIndex = h * gridSplittigSize;
                        var minWidthIndex = w * gridSplittigSize;

                        for (int k = minDepthIndex; k < minDepthIndex + gridSplittigSize && k < voxelsData.Depth; k++)
                            for (int j = minHeightIndex; j < minHeightIndex + gridSplittigSize && j < voxelsData.Height; j++)
                                for (int i = minWidthIndex; i < minWidthIndex + gridSplittigSize && i < voxelsData.Width; i++)
                                {
                                    var vIndex =
                                        ArrayFunctions.Index3DTo1D(i, j, k, voxelsData.Width, voxelsData.Height);
                                    var v = voxels[vIndex];
                                    if (v.fill > 0)
                                    {
                                        var iLeft = i - 1;
                                        var iRight = i + 1;
                                        var jDown = j - 1;
                                        var jUp = j + 1;
                                        var kBack = k - 1;
                                        var kForward = k + 1;

                                        // left
                                        if (iLeft != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(iLeft, j, k, voxelsData.Width, voxelsData.Height);
                                            var voxelLeft = voxels[voxelIndex];

                                            if (voxelLeft.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hleft, back, up, Vector3.left);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hleft, back, up, Vector3.left);
                                        }

                                        // right
                                        if (iRight != voxelsData.Width)
                                        {
                                            var voxelIndex = ArrayFunctions.Index3DTo1D(iRight, j, k, voxelsData.Width,
                                                voxelsData.Height);
                                            var voxel = voxels[voxelIndex];

                                            if (voxel.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hright, forward, up, Vector3.right);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hright, forward, up, Vector3.right);
                                        }

                                        // down
                                        if (jDown != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, jDown, k, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxels[voxelIndex];

                                            if (voxel.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hbottom, right, back, Vector3.down);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hbottom, right, back, Vector3.down);
                                        }

                                        // up
                                        if (jUp != voxelsData.Height)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, jUp, k, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxels[voxelIndex];

                                            if (voxel.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hup, right, forward, Vector3.up);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hup, right, forward, Vector3.up);
                                        }

                                        // back
                                        if (kBack != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, j, kBack, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxels[voxelIndex];

                                            if (voxel.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hback, right, up, Vector3.back);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hback, right, up, Vector3.back);
                                        }

                                        // forward
                                        if (kForward != voxelsData.Depth)
                                        {
                                            var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kForward, voxelsData.Width,
                                                voxelsData.Height);
                                            var voxel = voxels[voxelIndex];

                                            if (voxel.fill == 0)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles,
                                                    v, useUV, hforward, left, up, Vector3.forward);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles,
                                                v, useUV, hforward, left, up, Vector3.forward);
                                        }
                                    }
                                }

                        if (vertices.Any())
                        {
                            var mesh = new Mesh();
                            mesh.indexFormat = IndexFormat.UInt32;
                            mesh.vertices = vertices.ToArray();
                            mesh.uv = uvs.ToArray();
                            mesh.normals = normals.ToArray();
                            mesh.tangents = centers.ToArray();
                            mesh.SetTriangles(triangles.ToArray(), 0);
                            mesh.RecalculateBounds();

                            yield return new VoxelChunk(minWidthIndex,minHeightIndex, minDepthIndex, mesh);
                        }
                        yield return null;
                    }
        }

        public static IEnumerable<VoxelColorChunk> BuildWithColor(MultiValueVoxelModel voxelsData, float unit, int gridSplittigSize, int materialsNumber, bool useUV = false)
        {
            var chunksWidthCount = Mathf.CeilToInt((float)voxelsData.Width / gridSplittigSize);
            var chunksHeightCount = Mathf.CeilToInt((float)voxelsData.Height / gridSplittigSize);
            var chunksDepthCount = Mathf.CeilToInt((float)voxelsData.Depth / gridSplittigSize);

            var halfVoxelSize = voxelsData.VoxelSize / 2;
            var firstVoxelCentroid = new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize) + voxelsData.PivotPoint;

            var uniqueMaterials = VisualizationFunctions.GetUniqueMaterials(materialsNumber, "Universal Render Pipeline/Simple Lit");

            for (var d = 0; d < chunksDepthCount; d++)
                for (var h = 0; h < chunksHeightCount; h++)
                    for (var w = 0; w < chunksWidthCount; w++)
                    {
                        var vertices = new List<Vector3>();
                        var uvs = new List<Vector2>();
                        var triangles = new List<List<int>>();
                        var normals = new List<Vector3>();
                        var centers = new List<Vector4>();

                        var up = Vector3.up * unit;
                        var hup = up * 0.5f;
                        var hbottom = -hup;

                        var right = Vector3.right * unit;
                        var hright = right * 0.5f;

                        var left = -right;
                        var hleft = left * 0.5f;

                        var forward = Vector3.forward * unit;
                        var hforward = forward * 0.5f;
                        var back = -forward;
                        var hback = back * 0.5f;

                        var subMeshMaterials = new Dictionary<int, SubmeshMaterial>();

                        var minDepthIndex = d * gridSplittigSize;
                        var minHeightIndex = h * gridSplittigSize;
                        var minWidthIndex = w * gridSplittigSize;
                        var triangleSubMesh = 0;
                        var currentSubMesh = 0;

                        for (int k = minDepthIndex; k < minDepthIndex + gridSplittigSize && k < voxelsData.Depth; k++)
                            for (int j = minHeightIndex; j < minHeightIndex + gridSplittigSize && j < voxelsData.Height; j++)
                                for (int i = minWidthIndex; i < minWidthIndex + gridSplittigSize && i < voxelsData.Width; i++)
                                {
                                    var vIndex =
                                        ArrayFunctions.Index3DTo1D(i, j, k, voxelsData.Width, voxelsData.Height);
                                    var v = voxelsData.Voxels[vIndex];
                                    
                                    if (v != null)
                                    {
                                        //consider only the first object
                                        var firstObject = v[0];
                                        if (!subMeshMaterials.ContainsKey(firstObject.Id))
                                        {
                                            var material = uniqueMaterials[currentSubMesh];
                                            var subMeshMaterial = new SubmeshMaterial(currentSubMesh, material);
                                            subMeshMaterials.Add(firstObject.Id, subMeshMaterial);
                                            triangleSubMesh = currentSubMesh;
                                            triangles.Add(new List<int>());
                                            currentSubMesh++;
                                            if (currentSubMesh == materialsNumber)
                                            {
                                                currentSubMesh = 0;
                                            }
                                        }
                                        else
                                        {
                                            triangleSubMesh = subMeshMaterials[firstObject.Id].Id;
                                        }

                                        var voxelCentroid = new Vector3(i * voxelsData.VoxelSize,
                                                                j * voxelsData.VoxelSize,
                                                                k * voxelsData.VoxelSize) +
                                                            firstVoxelCentroid;

                                        var iLeft = i - 1;
                                        var iRight = i + 1;
                                        var jDown = j - 1;
                                        var jUp = j + 1;
                                        var kBack = k - 1;
                                        var kForward = k + 1;

                                        // left
                                        if (iLeft != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(iLeft, j, k, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hleft, back, up, Vector3.left);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hleft, back, up, Vector3.left);
                                        }
                                        
                                        // right
                                        if (iRight != voxelsData.Width)
                                        {
                                            var voxelIndex = ArrayFunctions.Index3DTo1D(iRight, j, k, voxelsData.Width,
                                                voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hright, forward, up, Vector3.right);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hright, forward, up, Vector3.right);
                                        }

                                        // down
                                        if (jDown != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, jDown, k, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hbottom, right, back, Vector3.down);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hbottom, right, back, Vector3.down);
                                        }

                                        // up
                                        if (jUp != voxelsData.Height)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, jUp, k, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hup, right, forward, Vector3.up);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hup, right, forward, Vector3.up);
                                        }

                                        // back
                                        if (kBack != -1)
                                        {
                                            var voxelIndex =
                                                ArrayFunctions.Index3DTo1D(i, j, kBack, voxelsData.Width, voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hback, right, up, Vector3.back);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hback, right, up, Vector3.back);
                                        }

                                        // forward
                                        if (kForward != voxelsData.Depth)
                                        {
                                            var voxelIndex = ArrayFunctions.Index3DTo1D(i, j, kForward, voxelsData.Width,
                                                voxelsData.Height);
                                            var voxel = voxelsData.Voxels[voxelIndex];

                                            if (voxel == null)
                                            {
                                                CalculatePlane(
                                                    vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                    voxelCentroid, useUV, hforward, left, up, Vector3.forward);
                                            }
                                        }
                                        else
                                        {
                                            CalculatePlane(
                                                vertices, normals, centers, uvs, triangles, triangleSubMesh,
                                                voxelCentroid, useUV, hforward, left, up, Vector3.forward);
                                        }
                                    }
                                }

                        if (vertices.Any())
                        {
                            var mesh = new Mesh();
                            mesh.indexFormat = IndexFormat.UInt32;
                            mesh.vertices = vertices.ToArray();
                            mesh.uv = uvs.ToArray();
                            mesh.normals = normals.ToArray();
                            mesh.tangents = centers.ToArray();

                            mesh.subMeshCount = subMeshMaterials.Keys.Max();
                            var i = 0;
                            foreach (var subMeshTriangles in triangles)
                            {
                                mesh.SetTriangles(subMeshTriangles.ToArray(), i);
                                i++;
                            }
                            mesh.RecalculateBounds();
                            var materials = subMeshMaterials.Values.Select(x => x.Mat).ToArray();
                            yield return new VoxelColorChunk(minWidthIndex, minHeightIndex, minDepthIndex, mesh, materials);
                        }
                        yield return null;
                    }
        }

        static void CalculatePlane (
			List<Vector3> vertices, List<Vector3> normals, List<Vector4> centers, List<Vector2> uvs, List<int> triangles,
			Voxel_t voxel, bool useUV, Vector3 offset, Vector3 right, Vector3 up, Vector3 normal, int rSegments = 2, int uSegments = 2
		) {
			float rInv = 1f / (rSegments - 1);
			float uInv = 1f / (uSegments - 1);

			int triangleOffset = vertices.Count;
            var center = voxel.position;

            var transformed = center + offset;
			for(int y = 0; y < uSegments; y++) {
				float ru = y * uInv;
				for(int x = 0; x < rSegments; x++) {
					float rr = x * rInv;
					vertices.Add(transformed + right * (rr - 0.5f) + up * (ru - 0.5f));
					normals.Add(normal);
					centers.Add(center);
                    if(useUV)
                    {
					    uvs.Add(voxel.uv);
                    } else
                    {
					    uvs.Add(new Vector2(rr, ru));
                    }
				}

				if(y < uSegments - 1) {
					var ioffset = y * rSegments + triangleOffset;
					for(int x = 0, n = rSegments - 1; x < n; x++) {
						triangles.Add(ioffset + x);
						triangles.Add(ioffset + x + rSegments);
						triangles.Add(ioffset + x + 1);

						triangles.Add(ioffset + x + 1);
						triangles.Add(ioffset + x + rSegments);
						triangles.Add(ioffset + x + 1 + rSegments);
					}
				}
			}
		}

        static void CalculatePlane(
            List<Vector3> vertices, List<Vector3> normals, List<Vector4> centers, List<Vector2> uvs, List<List<int>> triangles, 
            int trianglesSubMesh, Vector3 center, bool useUV, Vector3 offset, Vector3 right, Vector3 up, Vector3 normal, int rSegments = 2, int uSegments = 2
        )
        {
            float rInv = 1f / (rSegments - 1);
            float uInv = 1f / (uSegments - 1);

            int triangleOffset = vertices.Count;

            var subMeshTriangles = triangles[trianglesSubMesh];
            var transformed = center + offset;
            for (int y = 0; y < uSegments; y++)
            {
                float ru = y * uInv;
                for (int x = 0; x < rSegments; x++)
                {
                    float rr = x * rInv;
                    vertices.Add(transformed + right * (rr - 0.5f) + up * (ru - 0.5f));
                    normals.Add(normal);
                    centers.Add(center);
                    uvs.Add(new Vector2(rr, ru));
                }

                if (y < uSegments - 1)
                {
                    var ioffset = y * rSegments + triangleOffset;
                    for (int x = 0, n = rSegments - 1; x < n; x++)
                    {
                        subMeshTriangles.Add(ioffset + x);
                        subMeshTriangles.Add(ioffset + x + rSegments);
                        subMeshTriangles.Add(ioffset + x + 1);

                        subMeshTriangles.Add(ioffset + x + 1);
                        subMeshTriangles.Add(ioffset + x + rSegments);
                        subMeshTriangles.Add(ioffset + x + 1 + rSegments);
                    }
                }
            }
        }
    }

}



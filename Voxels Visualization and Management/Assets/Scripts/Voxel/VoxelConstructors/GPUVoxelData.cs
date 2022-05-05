using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace VoxelSystem {

	public class GPUVoxelData : System.IDisposable {

		public ComputeBuffer Buffer { get { return buffer; } }
        public int Width { get { return width; } }
		public int Height { get { return height; } }
		public int Depth { get { return depth; } }
		public Vector3 PivotPoint { get { return pivotPoint; } }

        int width, height, depth;
		Vector3 pivotPoint;

		ComputeBuffer buffer;
        Voxel_t[] voxels;

		public GPUVoxelData(ComputeBuffer buf, int w, int h, int d, Vector3 p) {
			buffer = buf;
			width = w;
			height = h;
			depth = d;
            pivotPoint = p;
        }

		public Voxel_t[] GetData() {
            // cache
            if(voxels == null) {
			    voxels = new Voxel_t[Buffer.count];
			    Buffer.GetData(voxels);
            }
			return voxels;
		}

		public void Dispose() {
			buffer.Release();
		}

	}

}


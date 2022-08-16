using UnityEngine;

namespace VoxelSystem {

	public class GPUVoxelData : System.IDisposable {

        public ComputeBuffer Buffer { get; }
        public int Width { get; }
		public int Height { get; }
		public int Depth { get; }
		public Bounds Bounds { get; }

        private Voxel_t[] _voxels;

		public GPUVoxelData(ComputeBuffer buffer, int width, int height, int depth, Bounds bounds) {
			Buffer = buffer;
			Width = width;
			Height = height;
			Depth = depth;
            Bounds = bounds;
        }

		public Voxel_t[] GetData() {
            // cache
            if (_voxels != null) return _voxels;

            _voxels = new Voxel_t[Buffer.count];
            Buffer.GetData(_voxels);
            return _voxels;
		}

		public void Dispose() {
			Buffer.Release();
		}
    }

}


using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoxelSystem.PointCloud;

namespace VoxelSystem.IO
{
    public static class Output
    {
        public static void WritePts(Point[] points, string path, string delimiter)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(points.Length);
                foreach (var point in points)
                {
                    var text = $"{point.point.x}{delimiter}{point.point.z}{delimiter}{point.point.y}";
                    writer.WriteLine(text);
                }
            }
        }

        public static void WritePts(PointColour[] points, string path, string delimiter)
        {
            using (StreamWriter writer = new StreamWriter(path))
            { 
                writer.WriteLine(points.Length);
                foreach (var point in points)
                {
                    var text =
                        $"{point.point.x}{delimiter}{point.point.z}{delimiter}{point.point.y}{delimiter}{(int) point.PointColor.r}{delimiter}{(int) point.PointColor.g}{delimiter}{(int) point.PointColor.b}";
                    writer.WriteLine(text);
                }
            }
        }

        public static void WritePts(List<Point> points, Color color, string path, string delimiter, bool append)
        {
            using (var writer = new StreamWriter(path, append))
            {
                foreach (var point in points)
                {
                    var text =
                        $"{point.point.x}{delimiter}{point.point.z}{delimiter}{point.point.y}{delimiter}{(int)color.r*255}{delimiter}{(int)color.g * 255}{delimiter}{(int)color.b * 255}";
                    writer.WriteLine(text);
                }
            }
        }


        public static void ExportPts(Voxel_t[] voxels, int width, int height, Vector3 pivotPoint, float voxelSize, string filePathExport, string delimiter)
        {
            var halfVoxelSize = voxelSize / 2;
            var pivotVoxelCentroid = pivotPoint + new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

            var points = new List<Point>();

            var i = 0;
            foreach (var voxel in voxels)
            {
                if (voxel.fill > 0)
                {
                    var voxelIndex = ArrayFunctions.Index1DTo3D(i, width, height);
                    var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * voxelSize,
                        voxelIndex.Y * voxelSize, voxelIndex.Z * voxelSize);
                    points.Add(new Point(point));
                }
                i++;
            }
            WritePts(points, Color.green, filePathExport, delimiter, true);

        }

        public static void ExportPts(MultiValueVoxelModel voxelModel,float voxelSize, string filePathExport, string delimiter)
        {
            var halfVoxelSize = voxelSize / 2;
            var pivotVoxelCentroid =
                voxelModel.Bounds.min + new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

            var points = new List<PointColour>();

            for (int i = 0; i < voxelModel.Voxels.Length; i++)
            {
                var voxelIndex = ArrayFunctions.Index1DTo3D(i, voxelModel.Width, voxelModel.Height);
                var voxel = voxelModel.Voxels[i];

                if (voxel != null)
                {
                    var point = pivotVoxelCentroid + new Vector3(voxelIndex.X * voxelSize, voxelIndex.Y * voxelSize,
                        voxelIndex.Z * voxelSize);
                    points.Add(new PointColour(point, voxel[0].VoxelColor * 256));
                }
            }

            WritePts(points.ToArray(), filePathExport, delimiter);
        }
    }
}

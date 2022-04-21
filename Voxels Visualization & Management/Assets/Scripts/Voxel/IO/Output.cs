using System;
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
                        $"{point.point.x}{delimiter}{point.point.z}{delimiter}{point.point.y}{delimiter}{(int) point.Red}{delimiter}{(int) point.Green}{delimiter}{(int) point.Blue}";
                    writer.WriteLine(text);
                }
            }
        }

        public static void WritePts(Point[] points, Color color, string path, string delimiter)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(points.Length);
                foreach (var point in points)
                {
                    var text =
                        $"{point.point.x}{delimiter}{point.point.z}{delimiter}{point.point.y}{delimiter}{(int)color.r*255}{delimiter}{(int)color.g * 255}{delimiter}{(int)color.b * 255}";
                    writer.WriteLine(text);
                }
            }
        }
    }
}

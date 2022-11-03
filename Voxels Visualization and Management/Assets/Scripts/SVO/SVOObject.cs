/*
Sparse Voxel Octrees Demo
Copyright (C) 2021 Alexander Goslin

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using SVO;
using UnityEngine;

public class SVOObject
{
    private readonly Octree _octree;
    public int Depth { get; }
    private readonly float _octreeSize;
    private readonly Color _voxelColor;
    private float _halfVoxelSize;

    public SVOObject()
    {

    }

    public SVOObject(Bounds bounds, float voxelSize, Color color)
    {
        _octree = new Octree();
        Depth = GetInitialDepth(bounds, voxelSize);
        _octreeSize = Mathf.Pow(2f, Depth) - 1;
        _voxelColor = new Color(color.r, color.g, color.b, 1f);
        _halfVoxelSize = voxelSize / 2;
    }

    private int GetInitialDepth(Bounds bounds, float voxelSize)
    {
        float maxSize = bounds.size.x;
        if (maxSize < bounds.size.y)
            maxSize = bounds.size.y;
        if (maxSize < bounds.size.z)
            maxSize = bounds.size.z;

        maxSize /= voxelSize;

        int initialDepth = 0;
        float octreeSize = Mathf.Pow(2, initialDepth);
        while (maxSize > octreeSize)
        {
            initialDepth++;
            octreeSize = Mathf.Pow(2, initialDepth);
        }

        return initialDepth;
    }

    public void AddVoxelsToSVO(bool[] voxels, int sizeX, int sizeY, Vector3 pivotPoint, float voxelSize)
    {
        pivotPoint = new Vector3(pivotPoint.x + _halfVoxelSize, pivotPoint.y + _halfVoxelSize,
            pivotPoint.z + _halfVoxelSize);
        for (var i = 0; i < voxels.Length; i++)
        {
            if (!voxels[i]) continue;

            var index = ArrayFunctions.Index1DTo3D(i, sizeX, sizeY);
            var voxelPosition = new Vector3(index.X * voxelSize + pivotPoint.x, index.Y * voxelSize + pivotPoint.y, index.Z * voxelSize + pivotPoint.z);
            var voxelOctreePosition = new Vector3(voxelPosition.x / _octreeSize - 0.5f, voxelPosition.y / _octreeSize - 0.5f,
                voxelPosition.z / _octreeSize - 0.5f);
            _octree.SetVoxel(voxelOctreePosition, Depth, _voxelColor, Array.Empty<int>());
        }
    }

    public void AddVoxelsWithColorToSVO(List<VoxelObject>[] voxels, int sizeX, int sizeY, Vector3 pivotPoint, float voxelSize)
    {
        pivotPoint = new Vector3(pivotPoint.x + _halfVoxelSize, pivotPoint.y + _halfVoxelSize,
            pivotPoint.z + _halfVoxelSize);
        for (var i = 0; i < voxels.Length; i++)
        {
            var voxel = voxels[i];
            if (voxel.Any())
            {
                var index = ArrayFunctions.Index1DTo3D(i, sizeX, sizeY);
                var voxelPosition = new Vector3(index.X * voxelSize + pivotPoint.x, index.Y * voxelSize + pivotPoint.y, index.Z * voxelSize + pivotPoint.z);
                var voxelOctreePosition = new Vector3(voxelPosition.x / _octreeSize - 0.5f,
                    voxelPosition.y / _octreeSize - 0.5f,
                    voxelPosition.z / _octreeSize - 0.5f);
                _octree.SetVoxel(voxelOctreePosition, Depth, voxel[0].VoxelColor, Array.Empty<int>());
            }
        }
    }

    public void VisualizeOctree()
    {
        var material = Resources.Load("OctreeLit", typeof(Material)) as Material;

        material.mainTexture = _octree.Apply(false);
    }
}

 
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

using System.Collections.Generic;
using System.Linq;
using SVO;
using UnityEngine;
using Random = System.Random;

public class VoxelPlacer
{
    public int Depth { get; private set; }
    
    // Start is called before the first frame update
    public void BuildSVO(bool[] voxels, int sizeX, int sizeY, float voxelSize, Bounds bounds, Color color)
    {
        var _octree = new Octree();         
        Depth = GetInitialDepth(bounds, voxelSize);
        var octreeSize = Mathf.Pow(2f, Depth) - 1;
        var material = Resources.Load("OctreeLit", typeof(Material)) as Material;
        var halfVoxelSize = voxelSize / 2;
        var initialVoxelPosition = new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);
        var newColor = new Color(color.r, color.g, color.b, 1f);

        for (var i = 0; i < voxels.Length; i++)
        {
            if (voxels[i])
            {
                var index = ArrayFunctions.Index1DTo3D(i, sizeX, sizeY);
                var voxelPosition = new Vector3(index.X*voxelSize, index.Y * voxelSize, index.Z * voxelSize) + initialVoxelPosition;
                var voxelOctreePosition = new Vector3(voxelPosition.x / octreeSize - 0.5f, voxelPosition.y / octreeSize - 0.5f,
                    voxelPosition.z / octreeSize - 0.5f);
                _octree.SetVoxel(voxelOctreePosition, Depth, newColor, new int[0]);
            }
        }

        material.mainTexture = _octree.Apply(false);
    }

    public void BuildSVOWithColor(List<VoxelObject>[] voxels, int sizeX, int sizeY, float voxelSize, Bounds bounds)
    {
        var _octree = new Octree();
        Depth = GetInitialDepth(bounds, voxelSize);
        var octreeSize = Mathf.Pow(2f, Depth) - 1;
        var material = Resources.Load("OctreeLit", typeof(Material)) as Material;
        var halfVoxelSize = voxelSize / 2;
        var initialVoxelPosition = new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);

        for (var i = 0; i < voxels.Length; i++)
        {
            var voxel = voxels[i];
            if (voxel.Any())
            {
                var index = ArrayFunctions.Index1DTo3D(i, sizeX, sizeY);
                var voxelPosition = new Vector3(index.X * voxelSize, index.Y * voxelSize, index.Z * voxelSize) +
                                    initialVoxelPosition;
                var voxelOctreePosition = new Vector3(voxelPosition.x / octreeSize - 0.5f,
                    voxelPosition.y / octreeSize - 0.5f,
                    voxelPosition.z / octreeSize - 0.5f);
                _octree.SetVoxel(voxelOctreePosition, Depth, voxel[0].VoxelColor, new int[0]);
            }

        }

        material.mainTexture = _octree.Apply(false);
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

    private Dictionary<int, Color> GetComponentColors(int[] components)
    {
        var componentColors = new Dictionary<int, Color>(components.Length);

        foreach (var component in components)
        {
            componentColors.Add(component, new Color(UnityEngine.Random.Range(0,1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f),1f));
        }

        return componentColors;
    }
}

 
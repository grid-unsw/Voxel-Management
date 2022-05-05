using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ArrayFunctions
{
    public static int Index3DTo1D(int x, int y, int z, int xMax, int yMax)
    {
        return (z * xMax * yMax) + (y * xMax) + x;
    }

    public static Index3D Index1DTo3D(int idx, int xMax, int yMax)
    {
        var z = idx / (xMax * yMax);
        idx -= (z * xMax * yMax);
        var y = idx / xMax;
        var x = idx % xMax;
        return new Index3D(x,y,z);
    }

    /// <summary>
    /// Identify connected components in 2D based on a filter
    /// </summary>
    public static (int[,], int[]) GetConnectedComponents2D(bool[,] voxels, bool[,] filter)
    {
        var sizeX = voxels.GetLength(0);
        var sizeY = voxels.GetLength(1);
        var connectedComponents = new int[sizeX, sizeY];
        var componentsIds = new List<int>();

        var sizeFilterX = filter.GetLength(0);
        var sizeFilterY = filter.GetLength(1);

        if (sizeFilterX % 2 == 0 || sizeFilterY % 2 == 0)
        {
            Debug.Log("Filter should have lenght sizes that is odd");
        }
        else
        {
            var extendX = Mathf.FloorToInt(sizeFilterX / 2);
            var extendY = Mathf.FloorToInt(sizeFilterY / 2);

            var ccId = 1;
            var connections = new List<(int, int)>();

            for (int j = 0; j < sizeY; j++)
                for (int i = 0; i < sizeX; i++)
                    if (voxels[i, j])
                    {
                        int m = 0;
                        int n = 0;
                        List<int> componentsId = new List<int>();
                        int minj1X = i - extendX;
                        int minj1Y = j - extendY;
                        if (minj1X < 0)
                            minj1X = 0;
                        if (minj1Y < 0)
                            minj1Y = 0;
                        for (int j1 = minj1Y; j1 <= j + extendY && j1 < sizeY; j1++)
                        {
                            for (int i1 = minj1X; i1 <= i + extendX && i1 < sizeX; i1++)
                            {
                                if (filter[m, n] && voxels[i1, j1])
                                {
                                    int currnetCcId = connectedComponents[i1, j1];
                                    if (currnetCcId != 0)
                                    {
                                        if (!componentsId.Contains(currnetCcId))
                                        {
                                            //udapte connections list
                                            foreach (var componentId in componentsId)
                                            {
                                                connections.Add((componentId, currnetCcId));
                                            }
                                            componentsId.Add(currnetCcId);
                                        }
                                    }
                                }
                                m++;
                            }
                            n++;
                            m = 0;
                        }

                        //update of cc array
                        if (componentsId.Count > 0)
                            connectedComponents[i, j] = componentsId[0];
                        else
                        {
                            connectedComponents[i, j] = ccId;
                            ccId++;
                        }
                    }

            //connecting components
            int[] components = new int[ccId];
            for (int i = 1; i < ccId; i++)
            {
                components[i] = i;
            }
            var graph = new Graph<int>(components, connections);
            Algorithms algorithms = new Algorithms();

            List<int> visitedComponents = new List<int>();
            Dictionary<int, int> equivalencyList = new Dictionary<int, int>();
            for (int i = 1; i < ccId; i++)
            {
                if (!visitedComponents.Contains(i))
                {
                    var connectedConnections = algorithms.DFS(graph, i);
                    visitedComponents.AddRange(connectedConnections);
                    int minId = connectedConnections.Min(x => x);
                    foreach (var component in connectedConnections)
                    {
                        equivalencyList.Add(component, minId);
                    }
                    componentsIds.Add(minId);
                }
            }

            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                {
                    int componentId = connectedComponents[i, j];
                    if (componentId != 0)
                    {
                        connectedComponents[i, j] = equivalencyList[componentId];
                    }
                }

        }
        return (connectedComponents, componentsIds.ToArray());
    }

    /// <summary>
    /// Identify connected components in 3D based on a filter
    /// </summary>
    public static (int[,,], int[]) GetConnectedComponents3D(bool[,,] voxels, bool[,,] filter)
    {
        var sizeX = voxels.GetLength(0);
        var sizeY = voxels.GetLength(1);
        var sizeZ = voxels.GetLength(2);
        var connectedComponents = new int[sizeX, sizeY, sizeZ];
        var componentsIds = new List<int>();

        var sizeFilterX = filter.GetLength(0);
        var sizeFilterY = filter.GetLength(1);
        var sizeFilterZ = filter.GetLength(2);

        if (sizeFilterX % 2 == 0 || sizeFilterY % 2 == 0 || sizeFilterZ % 2 == 0)
        {
            Debug.Log("Filter should have lenght sizes that is odd");
        }
        else
        {
            var extendX = Mathf.FloorToInt(sizeFilterX / 2);
            var extendY = Mathf.FloorToInt(sizeFilterY / 2);
            var extendZ = Mathf.FloorToInt(sizeFilterZ / 2);

            var ccId = 1;
            var connections = new List<(int, int)>();

            for (int j = 0; j < sizeY; j++)
                for (int i = 0; i < sizeX; i++)
                    for (int k = 0; k < sizeZ; k++)
                        if (voxels[i, j, k])
                        {
                            int m = 0;
                            int n = 0;
                            int p = 0;
                            List<int> componentsId = new List<int>();
                            int minj1X = i - extendX;
                            int minj1Y = j - extendY;
                            int minj1Z = k - extendZ;
                            if (minj1X < 0)
                                minj1X = 0;
                            if (minj1Y < 0)
                                minj1Y = 0;
                            if (minj1Z < 0)
                                minj1Z = 0;
                            for (int j1 = minj1Y; j1 <= j + extendY && j1 < sizeY; j1++)
                            {
                                for (int i1 = minj1X; i1 <= i + extendX && i1 < sizeX; i1++)
                                {
                                    for (int k1 = minj1Z; k1 <= k + extendZ && k1 < sizeZ; k1++)
                                    {
                                        if (filter[m, n, p] && voxels[i1, j1, k1])
                                        {
                                            int currnetCcId = connectedComponents[i1, j1, k1];
                                            if (currnetCcId != 0)
                                            {
                                                if (!componentsId.Contains(currnetCcId))
                                                {
                                                    //udapte connections list
                                                    foreach (var componentId in componentsId)
                                                    {
                                                        connections.Add((componentId, currnetCcId));
                                                    }
                                                    componentsId.Add(currnetCcId);
                                                }
                                            }
                                        }
                                        p++;
                                    }
                                    m++;
                                    p = 0;
                                }
                                n++;
                                m = 0;
                            }

                            //update of cc array
                            if (componentsId.Count > 0)
                                connectedComponents[i, j, k] = componentsId[0];
                            else
                            {
                                connectedComponents[i, j, k] = ccId;
                                ccId++;
                            }
                        }

            //connecting components
            int[] components = new int[ccId];
            for (int i = 1; i < ccId; i++)
            {
                components[i] = i;
            }
            var graph = new Graph<int>(components, connections);
            Algorithms algorithms = new Algorithms();

            List<int> visitedComponents = new List<int>();
            Dictionary<int, int> equivalencyList = new Dictionary<int, int>();
            for (int i = 1; i < ccId; i++)
            {
                if (!visitedComponents.Contains(i))
                {
                    var connectedConnections = algorithms.DFS(graph, i);
                    visitedComponents.AddRange(connectedConnections);
                    int minId = connectedConnections.Min(x => x);
                    foreach (var component in connectedConnections)
                    {
                        equivalencyList.Add(component, minId);
                    }
                    componentsIds.Add(minId);
                }
            }

            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    for (int k = 0; k < sizeZ; k++)
                    {
                        int componentId = connectedComponents[i, j, k];
                        if (componentId != 0)
                        {
                            connectedComponents[i, j, k] = equivalencyList[componentId];
                        }
                    }

        }
        return (connectedComponents, componentsIds.ToArray());
    }

}

using Npgsql;
using UnityEditor;
using UnityEngine;

public static class DBquery
{
    static string conString = "Host=localhost;Username=postgres;Password=grid21;Database=postgres;Pooling=true;Keepalive=15;";
    static NpgsqlConnection conn = new NpgsqlConnection(conString);
   
    public static bool[,,] RemovingUnreachableNavigableVoxels(bool[,,] voxels, Bounds bounds, float voxelSize)
    {
        var cs = "Host=localhost;Username=postgres;Password=grid21;Database=postgres;Pooling=true;";
        string sql = "SELECT * from triangles1 where ST_3DDWithin(geom, ST_SetSRID(ST_MakePoint(:x,:y,:z), 7856),0.5) limit 1";

        int sizeX = voxels.GetLength(0);
        int sizeY = voxels.GetLength(1);
        int sizeZ = voxels.GetLength(2);
        bool[,,] reachableVoxels = new bool[sizeX, sizeY, sizeZ];
        Vector3 initModelPos = new Vector3(bounds.min.x + voxelSize / 2, bounds.min.y + voxelSize / 2, bounds.min.z + voxelSize / 2);
        //var clock = System.Diagnostics.Stopwatch.StartNew();
        using (var conn = new NpgsqlConnection(cs))
        {
            conn.Open();
            var cmd = new NpgsqlCommand(sql,conn);

            var p1 = cmd.Parameters.Add("x", NpgsqlTypes.NpgsqlDbType.Real);
            var p2 = cmd.Parameters.Add("y", NpgsqlTypes.NpgsqlDbType.Real);
            var p3 = cmd.Parameters.Add("z", NpgsqlTypes.NpgsqlDbType.Real);
            cmd.Prepare();

            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    for (int k = 0; k < sizeZ; k++)
                    {
                        if (voxels[i, j, k])
                        {
                            Vector3 voxelCentroid = new Vector3(initModelPos.x + i * voxelSize, initModelPos.y + j * voxelSize, initModelPos.z + k * voxelSize);
                            p1.Value = voxelCentroid.x;
                            p2.Value = voxelCentroid.z;
                            p3.Value = voxelCentroid.y;
                            //p4.Value = distance;

                            var exist = cmd.ExecuteScalar();
                            if (exist != null)
                            {
                                reachableVoxels[i, j, k] = true;
                            }
                        }
                    }

        }

        return reachableVoxels;
    }


    
}

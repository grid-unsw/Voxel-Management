﻿using UnityEngine;
using Npgsql;
using UnityEditor;
using VoxelSystem;

public static class DBexport
{
    private static string GetConnectionString()
    {
        var voxelEditor = (VoxelEditor)EditorWindow.GetWindow(typeof(VoxelEditor));
        
        var connectionString = voxelEditor.GetConnectionString();
        voxelEditor.Close();

        return connectionString;
    }

    public static void ExportVoxels(Voxel_t[] voxels, int width, int height, Vector3 pivotPoint, float voxelSize, string tableName, bool truncate)
    {
        var connectionString = GetConnectionString();
        tableName = tableName.ToLower();

        if (!CheckIfTableExist(tableName, connectionString))
        {
            CreateTable(tableName, connectionString, "(geom GEOMETRY(POINTZ))");
        }
        else
        {
            if (truncate)
            {
                TruncateTable(tableName, connectionString);
            }
        }

        var sql2 = $"INSERT INTO {tableName} (geom) Values(ST_SetSRID(ST_MakePoint(:x, :y, :z), 7856))";

        var halfVoxelSize = voxelSize / 2;
        var initModelPos = new Vector3(pivotPoint.x + halfVoxelSize, pivotPoint.y + halfVoxelSize, pivotPoint.z + halfVoxelSize);

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql2;

            var p1 = cmd.Parameters.Add("x", NpgsqlTypes.NpgsqlDbType.Real);
            var p2 = cmd.Parameters.Add("y", NpgsqlTypes.NpgsqlDbType.Real);
            var p3 = cmd.Parameters.Add("z", NpgsqlTypes.NpgsqlDbType.Real);
            cmd.Prepare();

            for (var i = 0; i < voxels.Length; i++)
            {
                if (voxels[i].fill <= 0) continue;

                var index = ArrayFunctions.Index1DTo3D(i, width, height);
                var voxelCentroid = new Vector3(initModelPos.x + index.X * voxelSize,
                    initModelPos.y + index.Y * voxelSize, initModelPos.z + index.Z * voxelSize);
                p1.Value = voxelCentroid.x;
                p2.Value = voxelCentroid.z;//switch y and z
                p3.Value = voxelCentroid.y; 
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void ExportVoxels(MultiValueVoxelModel voxelModel, int width, int height, Vector3 pivotPoint, float voxelSize, string tableName, bool truncate)
    {
        var connectionString = GetConnectionString();
        tableName = tableName.ToLower();

        if (!CheckIfTableExist(tableName, connectionString))
        {
            CreateTable(tableName, connectionString, "(objectId Integer, geom GEOMETRY(POINTZ))");
        }
        else
        {
            if (truncate)
            {
                TruncateTable(tableName, connectionString);
            }
        }

        var sql2 = $"INSERT INTO {tableName} (objectid, geom) Values(:objectid, ST_SetSRID(ST_MakePoint(:x, :y, :z), 7856))";

        var halfVoxelSize = voxelSize / 2;
        var initModelPos = new Vector3(pivotPoint.x + halfVoxelSize, pivotPoint.y + halfVoxelSize, pivotPoint.z + halfVoxelSize);

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql2;

            var p0 = cmd.Parameters.Add("objectid", NpgsqlTypes.NpgsqlDbType.Integer);
            var p1 = cmd.Parameters.Add("x", NpgsqlTypes.NpgsqlDbType.Real);
            var p2 = cmd.Parameters.Add("y", NpgsqlTypes.NpgsqlDbType.Real);
            var p3 = cmd.Parameters.Add("z", NpgsqlTypes.NpgsqlDbType.Real);

            cmd.Prepare();

            for (var i = 0; i < voxelModel.Voxels.Length; i++)
            {
                var voxel = voxelModel.Voxels[i];
                if (voxel == null) continue;

                var index = ArrayFunctions.Index1DTo3D(i, width, height);
                var voxelCentroid = new Vector3(initModelPos.x + index.X * voxelSize,
                    initModelPos.y + index.Y * voxelSize, initModelPos.z + index.Z * voxelSize);
                p0.Value = voxel[0].Id;
                p1.Value = voxelCentroid.x;
                p2.Value = voxelCentroid.z;//switch y and z
                p3.Value = voxelCentroid.y;
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static void CreateTable(string tableName, string connectionString, string fields)
    {
        var sql = $"CREATE TABLE {tableName} {fields};";
        var connection = new NpgsqlConnection(connectionString);
        var cmd = EstablishConnectionWithQuery(connection, sql);
        cmd.ExecuteNonQuery();
    }

    private static bool CheckIfTableExist(string tableName, string connectionString)
    {
        var sql = $"SELECT EXISTS ( SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '{tableName}' );";
        var connection = new NpgsqlConnection(connectionString);
        var cmd = EstablishConnectionWithQuery(connection, sql);
        return (bool)cmd.ExecuteScalar();
    }

    private static void TruncateTable(string tableName, string connectionString)
    {
        var sql = $"TRUNCATE TABLE {tableName};";
        var connection = new NpgsqlConnection(connectionString);
        var cmd = EstablishConnectionWithQuery(connection, sql);
        cmd.ExecuteNonQuery();
    }

    private static NpgsqlCommand EstablishConnectionWithQuery(NpgsqlConnection connection, string sql)
    {
        connection.Open();
        var cmd = new NpgsqlCommand();
        cmd.Connection = connection;
        cmd.CommandText = sql;
        return cmd;
    }

    private static void CreatePgPointcloudSchema(string connectionString)
    {
        const string quote = "\"";

        var sql = "CREATE EXTENSION IF NOT EXISTS POSTGIS;" +
            "CREATE EXTENSION IF NOT EXISTS pointcloud;" +
            "CREATE EXTENSION IF NOT EXISTS pointcloud_postgis; " +
            "INSERT INTO pointcloud_formats (pcid, srid, schema) VALUES (1, 7856,'<?xml version="
            + quote + "1.0" + quote + " encoding=" + quote + "UTF-8" + quote + "?>" +
            "<pc:PointCloudSchema xmlns:pc=" + quote + "http://pointcloud.org/schemas/PC/1.1" + quote + "" +
            "    xmlns:xsi=" + quote + "http://www.w3.org/2001/XMLSchema-instance" + quote + ">" +
            "<pc:dimension>" +
            "<pc:position>1</pc:position>" +
            "    <pc:size>4</pc:size>" +
            "    <pc:description>X coordinate as a long integer. You must use the" +
            "                    scale and offset information of the header to" +
            "                    determine the double value.</pc:description>" +
            "    <pc:name>X</pc:name>" +
            "    <pc:interpretation>int32_t</pc:interpretation>" +
            "    <pc:scale>0.01</pc:scale>" +
            "  </pc:dimension>" +
            "  <pc:dimension>" +
            "    <pc:position>2</pc:position>" +
            "    <pc:size>4</pc:size>" +
            "    <pc:description>Y coordinate as a long integer. You must use the" +
            "                    scale and offset information of the header to" +
            "                    determine the double value.</pc:description>" +
            "    <pc:name>Y</pc:name>" +
            "    <pc:interpretation>int32_t</pc:interpretation>" +
            "    <pc:scale>0.01</pc:scale>" +
            "  </pc:dimension>" +
            "  <pc:dimension>" +
            "    <pc:position>3</pc:position>" +
            "    <pc:size>4</pc:size>" +
            "    <pc:description>Z coordinate as a long integer. You must use the" +
            "                    scale and offset information of the header to" +
            "                    determine the double value.</pc:description>" +
            "    <pc:name>Z</pc:name>" +
            "    <pc:interpretation>int32_t</pc:interpretation>" +
            "    <pc:scale>0.01</pc:scale>" +
            "  </pc:dimension>" +
            "  <pc:dimension>" +
            "    <pc:position>4</pc:position>" +
            "    <pc:size>2</pc:size>" +
            "    <pc:description>The obj calss is the integer representation" +
            "                    of the object semantic information. This value is optional" +
            "                    and system specific. However, it should always be" +
            "                    included if available.</pc:description>" +
            "    <pc:name>objID</pc:name>" +
            "    <pc:interpretation>uint16_t</pc:interpretation>" +
            "  </pc:dimension>" +
            "  <pc:dimension>" +
            "    <pc:position>5</pc:position>" +
            "    <pc:size>2</pc:size>" +
            "    <pc:description>Ifc Space id that the voxel belongs to.</pc:description>" +
            "    <pc:name>objID</pc:name>" +
            "    <pc:interpretation>uint16_t</pc:interpretation>" +
            "  </pc:dimension>" +
            "  <pc:dimension>" +
            "    <pc:position>6</pc:position>" +
            "    <pc:size>4</pc:size>" +
            "    <pc:description>Actual height.</pc:description>" +
            "    <pc:name>height</pc:name>" +
            "    <pc:interpretation>int16_t</pc:interpretation>" +
            "    <pc:scale>0.01</pc:scale>  " +
            "  </pc:dimension>" +
            "  <pc:metadata>" +
            "    <Metadata name=" + quote + "compression" + quote + ">dimensional</Metadata>" +
            "  </pc:metadata>" +
            "</pc:PointCloudSchema>');";

        var connection = new NpgsqlConnection(connectionString);
        var cmd = EstablishConnectionWithQuery(connection, sql);
        cmd.ExecuteNonQuery();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmeshMaterial
{
    public int Id { get; }
    public Material Mat { get; }

    public SubmeshMaterial(int id, Material mat)
    {
        Id = id;
        Mat = mat;
    }
}

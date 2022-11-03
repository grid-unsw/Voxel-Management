using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreatePyramid))]
public class CreatePiramidEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var piramid = (CreatePyramid) target;

        if (GUILayout.Button("Run"))
        {
            piramid.Create();
        }
    }
}

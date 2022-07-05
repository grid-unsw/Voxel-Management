using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VoxelEditor : EditorWindow
{
    private VoxelEditorData _voxelEditorData;

    [MenuItem("Tools/Voxel Editor")]
    public static void ShowVoxelEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        var wnd = GetWindow<VoxelEditor>();
        wnd.titleContent = new GUIContent("Voxel Editor");
    }

    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
        if (AssetDatabase.LoadAssetAtPath("Assets/Resources/VoxelEditorData.asset", typeof(VoxelEditorData)) == null)
        {
            _voxelEditorData = CreateInstance<VoxelEditorData>();
        }
        else
        {
            _voxelEditorData = (VoxelEditorData)AssetDatabase.LoadAssetAtPath("Assets/Resources/VoxelEditorData.asset", typeof(VoxelEditorData));
        }
    }

    public string GetConnectionString()
    {
        if (_voxelEditorData.Host == "" || _voxelEditorData.Username == "" || _voxelEditorData.Password == "" ||
            _voxelEditorData.Database == "")
        {
            throw new InvalidDataException("Database connection field are not set up");
        }

        return $"Host={_voxelEditorData.Host}; Username={_voxelEditorData.Username}; Password={_voxelEditorData.Password}; Database={_voxelEditorData.Database}";
    }

    private void OnGUI()
    {
        GUILayout.Label("Database connection");
        _voxelEditorData.Host = EditorGUILayout.TextField("Host", _voxelEditorData.Host);
        _voxelEditorData.Username = EditorGUILayout.TextField("Username", _voxelEditorData.Username);
        _voxelEditorData.Password = EditorGUILayout.TextField("Password", _voxelEditorData.Password);
        _voxelEditorData.Database = EditorGUILayout.TextField("Database", _voxelEditorData.Database);
    }
    
    void OnDestroy()
    {
        if (AssetDatabase.LoadAssetAtPath("Assets/Resources/VoxelEditorData.asset", typeof(VoxelEditorData)) == null)
        {
            AssetDatabase.CreateAsset(_voxelEditorData, "Assets/Resources/VoxelEditorData.asset");
        }
        else
        {
            EditorUtility.SetDirty(_voxelEditorData);
            AssetDatabase.SaveAssets();
        }
    }
}

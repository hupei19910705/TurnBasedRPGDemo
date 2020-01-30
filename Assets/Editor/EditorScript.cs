using System.Collections;
using UnityEditor;
using UnityEngine;

public class EditorScript
{
    [MenuItem("Assets/MyTools/LogAssetName", true, 1)]
    static bool IsValidate()
    {
        return string.Equals(Selection.activeObject.name, "Test");
    }

    [MenuItem("Assets/MyTools/LogAssetName", false, 1)]
    static void LogAssetName()
    {
        Debug.Log(Selection.activeObject.name);
    }

    [MenuItem("Assets/Create/MyCreate/Cube",false,1)]
    static void CreateCube()
    {
        GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    [MenuItem("Root/Test1", false, 1)]
    static void CreateTest1()
    {
        var menuPath = "Root/Test1";
        bool mChecked = Menu.GetChecked(menuPath);
        Menu.SetChecked(menuPath, !mChecked);
    }

    [MenuItem("CONTEXT/Transform/MyContext")]
    static void MyContext(MenuCommand command)
    {
        Debug.Log(command.context.name);
    }
}

[CustomEditor(typeof(Camera))]
public class EditorCamera : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("拓展按钮")) { }
        base.OnInspectorGUI();
    }
}
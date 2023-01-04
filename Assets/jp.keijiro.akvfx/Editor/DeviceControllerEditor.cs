using UnityEngine;
using UnityEditor;

namespace Akvfx {

[CanEditMultipleObjects]
[CustomEditor(typeof(DeviceController))]
sealed class DeviceControllerEditor : Editor
{
    SerializedProperty _deviceSettings;
    SerializedProperty deviceIndex;

    void OnEnable()
    {
        _deviceSettings = serializedObject.FindProperty("_deviceSettings");
        deviceIndex = serializedObject.FindProperty("deviceIndex");
    }

    
    
        

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_deviceSettings);
        EditorGUILayout.PropertyField(deviceIndex);
        serializedObject.ApplyModifiedProperties();
    }
}

}

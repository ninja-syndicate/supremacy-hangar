using System;
using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomEditor(typeof(Runtime.ContentLoader.AssetMappings))]
    public class AssetMappingsEditor : UnityEditor.Editor
    {
        private SerializedProperty factionsProperty;
        private SerializedProperty mechChassisProperty;
        private SerializedProperty mechSkinsProperty;

        private void OnEnable()
        {
            factionsProperty = serializedObject.FindProperty("factions");
            mechChassisProperty = serializedObject.FindProperty("mechChassis");
            mechSkinsProperty = serializedObject.FindProperty("mechSkins");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(factionsProperty);
            EditorGUILayout.PropertyField(mechChassisProperty);
            EditorGUILayout.PropertyField(mechSkinsProperty);
        }
    }
}
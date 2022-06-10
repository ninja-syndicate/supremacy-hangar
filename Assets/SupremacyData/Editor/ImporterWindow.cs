using System;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    public class ImporterWindow : EditorWindow
    {
        private Runtime.Data myData;
        
        [MenuItem("Supremacy/Data/Importer")]
        public static void Spawn()
        {
            var window = GetWindow<ImporterWindow>();
            window.Show();
        }

        public void OnGUI()
        {
            myData = EditorGUILayout.ObjectField(
                "Static Data Object",
                myData, 
                typeof(Runtime.Data), 
                false) as Runtime.Data;
            EditorGUI.BeginDisabledGroup(myData);
            if (GUILayout.Button("Create")) CreateDataObject();
            {
                Debug.Log("Create!");
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateDataObject()
        {
            myData = CreateInstance<Runtime.Data>();
            AssetDatabase.CreateAsset(myData, "Assets/Settings/Static Data.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
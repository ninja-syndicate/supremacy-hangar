using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    public class ImporterWindow : EditorWindow
    {
        private const float SpacerAmount = 5.0f;
        
        private Runtime.Data myData;
        private string importDirectory;
        private readonly LogWidget logWidget = new LogWidget();
        private bool busy;
        private Vector2 logWindowPos;

        private FactionsImporter factionsImporter;
        
        [MenuItem("Supremacy/Data/Importer")]
        public static void Spawn()
        {
            var window = GetWindow<ImporterWindow>();
            window.titleContent = new GUIContent("Static Data Importer");
            window.Show();
        }

        public void OnGUI()
        {
            RenderDataObjectFields();
            GUILayout.Space(SpacerAmount);
            RenderImportDirectoryFields();
            
            GUILayout.Space(SpacerAmount);
            EditorGUI.BeginDisabledGroup(!ReadyForImport() && !busy);
            string label = busy ? "Updating..." : "Update Static Data";
            if (GUILayout.Button(label)) StartImport();
            EditorGUI.EndDisabledGroup();
            
            logWindowPos = EditorGUILayout.BeginScrollView(logWindowPos, GUILayout.ExpandHeight(true));
            logWidget.Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
        }
        
        private void RenderDataObjectFields()
        {
            
            myData = EditorGUILayout.ObjectField(
                "Static Data Object",
                myData, 
                typeof(Runtime.Data), 
                false) as Runtime.Data;
            EditorGUI.BeginDisabledGroup(myData);
            if (GUILayout.Button("Create")) CreateDataObject();
            EditorGUI.EndDisabledGroup();            
        }
        
        private void RenderImportDirectoryFields()
        {
            string label = String.IsNullOrWhiteSpace(importDirectory) ? "Select Import Directory" : $"{importDirectory}";
            if (GUILayout.Button(label)) SelectImportDirectory();
        }

        private bool ReadyForImport()
        {
            if (myData == null) return false;
            if (String.IsNullOrWhiteSpace(importDirectory)) return false;
            return Directory.Exists(importDirectory);
        }
        
        private void SelectImportDirectory()
        {
            var directory = EditorUtility.OpenFolderPanel("Select Static Data Directory", importDirectory, "test");
            if (!Directory.Exists(directory)) return;
            //TODO: verify the directory contains import stuff
            factionsImporter = new FactionsImporter(logWidget, directory);

            bool valid = true;
            valid &= factionsImporter.ValidateFile();

            if (!valid) return;
            Debug.Log("things were valid");
            importDirectory = directory;
        }

        private void CreateDataObject()
        {
            myData = CreateInstance<Runtime.Data>();
            AssetDatabase.CreateAsset(myData, "Assets/Settings/Static Data.asset");
            AssetDatabase.SaveAssets();
        }

        private async void StartImport()
        {
            busy = true;
            logWidget.Reset();
            logWidget.LogNormal("Importing...");
            try
            {
                factionsImporter ??= new FactionsImporter(logWidget, importDirectory);

                await factionsImporter.Update(myData);
                
                logWidget.LogNormal("Import completed");
            }
            catch (Exception e)
            {
                logWidget.LogError("Exception occured during import!");
                logWidget.LogError(e.ToString());
                logWidget.LogError(e.StackTrace);
            }
            finally
            {
                EditorUtility.SetDirty(myData);
                AssetDatabase.SaveAssetIfDirty(myData);
                busy = false;
                Repaint();
            }
        }
    }
}

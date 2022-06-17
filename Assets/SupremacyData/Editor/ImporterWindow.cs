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
        
        private Importers.Factions factionsImporter;
        private Importers.Brands brandsImporter;
        private Importers.BattleAbilities battleAbilitiesImporter;
        private Importers.GameAbilities gameAbilitiesImporter;
        private Importers.MechModels mechModelsImporter;
        private Importers.MechSkins mechSkinsImporter;
        private Importers.MysteryCrates mysteryCratesImporter;
        
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

            factionsImporter = new Importers.Factions(logWidget, directory);
            brandsImporter = new Importers.Brands(logWidget, directory);
            battleAbilitiesImporter = new Importers.BattleAbilities(logWidget, directory);
            gameAbilitiesImporter = new Importers.GameAbilities(logWidget, directory);
            mechModelsImporter = new Importers.MechModels(logWidget, directory);
            mechSkinsImporter = new Importers.MechSkins(logWidget, directory);
            mysteryCratesImporter = new Importers.MysteryCrates(logWidget, directory);

            bool valid = true;
            valid &= factionsImporter.ValidateFile();
            valid &= brandsImporter.ValidateFile();
            valid &= battleAbilitiesImporter.ValidateFile();
            valid &= gameAbilitiesImporter.ValidateFile();
            valid &= mechModelsImporter.ValidateFile();
            valid &= mechSkinsImporter.ValidateFile();
            valid &= mysteryCratesImporter.ValidateFile();

            if (!valid) return;
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
            logWidget.LogNormal("Update Begins");
            try
            {
                factionsImporter ??= new Importers.Factions(logWidget, importDirectory);
                brandsImporter ??= new Importers.Brands(logWidget, importDirectory);
                battleAbilitiesImporter ??= new Importers.BattleAbilities(logWidget, importDirectory);
                gameAbilitiesImporter ??= new Importers.GameAbilities(logWidget, importDirectory);
                mechModelsImporter ??= new Importers.MechModels(logWidget, importDirectory);
                mechSkinsImporter ??= new Importers.MechSkins(logWidget, importDirectory);
                mysteryCratesImporter ??= new Importers.MysteryCrates(logWidget, importDirectory);

                logWidget.LogNormal("Updating factions");
                Repaint();
                await factionsImporter.Update(myData);
                logWidget.LogNormal("Updating brands");
                Repaint();
                await brandsImporter.Update(myData);
                logWidget.LogNormal("Updating battle abilities");
                Repaint();
                await battleAbilitiesImporter.Update(myData);
                logWidget.LogNormal("Updating game abilities");
                Repaint();
                await gameAbilitiesImporter.Update(myData);
                logWidget.LogNormal("Updating mech models");
                Repaint();
                await mechModelsImporter.Update(myData);
                logWidget.LogNormal("Updating mech skins");
                Repaint();
                await mechSkinsImporter.Update(myData);
                logWidget.LogNormal("Updating mystery crates");
                Repaint();
                await mysteryCratesImporter.Update(myData);
                
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

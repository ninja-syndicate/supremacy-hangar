// CreateMaterialsForTextures.cs
// C#
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using SupremacyData.Editor;
using SupremacyHangar.Runtime.ScriptableObjects;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace SupremacyHangar.Editor
{
    public class GenerateMaterialsForMesh : EditorWindow
    {
        public Shader shader;
        private string importDirectory;

        List<string> materialNames = new();

        private LogWidget logWidget = new();
        private Vector2 scrollPosition;

        private string meshName = "";
        private string activePath = "";

        [MenuItem("Assets/GenerateMaterialsForMesh")]
        public static void Spawn()
        {
            var window = GetWindow<GenerateMaterialsForMesh>();
            window.titleContent = new GUIContent("Material Generator");
            window.Show();
        }
        public void OnGUI()
        {
            RenderImportDirectoryFields();
            RenderSelectionFields();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Mesh Name: ");
                meshName = EditorGUILayout.TextField(meshName, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            logWidget.Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            EditorGUILayout.EndScrollView();
        }
        
        private MeshMaterialNames materialNameMap;
        private GameObject SelectedMesh;
        void OnEnable()
        {
            shader = Shader.Find("Supremacy/MechShader");
            materialNameMap = AssetDatabase.LoadAssetAtPath("Assets/Editor/MeshMaterialNameMap.asset", typeof(MeshMaterialNames)) as MeshMaterialNames;
            SelectedMesh = Selection.activeObject as GameObject;
            activePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            meshName = activePath.Substring(activePath.LastIndexOf('/') + 1);
            if(meshName.IndexOf('.') > 0) meshName = meshName.Substring(0, meshName.IndexOf('.'));
        }

        private void RenderImportDirectoryFields()
        {
            string label = String.IsNullOrWhiteSpace(importDirectory) ? "Select Material Directory" : $"{importDirectory}";
            if (GUILayout.Button(label)) SelectMatieralDirectory();

            if (GUILayout.Button("Use Local Materials")) SelectMatieralDirectory(true);
        }

        private void RenderSelectionFields()
        {
            shader = EditorGUILayout.ObjectField(
                   "Select Shader",
                   shader,
                   typeof(Shader),
                   false) as Shader;

            if(!materialNameMap) GUI.color = Color.red;

            materialNameMap = EditorGUILayout.ObjectField(
                   "Select Skin Order source",
                   materialNameMap,
                   typeof(MeshMaterialNames),
                   false) as MeshMaterialNames;
            
            GUI.color = Color.white;
        }

        private List<string> GetMaterialNames(GameObject mesh)
        {
            foreach (var item in materialNameMap.MaterialNameMap)
            {
                if (mesh == item.Mesh)
                    return item.MaterialNames;
            }

            return null;
        }

        private string OpenCustomDirectory()
        {
            var directory = EditorUtility.OpenFolderPanel("Select Material Directory", importDirectory, "Materials");
            if (!Directory.Exists(directory)) return null;

            importDirectory = directory;
            return directory;
        }

        private void SelectMatieralDirectory(bool useDefaultDir = false)
        {
            if(!materialNameMap)
            {
                logWidget.LogError("Need to specify Material Name Map!!");
                return;
            }
            logWidget.Reset();

            AssetDatabase.StartAssetEditing();
            materialNames = GetMaterialNames(SelectedMesh);
            if (materialNames.Count <= 0)
            {
                AssetDatabase.StopAssetEditing();
                logWidget.LogError("No Asset with material reference selected");
                return;
            }
            var defaultDir = activePath.Substring(0, activePath.LastIndexOf('/'));

            var directory = useDefaultDir ? defaultDir + "/Materials" : OpenCustomDirectory();
            if (directory == null) return;

            if (!Directory.Exists(directory))
            {
                AssetDatabase.StopAssetEditing();
                logWidget.LogError($"Cannot find Materials folder in path {directory}");
                return;
            }
            var materialDirectories = useDefaultDir ?
                AssetDatabase.GetSubFolders(directory) : Directory.GetDirectories(directory);

            GenerateMaterial(materialDirectories, useDefaultDir);

            AssetDatabase.StopAssetEditing();
        }

        private void GenerateMaterial(string[] materialDirectories, bool useDefaultDir)
        {
            List<FileInfo> currentMaterialTextures = new();
            int currentMatIndex = 1;
            string matFolderPath = "";

            foreach (var materialDir in materialDirectories)
            {
                List<string> matarialPaths = new();

                currentMatIndex++;
                //Get reference to all texture files for material
                var currentTexDir = materialDir + "/Textures";
                if (!Directory.Exists(currentTexDir))
                {
                    logWidget.LogError($"Cannot find Textures folder in path {currentTexDir}");
                    continue;
                }

                DirectoryInfo materialFolder = new DirectoryInfo(currentTexDir);
                FileInfo[] textureFiles = materialFolder.GetFiles();

                foreach (string mName in materialNames)
                {
                    string matName = mName;
                    var mat = new Material(shader);
                    currentMaterialTextures.Clear();
                    //Extract material textures
                    foreach (var textureFile in textureFiles)
                    {
                        var names = matName.Split(',');
                        foreach (var name in names)
                        {
                            if (textureFile.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase) && !textureFile.Name.Contains(".meta"))
                            {
                                currentMaterialTextures.Add(textureFile);

                                if (names.Length > 0 && materialNames.IndexOf(matName) >= 0)
                                    matName = name;
                            }
                        }
                    }

                    string materialPath = materialDir + matName;
                    //Populate Material
                    foreach (var texName in currentMaterialTextures)
                    {
                        var assetTexPath = texName.FullName.Substring(texName.FullName.IndexOf('\\', 3) + 1).Replace('\\', '/');
                        var tex = AssetDatabase.LoadAssetAtPath(assetTexPath, typeof(Texture2D)) as Texture2D;

                        if (tex.name.EndsWith("BaseColor"))
                            mat.SetTexture("_BaseMap", tex);
                        else if (tex.name.EndsWith("Emissive"))
                            mat.SetTexture("_EmissionMap", tex);
                        else if (tex.name.EndsWith("Normal"))
                            mat.SetTexture("_BumpMap", tex);
                        else
                            mat.SetTexture("_OcclusionMap", tex);
                    }

                    //Save Material
                    matFolderPath = useDefaultDir ? materialDir : materialDir.Substring(materialDir.IndexOf('/', 3) + 1).Replace("\\", "/");
                    var matPath = matFolderPath + "/" + matName + ".mat";
                    matarialPaths.Add(matPath);

                    if (AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) != null)
                    {
                        string message = useDefaultDir ?
                            matPath.Substring(materialDir.IndexOf('/', 4)).Replace("\\", "/") :
                            matPath.Substring(matPath.IndexOf('/'));

                        logWidget.LogWarning("Can't create material, it already exists: " + message);
                        //Move to next material
                        continue;
                    }

                    try
                    {
                        AssetDatabase.CreateAsset(mat, matPath);

                        logWidget.LogNormal($"Material Created {mat.name}");
                        AssetDatabase.SaveAssets();
                    }
                    catch (Exception e)
                    {
                        logWidget.LogError(e.ToString());
                        AssetDatabase.StopAssetEditing();
                        return;
                    }
                }

                var skinName = matFolderPath.Substring(matFolderPath.LastIndexOf('/') + 1);
                if(skinName.LastIndexOf('_') > 0 ) skinName = skinName.Substring(0, skinName.LastIndexOf('_'));
                var skinPath = matFolderPath + '/' + skinName + ".asset";

                //Create Skin object
                if (AssetDatabase.LoadAssetAtPath(skinPath, typeof(Skin)) != null)
                {
                    logWidget.LogWarning("Can't create Skin, it already exists: " + skinPath.Substring(skinPath.IndexOf('/')));
                    logWidget.LogNormal("----");
                    //Move to next material
                    continue;
                }
                GenerateSkinObject(matarialPaths, skinPath, skinName);

                logWidget.LogNormal("----");
            }
        }

        private void GenerateSkinObject(List<string> materialPaths, string skinPath, string skinName)
        {
            int numberOfMaterials = materialPaths.Count;
            Material[] materials = new Material[numberOfMaterials];

            for (int i = 0; i < numberOfMaterials; i++)
            {
                materials[i] = AssetDatabase.LoadAssetAtPath(materialPaths[i], typeof(Material)) as Material;
            }

            Skin newSkin = ScriptableObject.CreateInstance<Skin>();
            newSkin.mats = materials;

            //Save asset
            try
            {
                AssetDatabase.CreateAsset(newSkin, skinPath);
                AssetDatabase.SaveAssets();
                logWidget.LogNormal($"skin Created {newSkin.name}");
            }
            catch (Exception e)
            {
                logWidget.LogError(e.ToString());
                AssetDatabase.StopAssetEditing();
                return;
            }

            RefreshAddressables(skinPath);
        }

        private void RefreshAddressables(string skinAssetPath)
        {
            string skinAddress = skinAssetPath.Substring(skinAssetPath.LastIndexOf('/') + 1);
            string skinName = skinAddress.Substring(0, skinAddress.IndexOf('.'));
            string groupName = meshName + '_' + skinName;
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            var schemaOneLocation = "Assets/AddressableAssetsData/AssetGroups/Schemas/ConnectivityGraphs_BundledAssetGroupSchema.asset";
            var SchemTwoLocation = "Assets/AddressableAssetsData/AssetGroups/Schemas/ConnectivityGraphs_ContentUpdateGroupSchema.asset";

            List<AddressableAssetGroupSchema> schemas = new();

            schemas.Add(AssetDatabase.LoadAssetAtPath(schemaOneLocation, typeof(AddressableAssetGroupSchema)) as AddressableAssetGroupSchema);
            schemas.Add(AssetDatabase.LoadAssetAtPath(SchemTwoLocation, typeof(AddressableAssetGroupSchema)) as AddressableAssetGroupSchema);

            var group = settings.CreateGroup(groupName, false, false, false, schemas);
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { skinAssetPath.Substring(0, skinAssetPath.LastIndexOf('/')+1) });

            var entriesAdded = new List<AddressableAssetEntry>();
            for (int i = 0; i < guids.Length; i++)
            {
                var entry = settings.CreateOrMoveEntry(guids[i], group, readOnly: false, postEvent: false);
                entry.address = skinAssetPath.Substring(skinAssetPath.LastIndexOf('/') + 1);

                entriesAdded.Add(entry);
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);


            logWidget.LogNormal($"Addressable {skinName} created in group {groupName}");
        }
    }
}
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

namespace SupremacyHangar.Editor
{
    public class CreateMaterialsForTextures : EditorWindow
    {
        public Shader shader;
        private string importDirectory;

        List<string> materialNames = new();


        [MenuItem("Assets/ImportMaterialsForMech")]
        public static void Spawn()
        {
            var window = GetWindow<CreateMaterialsForTextures>();
            window.titleContent = new GUIContent("Static Data Importer");
            window.Show();
        }
        public void OnGUI()
        {
            RenderImportDirectoryFields();
            RenderSelectShader();
        }

        void OnEnable()
        {
            shader = Shader.Find("Supremacy/MechShader");
        }

        private void RenderImportDirectoryFields()
        {
            string label = String.IsNullOrWhiteSpace(importDirectory) ? "Select Material Directory" : $"{importDirectory}";
            if (GUILayout.Button(label)) SelectMatieralDirectory();

            if (GUILayout.Button("Use defualt directory (Local Materials)")) SelectMatieralDirectory(true);
        }

        private void RenderSelectShader()
        {
            shader = EditorGUILayout.ObjectField(
                   "Select Shader",
                   shader,
                   typeof(Shader),
                   false) as Shader;
        }

        private List<string> GetMaterialNames(string assetPath, string destinationPath)
        {
            IEnumerable<Object> enumerable = from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                             where x.GetType() == typeof(Material)
                                             select x;

            List<string> materialNames = new();
            foreach (Object item in enumerable)
            {
                materialNames.Add(item.name);
            }

            return materialNames;
        }

        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private string OpenCustomDirectory()
        {
            var directory = EditorUtility.OpenFolderPanel("Select Material Directory", importDirectory, "test");
            if (!Directory.Exists(directory)) return null;

            importDirectory = directory;
            return directory;
        }

        private void SelectMatieralDirectory(bool useDefaultDir = false)
        {
            AssetDatabase.StartAssetEditing();
            var activePath = AssetDatabase.GetAssetPath(Selection.activeObject);

            materialNames = GetMaterialNames(activePath, "Assets");
            if (materialNames.Count <= 0)
            {
                AssetDatabase.StopAssetEditing();
                Debug.LogError("No Asset with materials selected");
                return;
            }
            var defaultDir = activePath.Substring(0, activePath.LastIndexOf('/'));

            var directory = useDefaultDir ? defaultDir + "/Materials" : OpenCustomDirectory();
            if (directory == null) return;

            if(!Directory.Exists(directory))
            {
                AssetDatabase.StopAssetEditing();
                Debug.LogError($"Cannot find Materials folder in path {directory}");
                return;
            }
            var materialDirectories = useDefaultDir ?
                AssetDatabase.GetSubFolders(directory) : Directory.GetDirectories(directory);

            GenerateMaterial(materialDirectories, useDefaultDir);
        }

        private void GenerateMaterial(string[] materialDirectories, bool useDefaultDir)
        {
            List<FileInfo> currentMaterialTextures = new();
            foreach (var dir in materialDirectories)
            {
                //Get reference to all texture files for material
                var currentTexDir = dir + "/Textures";
                if (!Directory.Exists(currentTexDir))
                {
                    AssetDatabase.StopAssetEditing();
                    Debug.LogError($"Cannot find Textures folder in path {currentTexDir}");
                    return;
                }

                DirectoryInfo materialFolder = new DirectoryInfo(currentTexDir);
                FileInfo[] textureFiles = materialFolder.GetFiles();

                foreach (string matName in materialNames)
                {
                    string materialPath = dir + matName;

                    var mat = new Material(shader);
                    currentMaterialTextures.Clear();
                    //Extract material textures
                    foreach (var textureFile in textureFiles)
                    {
                        if (textureFile.Name.Contains(matName) && !textureFile.Name.Contains(".meta"))
                            currentMaterialTextures.Add(textureFile);
                    }

                    //Populate Material
                    foreach (var texName in currentMaterialTextures)
                    {
                        var assetTexPath = texName.FullName.Substring(GetNthIndex(texName.FullName, '\\', 3) + 1).Replace('\\', '/');
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
                    string path = useDefaultDir ? dir : dir.Substring(GetNthIndex(dir, '/', 3) + 1).Replace("\\", "/");
                    path += "/" + matName + ".mat";

                    if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
                    {
                        Debug.LogWarning("Can't create material, it already exists: " + path);
                        //Move to next material
                        continue;
                    }

                    try
                    {
                        AssetDatabase.CreateAsset(mat, path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        AssetDatabase.StopAssetEditing();
                        return;
                    }
                }
            }
        }
    }
}
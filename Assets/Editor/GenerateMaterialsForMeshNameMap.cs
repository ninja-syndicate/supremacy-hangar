using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SupremacyHangar.Editor
{
    public class GenerateMaterialsForMeshNameMap : GenerateMaterialsForMesh
    {
        [MenuItem("Assets/Supremacy/MeshNameMapMaterialGenerator", true)]
        public static new bool SpawnValidate()
        {
            var selection = Selection.activeObject as MeshMaterialNames;
            return selection != null;
        }

        [MenuItem("Assets/Supremacy/MeshNameMapMaterialGenerator")]
        public static new void Spawn()
        {
            var window = GetWindow<GenerateMaterialsForMeshNameMap>();
            window.titleContent = new GUIContent("Mesh Map Material Generator");
            window.Show();
        }

        void OnEnable()
        {
            shader = Shader.Find("Supremacy/MechShader");
            materialNameMap = Selection.activeObject as MeshMaterialNames;
        }

        public new void OnGUI()
        {
            RenderGenerateButton();
            RenderSelectionFields();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            logWidget.Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            EditorGUILayout.EndScrollView();
        }

        private void RenderGenerateButton()
        {
            if(GUILayout.Button("Generate All Materials"))
            {
                logWidget.Reset();
                GenerateAllMeshMaterials();
            }
        }

        private void GenerateAllMeshMaterials()
        {
            foreach (var item in materialNameMap.MaterialNameMap)
            {
                SelectedMesh = item.Mesh;
                activePath = AssetDatabase.GetAssetPath(SelectedMesh);
                meshName = String.Join('/', activePath.Split('/').Skip(3));
                meshName = meshName.Substring(0, meshName.IndexOf('.'));
                SelectMatieralDirectory(true);
                logWidget.LogNormal("\n--------New Mesh--------\n");
            }

            AssetDatabase.StopAssetEditing();
        }
    }
}

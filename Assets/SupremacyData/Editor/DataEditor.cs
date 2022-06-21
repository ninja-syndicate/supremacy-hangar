using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    [CustomEditor(typeof(Runtime.Data))]
    public class DataEditor : UnityEditor.Editor
    {
        private SerializedProperty factionsProperty;
        private SerializedProperty battleAbilitiesProperty;
        private SerializedProperty gameAbilitiesProperty;
        private SerializedProperty mechModelsProperty;
        private SerializedProperty mechSkinsProperty;
        private SerializedProperty mysteryCratesProperty;

        private Runtime.Data targetData;
        
        public void OnEnable()
        {
            targetData = serializedObject.targetObject as Runtime.Data;
            factionsProperty = serializedObject.FindProperty("factions");
            battleAbilitiesProperty = serializedObject.FindProperty("battleAbilities");
            gameAbilitiesProperty = serializedObject.FindProperty("gameAbilities");
            mechModelsProperty = serializedObject.FindProperty("mechModels");
            mechSkinsProperty = serializedObject.FindProperty("mechSkins");
            mysteryCratesProperty = serializedObject.FindProperty("mysteryCrates");
        }

        public override void OnInspectorGUI()
        {
            if (targetData == null)
            {
                EditorGUILayout.LabelField("Invalid Data!");
                return;
            }
            
            serializedObject.Update();
            RenderDefaultEditor();
        }

        private void RenderDefaultEditor()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(factionsProperty);
            if (GUILayout.Button("Delete Factions not in above list")) DeleteUnused(targetData.factions);
            
            EditorGUILayout.PropertyField(battleAbilitiesProperty);
            if (GUILayout.Button("Delete Battle Abilities not in above list")) DeleteUnused(targetData.battleAbilities);

            EditorGUILayout.PropertyField(gameAbilitiesProperty);
            if (GUILayout.Button("Delete Game Abilities not in above list")) DeleteUnused(targetData.gameAbilities);

            EditorGUILayout.PropertyField(mechModelsProperty);
            if (GUILayout.Button("Delete Mech Models not in above list")) DeleteUnused(targetData.mechModels);

            EditorGUILayout.PropertyField(mechSkinsProperty);
            if (GUILayout.Button("Delete Mech Skins not in above list")) DeleteUnused(targetData.mechSkins);

            EditorGUILayout.PropertyField(mysteryCratesProperty); 
            if (GUILayout.Button("Delete Mystery Crates not in above list")) DeleteUnused(targetData.mysteryCrates);

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        private void DeleteUnused<T>(List<T> records) where T : Runtime.BaseRecord
        {
            AssetDatabase.StartAssetEditing();
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(targetData));

            foreach (var subAsset in subAssets)
            {
                if (subAsset is not T typedAsset) continue;
                if (records.Contains(typedAsset)) continue;
                AssetDatabase.RemoveObjectFromAsset(typedAsset);
                EditorUtility.SetDirty(targetData);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssetIfDirty(targetData);
        }
    }
}
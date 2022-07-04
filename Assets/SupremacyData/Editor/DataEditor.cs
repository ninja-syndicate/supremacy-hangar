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
        private SerializedProperty mysteryCratesProperty;
        private SerializedProperty mechModelsProperty;
        private SerializedProperty mechSkinsProperty;

        private Runtime.Data targetData;
        private bool targetDataSet;
        private LogWidget logger;
        
        public void OnEnable()
        {
            if (logger == null)
            {
                logger = new LogWidget();
            }
            else
            {
                logger.Reset();
            }

            targetData = serializedObject.targetObject as Runtime.Data;
            targetDataSet = targetData != null;
            factionsProperty = serializedObject.FindProperty("factions");
            battleAbilitiesProperty = serializedObject.FindProperty("battleAbilities");
            gameAbilitiesProperty = serializedObject.FindProperty("gameAbilities");
            mysteryCratesProperty = serializedObject.FindProperty("mysteryCrates");
            mechModelsProperty = serializedObject.FindProperty("mechModels");
            mechSkinsProperty = serializedObject.FindProperty("mechSkins");
        }

        public override void OnInspectorGUI()
        {
            if (!targetDataSet)
            {
                EditorGUILayout.LabelField("Invalid Data!");
                return;
            }
            
            serializedObject.Update();
            RenderDefaultEditor();
            EditorGUILayout.Space(10f);
            logger.Render(GUILayout.Height(100));
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

            EditorGUILayout.PropertyField(mysteryCratesProperty); 
            if (GUILayout.Button("Delete Mystery Crates not in above list")) DeleteUnused(targetData.mysteryCrates);

            EditorGUILayout.PropertyField(mechModelsProperty);
            if (GUILayout.Button("Delete Mech Models not in above list")) DeleteUnused(targetData.mechModels);

            EditorGUILayout.PropertyField(mechSkinsProperty);
            if (GUILayout.Button("Delete Mech Skins not in above list")) DeleteUnused(targetData.mechSkins);
            
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        private void DeleteUnused<T>(List<T> records) where T : Runtime.BaseRecord
        {
            AssetDatabase.StartAssetEditing();
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(targetData));
            logger.LogNormal($"Removing unused {typeof(T)}");
            bool removedSomething = false;
            
            foreach (var subAsset in subAssets)
            {
                if (subAsset is not T typedAsset) continue;
                if (records.Contains(typedAsset)) continue;
                removedSomething = true;
                logger.LogNormal($"Removing unused {typedAsset.name}");
                AssetDatabase.RemoveObjectFromAsset(typedAsset);
                EditorUtility.SetDirty(targetData);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssetIfDirty(targetData);

            if (!removedSomething) logger.LogNormal("Did not remove anything");
        }
    }
}
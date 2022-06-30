using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Editor.ContentLoader
{
    public class MapOption
    {
        public string name;
        public BaseRecord data;
        public AssetReference mech;
        public AssetReferenceSkin skin;
    }

    public class AssetLoaderWindow : EditorWindow
    {
        private static AssetMappings allMaps;
        public void AllMaps(ref AssetMappings map)
        {
            allMaps = map;
        }
        public static AddressablesManager myAddressablesManager;

        public void MyAddressablesManager(ref AddressablesManager addressablesManager)
        {
            myAddressablesManager = addressablesManager;
        }

        private int index = 0;
        private int spacerAmount = 20;
        Vector2 scrollPosition = Vector2.zero;
        Color selectedColour = new Color(0, 0.7f, 1f);

        private bool optionsSet = false;
        private SerializedObject _serializedObject;
        List<MapOption> mapOptions = new();
        private string searchValue;

        [MenuItem("Supremacy/AssetLoader")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(AssetLoaderWindow));
            window.titleContent = new GUIContent("Asset Spawner");
            window.Show();
        }

        void OnGUI()
        {
            if (!optionsSet) GetAssetsAndNames();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            allMaps = EditorGUILayout.ObjectField(
                "Asset Mapping Object",
                allMaps,
                typeof(AssetMappings),
                false) as AssetMappings;
            RenderNavigationSection();
            RenderSearchFields();

            if (mapOptions.Count > 0) RenderSelectedInformation(mapOptions[index]);
            RenderButtonList();

            EditorGUILayout.EndScrollView();
        }

        private void RenderSelectedInformation(MapOption item)
        {
            switch(item.data)
            {
                case MechSkin skin:
                    RenderSelectedSkinInformation(item);
                    break;
                case MysteryCrate crate:
                    RenderSelectedCrateInformation(item);
                    break;
                default:
                    break;
            }
        }

        private void RenderSearchFields()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("List Search");
            searchValue = EditorGUILayout.DelayedTextField(searchValue);
            EditorGUILayout.EndHorizontal();
        }

        private void GetAssetsAndNames()
        {
            if (!allMaps) return;

            optionsSet = true;
            _serializedObject = new SerializedObject(allMaps);
            foreach (var item in allMaps.MechSkinAssetByGuid.Values)
            {
                if (!allMaps.MechChassisPrefabByGuid.ContainsKey(item.DataMechSkin.MechModel.Id))
                {
                    Debug.Log($"Mech not found in Asset Map {item.DataMechSkin.MechModel.HumanName}");
                    continue;
                }
                mapOptions.Add(new MapOption()
                {
                    data = item.DataMechSkin,
                    mech = allMaps.MechChassisPrefabByGuid[item.DataMechSkin.MechModel.Id].MechReference,
                    skin = item.SkinReference,
                    name = item.DataMechSkin.MechModel.HumanName + " - " + item.DataMechSkin.HumanName
                });
            }

            foreach (var item in allMaps.MysteryCrateAssetByGuid.Values)
            {
                mapOptions.Add(new MapOption()
                {
                    data = item.DataMysteryCrate,
                    mech = item.MysteryCrateReference,
                    name = item.DataMysteryCrate.HumanName
                });
            }
        }

        private void RenderButtonList()
        {   
            GUILayout.Space(spacerAmount);
            
            int counter = 0;
            foreach (var item in mapOptions)
            {
                if (counter == index) GUI.backgroundColor = selectedColour;

                counter++;
                if (searchValue != null && !item.name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase)) 
                    continue;
                                
                if (GUILayout.Button(item.name))
                {
                    index = counter - 1;
                    SetAndSpawnAsset();
                }

                GUI.backgroundColor = Color.white;
            }
        }

        private void RenderSelectedSkinInformation(MapOption item)
        {
            GUILayout.Space(spacerAmount);

            var selectedProperty = _serializedObject.FindProperty("mechSkins").GetArrayElementAtIndex(index);
            var skinReference = selectedProperty.FindPropertyRelative("skinReference");
            var dataReference = selectedProperty.FindPropertyRelative("dataMechSkin");

            EditorGUILayout.PropertyField(skinReference);
            EditorGUILayout.PropertyField(dataReference);

            //update listed item value
            var d = dataReference.objectReferenceValue as MechSkin;
            if(!allMaps.MechChassisPrefabByGuid.ContainsKey(d.MechModel.Id))
            {
                Debug.LogError("Skin Data reference is not in Asset Mappings");
                return;
            }
            item.mech = allMaps.MechChassisPrefabByGuid[d.MechModel.Id].MechReference;
            item.skin = allMaps.MechSkinAssetByGuid[d.Id].SkinReference;
            item.name = d.MechModel.HumanName + " - " + d.HumanName;

            EditorUtility.SetDirty(allMaps);

            _serializedObject.ApplyModifiedProperties();
        }

        private void RenderSelectedCrateInformation(MapOption item)
        {
            GUILayout.Space(spacerAmount);
            int crateIndex = mapOptions.Count - index - 1;

            var selectedProperty = _serializedObject.FindProperty("mysteryCrates").GetArrayElementAtIndex(crateIndex);
            var assetReference = selectedProperty.FindPropertyRelative("mysteryCrateReference");
            var dataReference = selectedProperty.FindPropertyRelative("dataMysteryCrate");

            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(dataReference);

            //update listed item value
            var d = dataReference.objectReferenceValue as MysteryCrate;
            if (!allMaps.MechChassisPrefabByGuid.ContainsKey(d.Id))
            {
                Debug.LogError("Mystery Crate Data reference is not in Asset Mappings");
                return;
            }
            item.mech = allMaps.MysteryCrateAssetByGuid[d.Id].MysteryCrateReference;
            item.skin = null;
            item.name = d.HumanName;

            EditorUtility.SetDirty(allMaps);

            _serializedObject.ApplyModifiedProperties();
        }

        private void RenderNavigationSection()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Previous", GUILayout.Height(20), GUILayout.Width(100)))
            {
                if(index > 0) 
                    index--;
                else
                    index = mapOptions.Count - 1;

                SetAndSpawnAsset();
            }

            if (GUILayout.Button("Next", GUILayout.Height(20), GUILayout.Width(100)))
            {
                if(index < mapOptions.Count - 1) 
                    index++;
                else
                    index = 0;

                SetAndSpawnAsset();
            }

            GUILayout.Label("Current index: " + index);

            if (GUILayout.Button("Spawn Current", GUILayout.Height(20), GUILayout.Width(100)))
            {
                SetAndSpawnAsset();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SetAndSpawnAsset()
        {
            myAddressablesManager.TargetMech = mapOptions[index].mech;
            myAddressablesManager.TargetSkin = mapOptions[index].skin;
            myAddressablesManager.QuickSpawn();
        }
    }
}

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
        private string searchValue = "";

        private int crateCount = 0;

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
            prevSearch = searchValue;
            searchValue = EditorGUILayout.DelayedTextField(searchValue);
            if (searchValue != prevSearch)
                searchResults.Clear();

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
                crateCount++;
            }
        }

        private void RenderButtonList()
        {   
            GUILayout.Space(spacerAmount);
            
            int counter = 0;
            foreach (var item in mapOptions)
            {
                if ((searchValue == "" && counter == index) || (searchResults.Count > 0 && searchResults[searchIndex] == counter)) GUI.backgroundColor = selectedColour;

                counter++;
                if (searchValue != "" && !item.name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase)) 
                    continue;
                                
                if (GUILayout.Button(item.name))
                {
                    index = counter - 1;
                    searchIndex = searchResults.Count > 0 ? SearchResultIndex(counter - 1) : 0;
                    SetAndSpawnAsset();
                }
                                
                if (searchValue != "" && searchValue != prevSearch)
                    searchResults.Add(counter - 1);

                if (searchValue != "" && prevSearch != searchValue)
                {
                    index = searchResults[0];
                }

                GUI.backgroundColor = Color.white;
            }
        }
        private string prevSearch;
        List<int> searchResults = new();
        private int searchIndex = 0;

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
            int crateIndex = crateCount - (mapOptions.Count - index);

            var selectedProperty = _serializedObject.FindProperty("mysteryCrates").GetArrayElementAtIndex(crateIndex);
            var assetReference = selectedProperty.FindPropertyRelative("mysteryCrateReference");
            var dataReference = selectedProperty.FindPropertyRelative("dataMysteryCrate");

            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(dataReference);

            //update listed item value
            var d = dataReference.objectReferenceValue as MysteryCrate;
            if (!allMaps.MysteryCrateAssetByGuid.ContainsKey(d.Id))
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
                if (searchValue == "" && index > 0)
                    index--;
                else if (searchValue != "" && searchIndex > 0)
                    searchIndex--;
                else
                {
                    index = mapOptions.Count - 1;
                    if(searchResults.Count > 0) searchIndex = searchResults.Count - 1;
                }

                SetAndSpawnAsset();
            }

            if (GUILayout.Button("Next", GUILayout.Height(20), GUILayout.Width(100)))
            {
                if (searchValue == "" && index < mapOptions.Count - 1)
                    index++;
                else if (searchValue != "" && searchIndex < searchResults.Count - 1)
                    searchIndex++;
                else
                {
                    index = 0;
                    searchIndex = 0;
                }

                SetAndSpawnAsset();
            }

            var t = searchResults.Count > 0 ? searchResults[searchIndex] : index;
            GUILayout.Label($"Current index: {t}");

            if (GUILayout.Button("Spawn Current", GUILayout.Height(20), GUILayout.Width(100)))
            {
                SetAndSpawnAsset();
            }

            EditorGUILayout.EndHorizontal();
        }

        private int SearchResultIndex(int targetIndex)
        {
            return searchResults[searchResults.IndexOf(targetIndex)];
        }

        private void SetAndSpawnAsset()
        {
            myAddressablesManager.TargetMech = searchResults.Count > 0 ? mapOptions[searchResults[searchIndex]].mech : mapOptions[index].mech;
            myAddressablesManager.TargetSkin = searchResults.Count > 0 ? mapOptions[searchResults[searchIndex]].skin : mapOptions[index].skin;
            myAddressablesManager.QuickSpawn();
        }
    }
}

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
        public AssetMappings AllMaps;
        public AddressablesManager MyAddressablesManager;

        private int index = 0;
        private int spacerAmount = 20;
        Vector2 scrollPosition = Vector2.zero;
        Color selectedColour = new Color(0, 0.7f, 1f);

        private bool optionsSet = false;
        private SerializedObject _serializedObject;
        List<MapOption> mapOptions = new();

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
        private string dataSearch;
        private string assetSearch;
        private void RenderSearchFields()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Search by Data Ref");
            dataSearch = EditorGUILayout.TextField(dataSearch, GUILayout.Height(20), GUILayout.Width(100));
            GUILayout.Label("Search by Asset Ref");
            assetSearch = EditorGUILayout.TextField(assetSearch, GUILayout.Height(20), GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        private void GetAssetsAndNames()
        {
            optionsSet = true;
            _serializedObject = new SerializedObject(AllMaps);
            foreach (var item in AllMaps.MechSkinAssetByGuid.Values)
            {
                mapOptions.Add(new MapOption()
                {
                    data = item.DataMechSkin,
                    mech = AllMaps.MechChassisPrefabByGuid[item.DataMechSkin.MechModel.Id].MechReference,
                    skin = item.SkinReference,
                    name = item.DataMechSkin.MechModel.HumanName + " - " + item.DataMechSkin.HumanName
                });
            }

            foreach (var item in AllMaps.MysteryCrateAssetByGuid.Values)
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
                if ((dataSearch != null && !item.name.ToLower().Contains(dataSearch)) || (assetSearch != null && !item.name.ToLower().Contains(assetSearch))) continue;

                if (GUILayout.Button(item.name))
                {
                    MyAddressablesManager.TargetMech = item.mech;
                    MyAddressablesManager.TargetSkin = item.skin;
                    MyAddressablesManager.QuickSpawn();
                    index = counter - 1;
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
            item.mech = AllMaps.MechChassisPrefabByGuid[d.MechModel.Id].MechReference;
            item.skin = AllMaps.MechSkinAssetByGuid[d.Id].SkinReference;
            item.name = d.MechModel.HumanName + " - " + d.HumanName;

            EditorUtility.SetDirty(AllMaps);

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
            item.mech = AllMaps.MysteryCrateAssetByGuid[d.Id].MysteryCrateReference;
            item.skin = null;
            item.name = d.HumanName;

            EditorUtility.SetDirty(AllMaps);

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

                MyAddressablesManager.TargetMech = mapOptions[index].mech;
                MyAddressablesManager.TargetSkin = mapOptions[index].skin;
                MyAddressablesManager.QuickSpawn();
            }

            if (GUILayout.Button("Next", GUILayout.Height(20), GUILayout.Width(100)))
            {
                if(index < mapOptions.Count - 1) 
                    index++;
                else
                    index = 0;

                MyAddressablesManager.TargetMech = mapOptions[index].mech;
                MyAddressablesManager.TargetSkin = mapOptions[index].skin;
                MyAddressablesManager.QuickSpawn();
            }

            GUILayout.Label("Current index: " + index);

            if (GUILayout.Button("Spawn Current", GUILayout.Height(20), GUILayout.Width(100)))
            {
                MyAddressablesManager.TargetMech = mapOptions[index].mech;
                MyAddressablesManager.TargetSkin = mapOptions[index].skin;
                MyAddressablesManager.QuickSpawn();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}

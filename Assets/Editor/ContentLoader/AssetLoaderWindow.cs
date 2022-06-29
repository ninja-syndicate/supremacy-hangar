using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SupremacyHangar.Editor.ContentLoader
{
    public class MapOption
    {
        public string name;
        public AssetReference mech;
        public AssetReferenceSkin skin;
    }
    public class AssetLoaderWindow : EditorWindow
    {
        public AssetMappings AllMaps;
        public AddressablesManager MyAddressablesManager;

        private bool optionsSet = false;
        List<MapOption> mapOptions = new();

        [MenuItem("Supremacy/AssetLoader")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(AssetLoaderWindow));
            window.titleContent = new GUIContent("Asset Spawner");
            window.Show();
        }

        Vector2 scrollPosition = Vector2.zero;
        void OnGUI()
        {
            if (!optionsSet) GetAssetsAndNames();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // create a button for each image loaded in, 4 buttons in width
            // calls the handler when a new image is selected.
            RenderButtonList();


            EditorGUILayout.EndScrollView();
        }

        private void RenderAssetMapObject()
        {
            AllMaps = EditorGUILayout.ObjectField(
                "Asset Mapping Object",
                AllMaps, 
                typeof(AssetMappings), 
                false) as AssetMappings;
            
        }

        private void GetAssetsAndNames()
        {
            optionsSet = true;
            foreach (var item in AllMaps.MechSkinAssetByGuid.Values)
            {
                mapOptions.Add(new MapOption()
                {
                    mech = AllMaps.MechChassisPrefabByGuid[item.DataMechSkin.MechModel.Id].MechReference,
                    skin = item.SkinReference,
                    name = item.DataMechSkin.MechModel.HumanName + " - " + item.DataMechSkin.HumanName
                });
            }

            foreach (var item in AllMaps.MysteryCrateAssetByGuid.Values)
            {
                mapOptions.Add(new MapOption()
                {
                    mech = item.MysteryCrateReference,
                    name = item.DataMysteryCrate.HumanName
                });
            }
        }

        private void RenderButtonList()
        {
            foreach (var item in mapOptions)
            {
                //EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(item.name))
                {
                    MyAddressablesManager.TargetMech = item.mech;
                    MyAddressablesManager.TargetSkin = item.skin;
                    MyAddressablesManager.QuickSpawn();
                }

                //EditorGUILayout.EndHorizontal();
            }
        }
    }
}

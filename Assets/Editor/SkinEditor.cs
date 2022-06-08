using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SupremacyHangar.Scriptable;

namespace SupremacyHangar
{
    public class SkinEditor
    {
        [MenuItem("Assets/Create/Supremacy/Skins/Zaibatsu")]
        public static void CreateZaibatsu()
        {
            Skin asset = ScriptableObject.CreateInstance<Skin>();
            asset.mats = new Material[6];

            AssetDatabase.CreateAsset(asset, "Assets/NewZaibatsuSkin.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        [MenuItem("Assets/Create/Supremacy/Skins/Red Mountain")]
        public static void CreateRedMountain()
        {
            Skin asset = ScriptableObject.CreateInstance<Skin>();
            asset.mats = new Material[14];

            AssetDatabase.CreateAsset(asset, "Assets/NewRedMountainSkin.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        [MenuItem("Assets/Create/Supremacy/Skins/BostonCybernetics")]
        public static void CreateBostonCybernetics()
        {
            Skin asset = ScriptableObject.CreateInstance<Skin>();
            asset.mats = new Material[4];

            AssetDatabase.CreateAsset(asset, "Assets/NewBostonCybernetics.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}
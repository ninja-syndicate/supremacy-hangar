using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.Silo;

namespace SupremacyHangar.Editor
{
    public class PlatformStopsEditor
    {
        [MenuItem("Assets/Create/Supremacy/Platform/StopList")]
        public static void CreatePlatformStopList()
        {
            PlatformStops asset = ScriptableObject.CreateInstance<PlatformStops>();

            AssetDatabase.CreateAsset(asset, "Assets/NewPlatformStopList.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}
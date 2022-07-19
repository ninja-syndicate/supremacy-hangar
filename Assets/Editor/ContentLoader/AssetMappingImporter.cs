using Malee.List;
using SupremacyData.Runtime;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using static SupremacyHangar.Editor.ContentLoader.AssetMappingsEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    public class AssetMappingImporter
    {
        private static AssetMappingAssetGrabber assetGrabber = new();
        private string refLabel = null;

        public void AddMissingKeys(ReorderableList key, ListType type)
        {
            //Get Static Data
            var staticDataGuids = AssetDatabase.FindAssets("t:Data");
            var assetMapGuids = AssetDatabase.FindAssets("t:AssetMappings");

            if (staticDataGuids.Length != 1)
            {
                Debug.LogError($"Only expected one static data reference");
                return;
            }

            var dataPath = AssetDatabase.GUIDToAssetPath(staticDataGuids[0]);
            var staticData = AssetDatabase.LoadAssetAtPath(dataPath, typeof(Data)) as Data;

            bool directoryMissing = false;

            if (staticData == null)
            {
                Debug.Log("Couldn't load static data");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            int firstNewItemIndex = -1;
            foreach (var dataKey in GetStatisDataList(type, staticData))
            {
                bool matchFound = false;
                for (int i = 0; i < key.Length; i++)
                {
                    var parent = key.GetItem(i);
                    var targetKey = parent.FindPropertyRelative(assetGrabber.TypeToNameMap[type][1]);

                    if (targetKey.objectReferenceValue == dataKey)
                        matchFound = true;
                }

                if (!matchFound)
                {
                    var parent = key.AddItem();
                    if (firstNewItemIndex == -1) firstNewItemIndex = key.Length - 1;

                    var targetKey = parent.FindPropertyRelative(assetGrabber.TypeToNameMap[type][1]);
                    var assetRef = parent.FindPropertyRelative(assetGrabber.TypeToNameMap[type][2]);

                    targetKey.objectReferenceValue = dataKey;

                    parent.serializedObject.ApplyModifiedProperties();

                    string targetName = dataKey.name;
                    string targetTypeName = dataKey.name;
                    //single object name only
                    if (type == ListType.WeaponSkin || type == ListType.MechSkin)
                    {
                        targetTypeName = targetTypeName.Split('-')[1];
                        targetTypeName = targetTypeName.Split(' ')[1];
                        targetTypeName = @"" + targetTypeName;
                        targetName = targetName.Substring(targetName.LastIndexOf('-') + 2);
                    }
                    else
                    {
                        targetName = targetName.Substring(targetName.IndexOf('-') + 2);
                        if (type == ListType.Faction) targetName = targetName.Split(' ')[0];
                    }

                    if (directoryMissing) continue;
                    //asset ref only
                    string[] targetAssetGuid = new string[1];
                    switch (type)
                    {
                        case ListType.Faction:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:EnvironmentConnectivity");
                            break;
                        case ListType.MechChassis:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Prefab");
                            break;
                        case ListType.MechSkin:
                            if (!Directory.Exists("Assets/Content/Mechs"))
                            {
                                Debug.LogError("Directory does NOT exist: Assets/Content/Mechs. NO Skins set");
                                directoryMissing = true;
                                continue;
                            }
                            string mechSkinfolderPath = SearchSubDirs("Assets/Content/Mechs", targetTypeName);
                            if (mechSkinfolderPath != null)
                                targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Skin", new[] { $"{mechSkinfolderPath}" });
                            break;
                        case ListType.MysteryCrate:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Prefab");
                            break;
                        case ListType.WeaponModel:
                            var targetWeaponModel = dataKey as WeaponModel;
                            string modelBrandName = targetWeaponModel.Brand?.HumanName;
                            targetName = targetWeaponModel.Type.ToString();
                            targetAssetGuid = AssetFinder("Assets/Content/Weapons", modelBrandName, targetName, targetName);
                            break;
                        case ListType.WeaponSkin:
                            if (!Directory.Exists("Assets/Content/Weapons"))
                            {
                                Debug.LogError("Directory does NOT exist: Assets/Content/Weapons. NO Skins set");
                                directoryMissing = true;
                                continue;
                            }
                            var targetWeapon = dataKey as WeaponSkin; 
                            string skinBrandName = targetWeapon.WeaponModel.Brand?.HumanName;
                            targetAssetGuid = AssetFinder("Assets/Content/Weapons", skinBrandName, targetWeapon.Type.ToString(), targetWeapon.Type.ToString());
                            break;
                        case ListType.PowerCore:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Prefab");
                            break;
                        default:
                            Debug.LogError($"Unknown type: {type}");
                            break;
                    }

                    if (targetAssetGuid != null && targetAssetGuid.Length > 0 && targetAssetGuid[0] != null)
                    {
                        switch (type)
                        {
                            case ListType.Faction:
                                var targetFactionAsset = assetGrabber.GetFactionGraphAssetReferenceValue(assetRef, ref refLabel);
                                var newAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as EnvironmentConnectivity;
                                targetFactionAsset.SetEditorAsset(newAsset);
                                break;
                            case ListType.MechSkin:
                                var targetMechSkinAsset = assetGrabber.GetSkinAssetReferenceValue(assetRef, ref refLabel);
                                var newMechSkinAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as Skin;
                                targetMechSkinAsset.SetEditorAsset(newMechSkinAsset);
                                break;
                            case ListType.WeaponSkin:
                                var targetWeaponSkinAsset = assetGrabber.GetSkinAssetReferenceValue(assetRef, ref refLabel);
                                var newWeaponSkinAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as Skin;
                                targetWeaponSkinAsset.SetEditorAsset(newWeaponSkinAsset);
                                break;
                            case ListType.PowerCore:
                            default:
                                var targetAsset = assetGrabber.GetAssetReferenceValue(assetRef, ref refLabel);
                                var newGOAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as GameObject;
                                targetAsset.SetEditorAsset(newGOAsset);
                                break;
                        }
                    }
                    parent.serializedObject.ApplyModifiedProperties();
                    parent.serializedObject.Update();
                }
            }

            if (key.paginate) key.SetPage(firstNewItemIndex / key.pageSize);
        }

        private string[] AssetFinder(string dirPath, string brandName, string folderName, string assetName)
        {
            if (brandName != null)
            {
                string weaponModelFolder = SearchSubDirs(dirPath, brandName);

                if (weaponModelFolder != null)
                {
                    string weaponfolderPath = SearchSubDirs(weaponModelFolder, folderName);

                    if (weaponfolderPath != null)
                        return AssetDatabase.FindAssets($"{assetName} t:Prefab", new[] { $"{weaponfolderPath}" });
                    else
                    {
                        Debug.LogError($"Weapon type folder NOT found: {dirPath}/{brandName}/{assetName}. No Asset Set");
                        return null;
                    }
                }

                Debug.LogError($"Brand folder NOT found: {dirPath}/{brandName}. No Asset Set");
            }
            return null;
        }

        private string SearchSubDirs(string dir, string targetFolder)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(dir);

            foreach (string subdirectory in subdirectoryEntries)
            {
                var result = SearchSubDirs(subdirectory, targetFolder);
                if (result != null) return result;

                if (subdirectory.EndsWith(targetFolder, StringComparison.CurrentCultureIgnoreCase))
                    return subdirectory;
            }

            return null;
        }

        private IEnumerable<BaseRecord> GetStatisDataList(ListType type, Data staticData)
        {
            switch (type)
            {
                case ListType.Faction:
                    return staticData.Factions;
                case ListType.MechChassis:
                    return staticData.MechModels;
                case ListType.MechSkin:
                    return staticData.MechSkins;
                case ListType.MysteryCrate:
                    return staticData.MysteryCrates;
                case ListType.WeaponModel:
                    return staticData.WeaponModels;
                case ListType.WeaponSkin:
                    return staticData.WeaponSkins;
                case ListType.PowerCore:
                    return staticData.PowerCores;
                default:
                    Debug.LogError($"Unknown type: {type}");
                    return null;
            }
        }
    }
}

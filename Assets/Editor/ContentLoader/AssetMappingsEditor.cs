using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using Malee.List;
using SupremacyData.Editor;
using UnityEngine.AddressableAssets;
using SupremacyData.Runtime;
using UnityEditor.AddressableAssets;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using System.IO;
using System;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomEditor(typeof(Runtime.ContentLoader.AssetMappings))]
    public class AssetMappingsEditor : UnityEditor.Editor
    {
        private ReorderableList factionList;
        private ReorderableList mysteryCrateList;
        private ReorderableList mechChassisList;
        private ReorderableList mechSkinList;
        private ReorderableList weaponModelList;
        private ReorderableList weaponSkinList;
        private ReorderableList powerCoreList;

        private int selectedIndex;
        private UnityEditor.Editor _editor;
        private ReorderableList selectedList;
        enum ListType
        {
            Faction,
            MechChassis,
            MechSkin,
            MysteryCrate,
            WeaponModel,
            WeaponSkin,
            PowerCore,
        };

        class AssetMapItem
        {
            public bool ContainsError = false;
            public LogWidget Log = new();
        }

        ListType currentListType;
        Dictionary<ListType, string[]> typeToNameMap = new();
        Dictionary<string, AssetMapItem> logDictionary = new();
        Dictionary<ReorderableList, ListType> typeByList = new();

        private string refLabel = null;
        private void OnEnable()
        {
            typeToNameMap.Clear();
            logDictionary.Clear();
            typeByList.Clear();
            
            InitTypeToNameMap();
            selectedIndex = -1;

            factionList = new ReorderableList(serializedObject.FindProperty("factions"));
            typeByList.Add(factionList, ListType.Faction);
            mysteryCrateList = new ReorderableList(serializedObject.FindProperty("mysteryCrates"));
            typeByList.Add(mysteryCrateList, ListType.MysteryCrate);
            mechChassisList = new ReorderableList(serializedObject.FindProperty("mechChassis"));
            typeByList.Add(mechChassisList, ListType.MechChassis);
            mechSkinList = new ReorderableList(serializedObject.FindProperty("mechSkins"));
            typeByList.Add(mechSkinList, ListType.MechSkin);
            mechSkinList.paginate = true;
            mechSkinList.pageSize = 10;
            weaponModelList = new ReorderableList(serializedObject.FindProperty("weaponModels"));
            typeByList.Add(weaponModelList, ListType.WeaponModel);
            weaponModelList.paginate = true;
            weaponModelList.pageSize = 10;
            weaponSkinList = new ReorderableList(serializedObject.FindProperty("weaponSkins"));
            typeByList.Add(weaponSkinList, ListType.WeaponSkin);
            weaponSkinList.paginate = true;
            weaponSkinList.pageSize = 10;
            powerCoreList = new ReorderableList(serializedObject.FindProperty("powerCores"));
            typeByList.Add(powerCoreList, ListType.PowerCore);

            foreach (var listPair in typeByList)
            {
                DrawListElements(listPair.Key, listPair.Value);

                listPair.Key.drawHeaderCallback += (Rect rect, GUIContent label) =>
                {
                    GUIStyle style = new GUIStyle("button");
                    style.fixedWidth = 100;
                    rect.x += 200;
                    rect.xMax -= 220;
                    if (GUI.Button(rect, "Import Missing", style))
                    {
                        AddMissingKeys(listPair.Key, listPair.Value);
                    }
                };

                listPair.Key.onSelectCallback += (l) =>
                {
                    selectedIndex = l.Index;
                    selectedList = l;
                    currentListType = typeByList[l];
                };

                //Todo fix last selected element removal
                listPair.Key.onRemoveCallback += (ReorderableList l) =>
                {
                    l.RemoveItem(selectedIndex);
                    selectedIndex = -1;
                };
            }
        }

        private void InitTypeToNameMap()
        {
            typeToNameMap.Clear();
            //String Order
            /*
                0 - list label
                1 - data reference property name
                2 - asset reference property name
            */
            typeToNameMap.Add(ListType.Faction, new[] { "Factions", "dataFaction", "connectivityGraph" });
            typeToNameMap.Add(ListType.MysteryCrate, new[] { "Mystery Crates", "dataMysteryCrate", "mysteryCrateReference" });
            typeToNameMap.Add(ListType.MechChassis, new[] { "Mech Chassis", "dataMechModel", "mechReference" });
            typeToNameMap.Add(ListType.MechSkin, new[] { "Mech Skins", "dataMechSkin", "skinReference" });
            typeToNameMap.Add(ListType.WeaponModel, new[] { "Weapon Model", "data", "reference" });
            typeToNameMap.Add(ListType.WeaponSkin, new[] { "Weapon Skin", "data", "reference" });
            typeToNameMap.Add(ListType.PowerCore, new[] { "Power Core", "data", "reference" });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if(GUILayout.Button("Help"))
            {
                EditorWindow.GetWindow<AssetMappingsEditorHelpWindow>();
            }

            factionList.DoLayoutList();
            mysteryCrateList.DoLayoutList();
            mechChassisList.DoLayoutList();
            mechSkinList.DoLayoutList();
            weaponModelList.DoLayoutList();
            weaponSkinList.DoLayoutList();
            powerCoreList.DoLayoutList();

            ElementSelectionDisplay();
            serializedObject.ApplyModifiedProperties();
        }
        
        private ListType GetListType(string type)
        {
            switch (type)
            {
                case "FactionMapping":
                    return ListType.Faction;
                case "MechChassisMapping":
                    return ListType.MechChassis;
                case "MechSkinMapping":
                    return ListType.MechSkin;
                case "MysteryCrateMapping":
                    return ListType.MysteryCrate;
                case "WeaponModelMapping":
                    return ListType.WeaponModel;
                case "WeaponSkinMapping":
                    return ListType.WeaponSkin;
                case "PowerCoreMapping":
                    return ListType.PowerCore;
                default:
                    Debug.LogError($"Unknown type: {type}");
                    return ListType.Faction;
            }
        }

        private void DrawListElements(ReorderableList list, ListType type)
        {
            list.drawElementCallback += (Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused) =>
            {
                ListType myType = GetListType(element.type);
                
                int listSize = list.pageSize > 0 ? list.pageSize : list.Length;
                listSize = listSize > list.Length ? list.Length : listSize;

                int startIndex = listSize == list.pageSize ? listSize * (list.GetCurrentPage() - 1) : 0;
                int t = list.Length - (list.Length % listSize);
                int fullPages = t / listSize;

                if (list.GetCurrentPage() > fullPages)
                {
                    startIndex -= 2;
                    listSize = list.Length;
                }
                else
                    listSize = listSize == list.pageSize ? listSize * list.GetCurrentPage() : listSize;

                string propertyPath = element.propertyPath;
                var dataReference = element.FindPropertyRelative(typeToNameMap[myType][1]);
                var assetReference = element.FindPropertyRelative(typeToNameMap[myType][2]);

                if (!logDictionary.ContainsKey(propertyPath))
                {
                    logDictionary.Add(propertyPath, new AssetMapItem());
                }

                bool dupeKeyFound = false;
                bool dupeValueFound = false;
                bool keyIsEmpty = false;
                bool valueIsEmpty = false;

                if (dataReference == null)
                {
                    if (!dupeKeyFound && !dupeValueFound && !keyIsEmpty && !valueIsEmpty)
                        logDictionary[propertyPath].Log.Reset();

                    if (!keyIsEmpty)
                        logDictionary[propertyPath].Log.LogError($"{dataReference.displayName} property {typeToNameMap[myType][1]} could not be found");

                    keyIsEmpty = true;
                }
                else if (!dataReference.objectReferenceValue)
                {
                    if (!dupeKeyFound && !dupeValueFound && !keyIsEmpty && !valueIsEmpty)
                        logDictionary[propertyPath].Log.Reset();

                    if (!keyIsEmpty)
                        logDictionary[propertyPath].Log.LogError($"{dataReference.displayName} is empty");

                    keyIsEmpty = true;
                }

                //Use listSize for per page validation otherwise use list.Length
                for (int x = startIndex; x < listSize; x++)
                {                        
                    var loopElement = list.GetItem(x);

                    if (loopElement.propertyPath == propertyPath) continue;

                    var otherDataReference = loopElement.FindPropertyRelative(typeToNameMap[myType][1]);
                    var otherAssetReference = loopElement.FindPropertyRelative(typeToNameMap[myType][2]);

                    //Check dublicate keys
                    if (dataReference.objectReferenceValue == otherDataReference.objectReferenceValue)
                    {
                        if (!dupeKeyFound && !dupeValueFound && !keyIsEmpty && !valueIsEmpty)
                            logDictionary[propertyPath].Log.Reset();

                        if (dataReference.objectReferenceValue != null && otherDataReference.objectReferenceValue != null)
                        {
                            list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;
                                
                            if (!dupeKeyFound)
                                logDictionary[propertyPath].Log.LogError($"Duplicate Key: {dataReference.objectReferenceValue.name}");
                                
                            dupeKeyFound = true;
                        }
                    }
                    //Check dublicate values
                    switch (myType)
                    {
                        case ListType.Faction:
                            if(!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var factionGraph = GetFactionGraphAssetReferenceValue(assetReference, ref refLabel); 
                            var otherFactionGraph = GetFactionGraphAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (factionGraph == null || !factionGraph.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (factionGraph.editorAsset.Equals(otherFactionGraph.editorAsset)) 
                            {
                                list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;
                                if(!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {factionGraph.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break; 
                        case ListType.MechSkin:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var skin = GetSkinAssetReferenceValue(assetReference, ref refLabel);
                            var otherSkin = GetSkinAssetReferenceValue(otherAssetReference, ref refLabel);

                            if (skin == null || !skin.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (skin.editorAsset.Equals(otherSkin.editorAsset))
                            {
                                list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;

                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {skin.editorAsset.name}");
                                    
                                dupeValueFound = true;
                            }
                            break;
                        case ListType.MechChassis:

                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var mechChassis = GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherMechChassis = GetAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (mechChassis == null || !mechChassis.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (mechChassis.editorAsset.Equals(otherMechChassis.editorAsset))
                            {
                                list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;

                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {mechChassis.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break;
                        case ListType.MysteryCrate:
                                
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var mysteryCrate = GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherMysteryCrate = GetAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (mysteryCrate == null || !mysteryCrate.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (mysteryCrate.editorAsset.Equals(otherMysteryCrate.editorAsset))
                            {
                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {mysteryCrate.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break;
                        case ListType.WeaponModel:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var weaponAsset = GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherWeaponAsset = GetAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (weaponAsset == null || !weaponAsset.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (weaponAsset.editorAsset.Equals(otherWeaponAsset.editorAsset))
                            {
                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {weaponAsset.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break;
                        case ListType.WeaponSkin:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var weaponSkin = GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherWeaponSkin = GetAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (weaponSkin == null || !weaponSkin.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (weaponSkin.editorAsset.Equals(otherWeaponSkin.editorAsset))
                            {
                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {weaponSkin.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break;
                        case ListType.PowerCore:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var powerCore = GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherPowerCore = GetAssetReferenceValue(otherAssetReference, ref refLabel);
                            if (powerCore == null || !powerCore.editorAsset)
                            {
                                if (!valueIsEmpty)
                                {
                                    logDictionary[propertyPath].Log.LogError($"{assetReference.displayName} is empty");
                                }
                                valueIsEmpty = true;
                                continue;
                            }
                            if (powerCore.editorAsset.Equals(otherPowerCore.editorAsset))
                            {
                                if (!dupeValueFound)
                                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {powerCore.editorAsset.name}");

                                dupeValueFound = true;
                            }
                            break;
                        default:
                            Debug.LogError($"Unknown list type {type}");
                            break;
                    }

                    loopElement.serializedObject.ApplyModifiedProperties();
                }

                if (dupeKeyFound || dupeValueFound || keyIsEmpty || valueIsEmpty)
                    logDictionary[propertyPath].ContainsError = true;

                if (!dupeKeyFound && !dupeValueFound && !keyIsEmpty && !valueIsEmpty)
                    logDictionary[propertyPath].ContainsError = false;

                element.serializedObject.ApplyModifiedProperties();

                if (logDictionary[propertyPath].ContainsError)
                {
                    GUI.color = Color.red;
                    EditorGUI.PropertyField(rect, element, label, true);
                    GUI.color = Color.white;
                }
                else EditorGUI.PropertyField(rect, element, label, true);
            }; 
        }

        public void ElementSelectionDisplay()
        {
            if (selectedIndex < 0) return;
            if(selectedList == null || selectedList.Length <= 0 ) return;

            var selectedElement = selectedList.GetItem(selectedIndex);

            if(selectedElement == null) return;

            var dataReference = selectedElement.FindPropertyRelative(typeToNameMap[currentListType][1]);
            var assetReference = selectedElement.FindPropertyRelative(typeToNameMap[currentListType][2]);

            GUILayout.BeginVertical("HelpBox");

            GUILayout.Label("Selected Data");

            logDictionary[selectedElement.propertyPath].Log.Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();

            if (dataReference != null)
            {
                GUILayout.BeginVertical("GroupBox");
                EditorGUILayout.PropertyField(dataReference);
                if (dataReference.objectReferenceValue)
                {
                    CreateCachedEditor(dataReference.objectReferenceValue, null, ref _editor);
                    _editor.OnInspectorGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (assetReference != null)
            {
                GUILayout.BeginVertical("GroupBox");
                EditorGUILayout.PropertyField(assetReference);
                AssetReferenceSelecter(assetReference);
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

        }

        private AssetReferenceEnvironmentConnectivity GetFactionGraphAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceEnvironmentConnectivity>(fieldInfo, ref label);
        }
        
        private AssetReference GetAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReference>(fieldInfo, ref label);
        }

        private AssetReferenceSkin GetSkinAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Skin);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceSkin>(fieldInfo, ref label);
        }

        private void AssetReferenceSelecter(SerializedProperty assetReference)
        {
            AssetReferenceSkin skinRef;
            switch (currentListType)
            {
                case ListType.Faction:
                    var graphRef = GetFactionGraphAssetReferenceValue(assetReference, ref refLabel);
                    if (!graphRef.editorAsset) return;
                    CreateCachedEditor(graphRef.editorAsset, null, ref _editor);
                    break;
                case ListType.MechChassis:
                    return;
                case ListType.MechSkin:
                    skinRef = GetSkinAssetReferenceValue(assetReference, ref refLabel);
                    if (!skinRef.editorAsset) return;
                    CreateCachedEditor(skinRef.editorAsset, null, ref _editor);
                    break;
                case ListType.MysteryCrate:
                    return;
                case ListType.WeaponModel:
                    return;
                case ListType.WeaponSkin:
                    skinRef = GetSkinAssetReferenceValue(assetReference, ref refLabel);
                    if (!skinRef.editorAsset) return;
                    CreateCachedEditor(skinRef.editorAsset, null, ref _editor);
                    break;
                case ListType.PowerCore:
                    return;
                default:
                    Debug.LogError($"Unknown listType {currentListType}");
                    break;
            }
            _editor.OnInspectorGUI();
        }

        private void AddMissingKeys(ReorderableList key, ListType type)
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

            var assetMapPath = AssetDatabase.GUIDToAssetPath(assetMapGuids[0]);
            var assetMap = AssetDatabase.LoadAssetAtPath(assetMapPath, typeof(AssetMappings)) as AssetMappings;

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
                    var targetKey = parent.FindPropertyRelative(typeToNameMap[type][1]);

                    if (targetKey.objectReferenceValue == dataKey)
                        matchFound = true;
                }

                if (!matchFound)
                {
                    var parent = key.AddItem();
                    if (firstNewItemIndex == -1) firstNewItemIndex = key.Length - 1;
                    
                    var targetKey = parent.FindPropertyRelative(typeToNameMap[type][1]);
                    var assetRef = parent.FindPropertyRelative(typeToNameMap[type][2]);

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

                    //asset ref only
                    string[] targetAssetGuid = new string[1];
                    switch (type)
                    {
                        case ListType.Faction:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:EnvironmentConnectivity");
                            break;
                        case ListType.MechChassis:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:GameObject");
                            break;
                        case ListType.MechSkin:
                            string mechSkinfolderPath = SearchSubDirs("Assets/Content/Mechs", targetTypeName);
                            if (mechSkinfolderPath != null)
                                targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Skin", new[] { $"{mechSkinfolderPath}" });
                            break; 
                        case ListType.MysteryCrate:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:GameObject");
                            break;
                        case ListType.WeaponModel:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:GameObject");
                            break;
                        case ListType.WeaponSkin:
                            string weaponSkinfolderPath = SearchSubDirs("Assets/Content/Weapons", targetTypeName);
                            if (weaponSkinfolderPath != null)
                                targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:Skin", new[] {$"{weaponSkinfolderPath}"});
                            break;
                        case ListType.PowerCore:
                            targetAssetGuid = AssetDatabase.FindAssets($"{targetName} t:GameObject");
                            break;
                        default:
                            Debug.LogError($"Unknown type: {type}");
                            break;
                    }

                    if (targetAssetGuid.Length > 0 && targetAssetGuid[0] != null)
                    {
                        switch (type)
                        {
                            case ListType.Faction:
                                var targetFactionAsset = GetFactionGraphAssetReferenceValue(assetRef, ref refLabel);
                                var newAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as EnvironmentConnectivity;
                                targetFactionAsset.SetEditorAsset(newAsset);
                                break;
                            case ListType.MechSkin:
                                var targetMechSkinAsset = GetSkinAssetReferenceValue(assetRef, ref refLabel);
                                var newMechSkinAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as Skin;
                                targetMechSkinAsset.SetEditorAsset(newMechSkinAsset);
                                break;
                            case ListType.WeaponSkin:
                                var targetWeaponSkinAsset = GetSkinAssetReferenceValue(assetRef, ref refLabel);
                                var newWeaponSkinAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as Skin;
                                targetWeaponSkinAsset.SetEditorAsset(newWeaponSkinAsset);
                                break;
                            case ListType.PowerCore:
                            default:
                                var targetAsset = GetAssetReferenceValue(assetRef, ref refLabel);
                                var newGOAsset = settings.FindAssetEntry(targetAssetGuid[0]).MainAsset as GameObject;
                                targetAsset.SetEditorAsset(newGOAsset);
                                break;
                        }
                    }
                    parent.serializedObject.ApplyModifiedProperties();
                    parent.serializedObject.Update();
                }
            }

            if(key.paginate) key.SetPage(firstNewItemIndex/key.pageSize);
        }

        private string SearchSubDirs(string dir, string targetFolder)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(dir);

            foreach (string subdirectory in subdirectoryEntries)
            {
                var result = SearchSubDirs(subdirectory, targetFolder);
                if(result != null) return result;
                
                if(subdirectory.EndsWith(targetFolder, StringComparison.CurrentCultureIgnoreCase))
                    return subdirectory;
            }

            return null;
        }

        private IEnumerable<BaseRecord> GetStatisDataList(ListType type, Data staticData)
        {
            switch(type)
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

    public enum ReferenceTypes
    {
        Graph,
        Skin,
    }

    public static class MyExtensionMethods
    {
        public static System.Type GetType(SerializedProperty property, ReferenceTypes type)
        {
            var parentType = property.serializedObject.targetObject as AssetMappings;

            switch (type)
            {
                case ReferenceTypes.Graph:
                    foreach (var s in parentType.FactionMappingByGuid.Values)
                        return s.GetType();
                    break;
                case ReferenceTypes.Skin:
                    foreach (var s in parentType.MechSkinMappingByGuid.Values)
                        return s.GetType();
                    break;
                default:
                    break;
            }

            return null;
        }

        public static FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            if (type == null) return null;
            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
                return fi[1];
            else return null;
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SupremacyHangar.Runtime.ContentLoader.Types;
using Malee.List;
using SupremacyData.Editor;
using UnityEngine.AddressableAssets;

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
        private ReorderableList utilityModelsList;
        private ReorderableList utilitySkinsList;

        private int selectedIndex;
        private UnityEditor.Editor _editor;
        private ReorderableList selectedList;
        
        class AssetMapItem
        {
            public bool ContainsError = false;
            public LogWidget Log = new();
        }

        ListType currentListType;
        Dictionary<string, AssetMapItem> logDictionary = new();
        Dictionary<ReorderableList, ListType> typeByList = new();

        private AssetMappingAssetGrabber assetGrabber = new();
        private AssetMappingImporter assetImporter = new();

        private string refLabel = null;
        private bool dupeKeyFound = false;
        private bool dupeValueFound = false;
        private bool keyIsEmpty = false;
        private bool valueIsEmpty = false;

        private void OnEnable()
        {
            logDictionary.Clear();
            typeByList.Clear();
            
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
            utilityModelsList = new ReorderableList(serializedObject.FindProperty("utilityModels"));
            typeByList.Add(utilityModelsList, ListType.UtilityModel);
            utilitySkinsList = new ReorderableList(serializedObject.FindProperty("utilitySkins"));
            typeByList.Add(utilitySkinsList, ListType.UtilitySkin);
             
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
                        assetImporter.AddMissingKeys(listPair.Key, listPair.Value);
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
            utilityModelsList.DoLayoutList();
            utilitySkinsList.DoLayoutList();

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
                case "UtilityModelMapping":
                    return ListType.UtilityModel;
                case "UtilitySkinMapping":
                    return ListType.UtilitySkin;
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
                var dataReference = element.FindPropertyRelative(assetGrabber.TypeToNameMap[myType][1]);
                var assetReference = element.FindPropertyRelative(assetGrabber.TypeToNameMap[myType][2]);

                if (!logDictionary.ContainsKey(propertyPath))
                {
                    logDictionary.Add(propertyPath, new AssetMapItem());
                }

                dupeKeyFound = false;
                dupeValueFound = false;
                keyIsEmpty = false;
                valueIsEmpty = false;

                if (dataReference == null)
                {
                    if (!dupeKeyFound && !dupeValueFound && !keyIsEmpty && !valueIsEmpty)
                        logDictionary[propertyPath].Log.Reset();

                    if (!keyIsEmpty)
                        logDictionary[propertyPath].Log.LogError($"{dataReference.displayName} property {assetGrabber.TypeToNameMap[myType][1]} could not be found");

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

                    var otherDataReference = loopElement.FindPropertyRelative(assetGrabber.TypeToNameMap[myType][1]);
                    var otherAssetReference = loopElement.FindPropertyRelative(assetGrabber.TypeToNameMap[myType][2]);

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

                            var factionGraph = assetGrabber.GetFactionGraphAssetReferenceValue(assetReference, ref refLabel); 
                            var otherFactionGraph = assetGrabber.GetFactionGraphAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, factionGraph, otherFactionGraph, propertyPath);
                            break; 
                        case ListType.MechSkin:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var skin = assetGrabber.GetSkinAssetReferenceValue(assetReference, ref refLabel);
                            var otherSkin = assetGrabber.GetSkinAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, skin, otherSkin, propertyPath);
                            break;
                        case ListType.MechChassis:

                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var mechChassis = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherMechChassis = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, mechChassis, otherMechChassis, propertyPath);
                            break;
                        case ListType.MysteryCrate:
                                
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var mysteryCrate = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherMysteryCrate = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, mysteryCrate, otherMysteryCrate, propertyPath);
                            break;
                        case ListType.WeaponModel:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var weaponAsset = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherWeaponAsset = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, weaponAsset, otherWeaponAsset, propertyPath);
                            break;
                        case ListType.WeaponSkin:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var weaponSkin = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherWeaponSkin = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, weaponSkin, otherWeaponSkin, propertyPath);
                            break;
                        case ListType.PowerCore:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var powerCore = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherPowerCore = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, powerCore, otherPowerCore, propertyPath);
                            break;
                        case ListType.UtilityModel:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var utilityAsset = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherUtilityAsset = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, utilityAsset, otherUtilityAsset, propertyPath);
                            break;
                        case ListType.UtilitySkin:
                            if (!dupeValueFound && !dupeKeyFound && !keyIsEmpty && !valueIsEmpty)
                                logDictionary[propertyPath].Log.Reset();

                            var utilitySkin = assetGrabber.GetAssetReferenceValue(assetReference, ref refLabel);
                            var otherUtilitySkin = assetGrabber.GetAssetReferenceValue(otherAssetReference, ref refLabel);

                            ValidateMapping(assetReference, utilitySkin, otherUtilitySkin, propertyPath);
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

        private void ValidateMapping(SerializedProperty assetProperty, AssetReference assetReference, AssetReference otherAssetReference, string propertyPath)
        {

            if (assetReference == null || !assetReference.editorAsset)
            {
                if (!valueIsEmpty)
                {
                    logDictionary[propertyPath].Log.LogError($"{assetProperty.displayName} is empty");
                }
                valueIsEmpty = true;
                return;
            }
            if (assetReference.editorAsset.Equals(otherAssetReference.editorAsset))
            {
                if (!dupeValueFound)
                    logDictionary[propertyPath].Log.LogError($"Dublicate Value: {assetReference.editorAsset.name}");

                dupeValueFound = true;
            }
        }

        public void ElementSelectionDisplay()
        {
            if (selectedIndex < 0) return;
            if(selectedList == null || selectedList.Length <= 0 ) return;

            var selectedElement = selectedList.GetItem(selectedIndex);

            if(selectedElement == null) return;

            var dataReference = selectedElement.FindPropertyRelative(assetGrabber.TypeToNameMap[currentListType][1]);
            var assetReference = selectedElement.FindPropertyRelative(assetGrabber.TypeToNameMap[currentListType][2]);

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

        private void AssetReferenceSelecter(SerializedProperty assetReference)
        {
            switch (currentListType)
            {
                case ListType.Faction:
                    var graphRef = assetGrabber.GetFactionGraphAssetReferenceValue(assetReference, ref refLabel);
                    if (!graphRef.editorAsset) return;
                    CreateCachedEditor(graphRef.editorAsset, null, ref _editor);
                    break;
                case ListType.MechChassis:
                    return;
                case ListType.MechSkin:
                    DisplaySelectedSkinAset(assetReference);
                    break;
                case ListType.MysteryCrate:
                    return;
                case ListType.WeaponModel:
                    return;
                case ListType.WeaponSkin:
                    DisplaySelectedSkinAset(assetReference);
                    break;
                case ListType.PowerCore:
                    return;
                case ListType.UtilityModel:
                    return;
                case ListType.UtilitySkin:
                    DisplaySelectedSkinAset(assetReference);
                    break;
                default:
                    Debug.LogError($"Unknown listType {currentListType}");
                    break;
            }
            _editor.OnInspectorGUI();
        }

        private void DisplaySelectedSkinAset(SerializedProperty assetReference)
        {
            AssetReferenceSkin skinRef = assetGrabber.GetSkinAssetReferenceValue(assetReference, ref refLabel);
            if (!skinRef.editorAsset) return;
            CreateCachedEditor(skinRef.editorAsset, null, ref _editor);
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using SupremacyHangar.Runtime.ContentLoader;
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
        private ReorderableList mechChassisList;
        private ReorderableList mechSkinList;
        private ReorderableList mysteryCrateList;

        private int selectedIndex;
        private UnityEditor.Editor _editor;
        private ReorderableList selectedList;
        enum ListType
        {
            Faction,
            MechChassis,
            MechSkin,
            MysteryCrate,
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
            
            initTypeToNameMap();
            selectedIndex = -1;

            factionList = new ReorderableList(serializedObject.FindProperty("factions"));
            typeByList.Add(factionList, ListType.Faction);
            mechChassisList = new ReorderableList(serializedObject.FindProperty("mechChassis"));
            typeByList.Add(mechChassisList, ListType.MechChassis);
            mechSkinList = new ReorderableList(serializedObject.FindProperty("mechSkins"));
            typeByList.Add(mechSkinList, ListType.MechSkin);
            mechSkinList.paginate = true;
            mechSkinList.pageSize = 10;
            mysteryCrateList = new ReorderableList(serializedObject.FindProperty("mysteryCrates"));
            typeByList.Add(mysteryCrateList, ListType.MysteryCrate);

            foreach (var listPair in typeByList)
            {
                DrawListElements(listPair.Key, listPair.Value);
             
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

        private void initTypeToNameMap()
        {
            typeToNameMap.Clear();
            //String Order
            /*
                0 - list label
                1 - data reference property name
                2 - asset reference property name
            */
            typeToNameMap.Add(ListType.Faction, new[] { "Factions", "dataFaction", "connectivityGraph" });
            typeToNameMap.Add(ListType.MechChassis, new[] { "Mech Chassis", "dataMechModel", "mechReference" });
            typeToNameMap.Add(ListType.MechSkin, new[] { "Mech Skins", "dataMechSkin", "skinReference" });
            typeToNameMap.Add(ListType.MysteryCrate, new[] { "Mystery Crates", "dataMysteryCrate", "mysteryCrateReference" });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            factionList.DoLayoutList();
            mechChassisList.DoLayoutList();
            mechSkinList.DoLayoutList();
            mysteryCrateList.DoLayoutList();

            ElementSelectionDisplay();
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawListElements(ReorderableList list, ListType type)
        {
            list.drawElementCallback += (Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused) =>
            {
                ListType myType = ListType.Faction;
                switch (element.type)
                {
                    case "FactionMapping":
                        myType = ListType.Faction;
                        break;
                    case "MechChassisMapping":
                        myType = ListType.MechChassis;
                        break;
                    case "MechSkinMapping":
                        myType = ListType.MechSkin;
                        break;
                    case "MysteryCrateMapping":
                        myType = ListType.MysteryCrate;
                        break;
                    default:
                        break;
                } 

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
                    var objectRef = dataReference.objectReferenceValue;

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

            var selectedElement = selectedList.GetItem(selectedIndex);

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
            System.Type type = MyExtensionMethods.GetType(assetReference, referenceTypes.graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceEnvironmentConnectivity>(fieldInfo, ref label);
        }
        
        private AssetReference GetAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, referenceTypes.graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReference>(fieldInfo, ref label);
        }

        private AssetReferenceSkin GetSkinAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, referenceTypes.graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceSkin>(fieldInfo, ref label);
        }

        private void AssetReferenceSelecter(SerializedProperty assetReference)
        {
            if (currentListType == ListType.MechChassis || currentListType == ListType.MysteryCrate) return;

            switch (currentListType)
            {
                case ListType.Faction:
                    var graphRef = GetFactionGraphAssetReferenceValue(assetReference, ref refLabel);
                    if (!graphRef.editorAsset) return;
                    CreateCachedEditor(graphRef.editorAsset, null, ref _editor);
                    break;
                case ListType.MechChassis:
                    Debug.LogError($"You Should not see me {currentListType}");
                    break;
                case ListType.MechSkin:
                    var skinRef = GetSkinAssetReferenceValue(assetReference, ref refLabel);
                    if (!skinRef.editorAsset) return;
                    CreateCachedEditor(skinRef.editorAsset, null, ref _editor);
                    break;
                case ListType.MysteryCrate:
                    Debug.LogError($"You Should not see me {currentListType}");
                    break;
                default:
                    Debug.LogError($"Unknown listType {currentListType}");
                    break;
            }
            _editor.OnInspectorGUI();
        }
    }

    public enum referenceTypes
    {
        graph,
        skin,
    }

    public static class MyExtensionMethods
    {
        public static System.Type GetType(SerializedProperty property, referenceTypes type)
        {
            var parentType = property.serializedObject.targetObject as AssetMappings;

            switch (type)
            {
                case referenceTypes.graph:
                    foreach (var s in parentType.FactionHallwayByGuid.Values)
                        return s.GetType();
                    break;
                case referenceTypes.skin:
                    foreach (var s in parentType.MechSkinAssetByGuid.Values)
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
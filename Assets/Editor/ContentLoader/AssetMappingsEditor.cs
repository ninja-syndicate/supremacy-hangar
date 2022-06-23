using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using Malee.List;
using SupremacyData.Editor;

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

        ListType currentListType;
        Dictionary<ListType, string[]> typeToNameMap = new();
        Dictionary<string, LogWidget> logDictionary = new();
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
            mechSkinList.pageSize = 20;
            mysteryCrateList = new ReorderableList(serializedObject.FindProperty("mysteryCrates"));
            typeByList.Add(mysteryCrateList, ListType.MysteryCrate);

            foreach (var listPair in typeByList)
            {
                listPair.Key.onSelectCallback += (l) =>
                {
                    selectedIndex = l.Index;
                    selectedList = l;
                    currentListType = typeByList[l];
                    Debug.Log($"Selecta! {currentListType}");
                };

                //Todo fix last selected element removal
                listPair.Key.onRemoveCallback += (ReorderableList l) =>
                {
                    l.RemoveItem(selectedIndex);
                    selectedIndex = -1;
                };
                
                listPair.Key.HighlightErrors = true;
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

            //logDictionary.Clear();
            factionList.DoLayoutList();
            mechChassisList.DoLayoutList();
            mechSkinList.DoLayoutList();
            mysteryCrateList.DoLayoutList();

            DrawListElements(factionList, ListType.Faction);
            DrawListElements(mechChassisList, ListType.MechChassis);
            DrawListElements(mechSkinList, ListType.MechSkin);
            DrawListElements(mysteryCrateList, ListType.MysteryCrate);

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
                Dictionary<SerializedProperty, LogWidget > temp = new();
                for (int i = 0; i < list.Length; i++)
                {
                    var property = list.GetItem(i);
                    var dataReference = property.FindPropertyRelative(typeToNameMap[myType][1]);
                    var assetReference = property.FindPropertyRelative(typeToNameMap[myType][2]);
                    var objectRef = dataReference.objectReferenceValue;

                    if (!logDictionary.ContainsKey(property.propertyPath))
                    {
                        logDictionary.Add(property.propertyPath, new LogWidget());
                    }


                    for (int x = 0; x < list.Length; x++)
                    {
                        if (i == x) continue;

                        logDictionary[property.propertyPath].Reset();
                        var otherDataReference = list.GetItem(x).FindPropertyRelative(typeToNameMap[myType][1]);
                        var otherAssetReference = list.GetItem(x).FindPropertyRelative(typeToNameMap[myType][2]);

                        if (!dataReference.objectReferenceValue || !otherDataReference.objectReferenceValue)
                        {
                            logDictionary[property.propertyPath].LogError($"{dataReference.displayName} is empty");
                            continue;
                        }
                        //Check dublicate keys
                        if (dataReference.objectReferenceValue == otherDataReference.objectReferenceValue)
                        {
                            list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;
                            
                            logDictionary[property.propertyPath].LogError("Dublicate Key");
                        }

                        //Check dublicate values
                        switch (myType)
                        {
                            case ListType.Faction:
                                var factionGraph = GetFactionGraphAssetReferenceValue(assetReference, ref refLabel);
                                var otherFactionGraph = GetFactionGraphAssetReferenceValue(otherAssetReference, ref refLabel);
                                if (!factionGraph.editorAsset || !otherFactionGraph.editorAsset)
                                {
                                    logDictionary[property.propertyPath].LogError($"{assetReference.displayName} is empty");
                                    continue;
                                }
                                if (factionGraph.editorAsset.Equals(otherFactionGraph.editorAsset))
                                {
                                    list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;

                                    logDictionary[property.propertyPath].LogError("Dublicate Value");
                                }
                                break; 
                            case ListType.MechSkin:
                                var skin = GetSkinAssetReferenceValue(assetReference, ref refLabel);
                                var otherSkin = GetSkinAssetReferenceValue(otherAssetReference, ref refLabel);
                                if (!skin.editorAsset || !otherSkin.editorAsset)
                                {
                                    logDictionary[property.propertyPath].LogError($"{assetReference.displayName} is empty");
                                    continue;
                                }
                                if (skin.editorAsset.Equals(otherSkin.editorAsset))
                                {
                                    list.GetItem(x).FindPropertyRelative("containsError").boolValue = true;
                                    logDictionary[property.propertyPath].LogError("Dublicate Value");
                                }
                                break;
                            case ListType.MechChassis:
                                break;
                            case ListType.MysteryCrate:
                                break;
                            default:
                                Debug.LogError($"Unknown list type {type}");
                                break;
                        }
                        list.GetItem(x).serializedObject.ApplyModifiedProperties();
                    }
                }

                list.DrawErrorHighlightingElement(rect, element);
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

            logDictionary[selectedElement.propertyPath].Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
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
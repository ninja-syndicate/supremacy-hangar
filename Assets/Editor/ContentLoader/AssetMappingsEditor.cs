using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using Malee.List;

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
        enum listType
        {
            faction,
            mechChassis,
            mechSkin,
            mysteryCrate,
        };

        listType currentListType;

        Dictionary<listType, string[]> typeToNameMap = new();
        private void OnEnable()
        {
            initTypeToNameMap();
            selectedIndex = -1;

            factionList = new ReorderableList(serializedObject.FindProperty("factions"));
            mechChassisList = new ReorderableList(serializedObject.FindProperty("mechChassis"));
            mechSkinList = new ReorderableList(serializedObject.FindProperty("mechSkins"));
            mechSkinList.paginate = true;
            mechSkinList.pageSize = 20;
            mysteryCrateList = new ReorderableList(serializedObject.FindProperty("mysteryCrates"));
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
            typeToNameMap.Add(listType.faction, new[] { "Factions", "dataFaction", "connectivityGraph" });
            typeToNameMap.Add(listType.mechChassis, new[] { "Mech Chassis", "dataMechModel", "mechReference" });
            typeToNameMap.Add(listType.mechSkin, new[] { "Mech Skins", "dataMechSkin", "skinReference" });
            typeToNameMap.Add(listType.mysteryCrate, new[] { "Mystery Crates", "dataMysteryCrate", "mysteryCrateReference" });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            factionList.DoLayoutList();
            mechChassisList.DoLayoutList();
            mechSkinList.DoLayoutList();
            mysteryCrateList.DoLayoutList();

            DrawListElements(factionList, listType.faction);
            DrawListElements(mechChassisList, listType.mechChassis);
            DrawListElements(mechSkinList, listType.mechSkin);
            DrawListElements(mysteryCrateList, listType.mysteryCrate);

            ElementSelectionDisplay();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawListElements(ReorderableList list, listType type) 
        {
            list.onSelectCallback = (ReorderableList l) =>
            {
                selectedIndex = l.Index;
                selectedList = l;
                currentListType = type;
            };

            //Todo fix last selected element removal
            list.onRemoveCallback = (ReorderableList l) =>
            {
                selectedIndex = -1;
                l.RemoveItem(l.Index);
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

            GUILayout.BeginHorizontal();

            if (dataReference != null)
            {
                GUILayout.BeginVertical("GroupBox");
                EditorGUILayout.PropertyField(dataReference);
                CreateCachedEditor(dataReference.objectReferenceValue, null, ref _editor);
                _editor.OnInspectorGUI();
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
            if (currentListType == listType.mechChassis || currentListType == listType.mysteryCrate) return;

            string label = null;
            System.Type type = MyExtensionMethods.GetType(assetReference);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);

            switch (currentListType)
            {
                case listType.faction:
                    CreateCachedEditor(assetReference.GetActualObjectForSerializedProperty<AssetReferenceEnvironmentConnectivity>(fieldInfo, ref label).editorAsset, null, ref _editor);
                    break;
                case listType.mechChassis:
                    Debug.LogError($"You Should not see me {currentListType}");
                    break;
                case listType.mechSkin:
                    CreateCachedEditor(assetReference.GetActualObjectForSerializedProperty<AssetReferenceSkin>(fieldInfo, ref label).editorAsset, null, ref _editor);
                    break;
                default:
                    Debug.LogError($"Unknown listType {currentListType}");
                    break;
            }
            _editor.OnInspectorGUI();
        }
    }

    public static class MyExtensionMethods
    {
        public static System.Type GetType(SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject as AssetMappings;

            foreach (var s in parentType.MechSkinAssetByGuid.Values) 
                return s.GetType();
            
            return null;
        }   

        public static FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
                return fi[1];
            else return null;
        }
    }
}
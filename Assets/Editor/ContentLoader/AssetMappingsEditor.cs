using SupremacyHangar.Runtime.ScriptableObjects;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Reflection;
using SupremacyHangar.Runtime.ContentLoader;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomEditor(typeof(Runtime.ContentLoader.AssetMappings))]
    public class AssetMappingsEditor : UnityEditor.Editor
    {
        private ReorderableList factionList;
        private ReorderableList mechChassisList;
        private ReorderableList mechSkinList;

        private int selectedIndex;
        private UnityEditor.Editor _editor;
        private ReorderableList selectedList;
        enum listType
        {
            faction,
            mechChassis,
            mechSkin,
        };

        listType currentListType;

        Dictionary<listType, string[]> typeToNameMap = new();
        private void OnEnable()
        {
            initTypeToNameMap();
               selectedIndex = -1;
            factionList = new ReorderableList(serializedObject, serializedObject.FindProperty("factions"));
            mechChassisList = new ReorderableList(serializedObject, serializedObject.FindProperty("mechChassis"));
            mechSkinList = new ReorderableList(serializedObject, serializedObject.FindProperty("mechSkins"));
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
            typeToNameMap.Add(listType.faction, new[] { "Faction", "dataFaction", "connectivityGraph" });
            typeToNameMap.Add(listType.mechChassis, new[] { "Mech Chassis", "dataMechModel", "mechReference" });
            typeToNameMap.Add(listType.mechSkin, new[] { "Mech Skin", "dataMechSkin", "skinReference" });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            factionList.DoLayoutList();
            mechChassisList.DoLayoutList();
            mechSkinList.DoLayoutList();

            factionListDrawElement(factionList, listType.faction);
            factionListDrawElement(mechChassisList, listType.mechChassis);
            factionListDrawElement(mechSkinList, listType.mechSkin);

            SelectCallBack();
            serializedObject.ApplyModifiedProperties();
        }

        private void factionListDrawElement(ReorderableList list, listType type)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, typeToNameMap[type][0]);
            };

            list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    rect.x += 10;
                    EditorGUI.PropertyField(rect, element);
                };

            list.elementHeightCallback = (int index) =>
            {
                var element = factionList.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element);
            };

            list.onSelectCallback = (ReorderableList l) =>
            {
                selectedIndex = l.index;
                selectedList = l;
                currentListType = type;
            };
        }

        public void SelectCallBack()
        {
            if (selectedIndex < 0) return;

            var selectedElement = selectedList.serializedProperty.GetArrayElementAtIndex(selectedIndex);
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
                if (currentListType != listType.mechChassis)
                {
                    string label = null;
                    System.Type type = MyExtensionMethods.GetType(assetReference);
                    FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
                    CreateCachedEditor(assetReference.GetActualObjectForSerializedProperty<Skin>(fieldInfo, ref label), null, ref _editor);
                    _editor.OnInspectorGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }        
    }

    public static class MyExtensionMethods
    {
        public static System.Type GetType(SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject as AssetMappings;

            //FieldInfo[] fi = parentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var s in parentType.MechSkinAssetByGuid.Values)//fi[6].FieldType;
                return s.GetType();   
            
            return null;
        }   

        public static FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            System.Type parentType = type;
            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            string[] perDot = path.Split('.');
            //foreach (string fieldName in perDot)
            //{
            //    fi = parentType.GetField(fieldName);
            //    if (fi != null)
            //        parentType = fi.FieldType;
            //    else
            //        return null;
            //}
            if (fi != null)
                return fi[1];
            else return null;
        }
    }
}
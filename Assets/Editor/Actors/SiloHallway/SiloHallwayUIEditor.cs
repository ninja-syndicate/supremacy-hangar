using System.Collections.Generic;
using System.Linq;
using SupremacyHangar.Runtime.Actors.SiloHallway;
using UnityEditor;
using UnityEngine;

namespace SupremacyHangar.Editor.Actors.SiloHallway
{
    [CustomEditor(typeof(Runtime.Actors.SiloHallway.SiloHallwayUI))]
    public class SiloHallwayUIEditor : UnityEditor.Editor
    {
        private class Settings
        {
            public Color TestColor = Color.black;
            public Sprite TestLogo = null;
            public string TypeString = "Mech";
            public string Name1 = "I am Name 1";
            public string Name2 = "I am Name 2";
            public int SiloNumber = 1;
        }
        
        private SerializedProperty factionColorImagesProperty;
        private SerializedProperty factionColorButtonsProperty;
        private SerializedProperty factionColorTextProperty;
        private SerializedProperty darkerFactionColorMultiplierProperty;
        private SerializedProperty darkerFactionColorImagesProperty;

        private SerializedProperty factionLogoProperty;
        private SerializedProperty siloNumberProperty;
        private SerializedProperty siloContentsTypeProperty;
        private SerializedProperty siloContentsName1Property;
        private SerializedProperty siloContentsName2Property;
        private SerializedProperty loadButtonProperty;

        private readonly  HashSet<SerializedObject> showLinkedUIElements = new();
        private readonly  HashSet<SerializedObject> showTestingFunctions = new();
        private readonly Dictionary<SerializedObject, Settings> settingsMap = new ();
        private SiloHallwayUI targetObject;

        private void OnEnable()
        {
            targetObject = target as SiloHallwayUI;
            
            factionColorImagesProperty = serializedObject.FindProperty("factionColorImages");
            factionColorButtonsProperty = serializedObject.FindProperty("factionColorButtons");
            factionColorTextProperty = serializedObject.FindProperty("factionColorText");
            darkerFactionColorMultiplierProperty = serializedObject.FindProperty("darkerFactionColorMultiplier");
            darkerFactionColorImagesProperty = serializedObject.FindProperty("darkerFactionColorImages");
            
            factionLogoProperty = serializedObject.FindProperty("factionLogo");
            siloNumberProperty = serializedObject.FindProperty("siloNumber");
            siloContentsTypeProperty = serializedObject.FindProperty("siloContentsType");
            siloContentsName1Property = serializedObject.FindProperty("siloContentsName1");
            siloContentsName2Property = serializedObject.FindProperty("siloContentsName2");
            loadButtonProperty = serializedObject.FindProperty("loadButton");
            
            if (!settingsMap.ContainsKey(serializedObject)) settingsMap.Add(serializedObject, new Settings());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RenderTestingFunctions();
            RenderLinkedUIElements();
        }

        private void RenderTestingFunctions()
        {
            bool visible = showTestingFunctions.Contains(serializedObject);
            visible = EditorGUILayout.Foldout(visible, "Testing Functions");
            if (visible)
            {
                showTestingFunctions.Add(serializedObject);
            }
            else
            {
                showTestingFunctions.Remove(serializedObject);
                return;
            }

            EditorGUI.indentLevel++;

            bool sceneDirty = false;

            var settings = settingsMap[serializedObject];
            
            var newColor = EditorGUILayout.ColorField("Faction Color", settings.TestColor);

            if (newColor != settings.TestColor)
            {
                settings.TestColor = newColor;
                targetObject.UpdateFactionColor(newColor);
                sceneDirty = true;
            }

            var newSprite = EditorGUILayout.ObjectField(
                "Faction Logo", settings.TestLogo, typeof(Sprite), false)
                as Sprite;

            if (newSprite != settings.TestLogo)
            {
                settings.TestLogo = newSprite;
                targetObject.UpdateFactionLogo(newSprite);
                sceneDirty = true;
            }

            var newInt = EditorGUILayout.IntField("Silo Number", settings.SiloNumber);
            if (settings.SiloNumber != newInt)
            {
                settings.SiloNumber = newInt;
                targetObject.UpdateSiloNumber(newInt);
                sceneDirty = true;
            }
            
            var newString = EditorGUILayout.TextField("Contents Type", settings.TypeString);
            if (settings.TypeString != newString)
            {
                settings.TypeString = newString;
                targetObject.UpdateTypeString(newString);
                sceneDirty = true;
            }
            
            newString = EditorGUILayout.TextField("Contents Line 1", settings.Name1);
            if (settings.Name1 != newString)
            {
                settings.Name1 = newString;
                targetObject.UpdateName1(newString);
                sceneDirty = true;
            }         
            
            newString = EditorGUILayout.TextField("Contents Line 2", settings.Name2);
            if (settings.Name2 != newString)
            {
                settings.Name2 = newString;
                targetObject.UpdateName2(newString);
                sceneDirty = true;
            }        
            
            if (sceneDirty) SceneView.RepaintAll();
            
            EditorGUI.indentLevel--;
        }

        private void RenderLinkedUIElements()
        {
            bool visible = showLinkedUIElements.Contains(serializedObject);
            visible = EditorGUILayout.Foldout(visible, "Linked UI Elements");
            if (visible)
            {
                showLinkedUIElements.Add(serializedObject);
            }
            else
            {
                showLinkedUIElements.Remove(serializedObject);
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(factionColorImagesProperty);
            EditorGUILayout.PropertyField(factionColorButtonsProperty);
            EditorGUILayout.PropertyField(factionColorTextProperty);
            EditorGUILayout.PropertyField(darkerFactionColorMultiplierProperty);
            EditorGUILayout.PropertyField(darkerFactionColorImagesProperty);

            EditorGUILayout.PropertyField(factionLogoProperty);
            EditorGUILayout.PropertyField(siloNumberProperty);
            EditorGUILayout.PropertyField(siloContentsTypeProperty);
            EditorGUILayout.PropertyField(siloContentsName1Property);
            EditorGUILayout.PropertyField(siloContentsName2Property);
            EditorGUILayout.PropertyField(loadButtonProperty);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            
            EditorGUI.indentLevel--;
        }
        
    }
}
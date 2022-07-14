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
        private SerializedProperty interactionButtonProperty;
        private SerializedProperty interactionButtonTextProperty;
        
        private SerializedProperty startFactionColorProperty;
        private SerializedProperty clockUpdateDelayProperty;

        private SerializedProperty loadRequestTextProperty;
        private SerializedProperty loadingTextProperty;
        private SerializedProperty openRequestTextProperty;
        private SerializedProperty openingTextProperty;        
        
        private readonly Dictionary<SerializedObject, Settings> settingsMap = new ();
        private readonly  HashSet<SerializedObject> showTestingFunctions = new();
        private readonly  HashSet<SerializedObject> showLinkedUIElements = new();
        private readonly  HashSet<SerializedObject> showRuntimeElements = new();

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
            interactionButtonProperty = serializedObject.FindProperty("interactionButton");
            interactionButtonTextProperty = serializedObject.FindProperty("interactionButtonText");
            
            startFactionColorProperty = serializedObject.FindProperty("startFactionColor");
            clockUpdateDelayProperty = serializedObject.FindProperty("clockUpdateDelay");
            
            loadRequestTextProperty = serializedObject.FindProperty("loadRequestText");
            loadingTextProperty = serializedObject.FindProperty("loadingText");
            openRequestTextProperty = serializedObject.FindProperty("openRequestText");
            openingTextProperty = serializedObject.FindProperty("openingText");

            if (!settingsMap.ContainsKey(serializedObject)) settingsMap.Add(serializedObject, new Settings());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RenderTestingFunctions();
            RenderLinkedUIElements();
            RenderRuntimeElements();
        }

        private void RenderTestingFunctions()
        {
            if (!ShowElements("Testing Functions", showTestingFunctions)) return;

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
            if (!ShowElements("Linked UI Elements", showLinkedUIElements)) return;

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(factionColorImagesProperty);
            EditorGUILayout.PropertyField(factionColorButtonsProperty);
            EditorGUILayout.PropertyField(factionColorTextProperty);
            EditorGUILayout.PropertyField(darkerFactionColorMultiplierProperty);
            EditorGUILayout.PropertyField(darkerFactionColorImagesProperty);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(factionLogoProperty);
            EditorGUILayout.PropertyField(siloNumberProperty);
            EditorGUILayout.PropertyField(siloContentsTypeProperty);
            EditorGUILayout.PropertyField(siloContentsName1Property);
            EditorGUILayout.PropertyField(siloContentsName2Property);
            EditorGUILayout.PropertyField(interactionButtonProperty);
            EditorGUILayout.PropertyField(interactionButtonTextProperty);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(loadRequestTextProperty);
            EditorGUILayout.PropertyField(loadingTextProperty);
            EditorGUILayout.PropertyField(openRequestTextProperty);
            EditorGUILayout.PropertyField(openingTextProperty);

            
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            
            EditorGUI.indentLevel--;
        }

        private void RenderRuntimeElements()
        {
            if (!ShowElements("Linked Runtime Elements", showRuntimeElements)) return;       
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(startFactionColorProperty);
            EditorGUILayout.PropertyField(clockUpdateDelayProperty);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        private bool ShowElements(string labelName, HashSet<SerializedObject> unfoldedTargets)
        {
            bool visible = unfoldedTargets.Contains(serializedObject);
            visible = EditorGUILayout.Foldout(visible, labelName);
            if (visible)
            {
                unfoldedTargets.Add(serializedObject);
            }
            else
            {
                unfoldedTargets.Remove(serializedObject);
            }

            return visible;
        }
        
    }
}
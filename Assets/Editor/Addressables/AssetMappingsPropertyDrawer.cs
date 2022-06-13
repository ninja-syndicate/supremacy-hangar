using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SupremacyHangar.Editor.Addressables
{
    public abstract class BaseMappingDrawer<T> : PropertyDrawer where T: class
    {
        protected abstract string StaticDataPropertyName { get;}
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float size = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded) size = size * 2 + GUIStatics.Controls.VerticalPadding * 1;
            return size;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                RenderExpanded(position, property, label);
                return;
            }
            RenderSummary(position, property, label);
        }
        
        private void RenderSummary(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            float controlWidth = position.width * 0.5f - GUIStatics.Controls.HorizontalPadding;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "");
            Rect dataRect = new Rect(position.x, position.y, controlWidth, position.height);
            Rect otherRect = new Rect(position.x + dataRect.width + GUIStatics.Controls.HorizontalPadding, position.y, controlWidth, position.height);

            string dataLabel;
            var data =
                property.FindPropertyRelative(StaticDataPropertyName).objectReferenceValue as T;
            dataLabel = data == null ? "No Data Linked" : StaticDataPropertySummary(data); 
            EditorGUI.LabelField(dataRect, dataLabel);
            EditorGUI.LabelField(otherRect, "Linking not defined");
        }

        protected abstract string StaticDataPropertySummary(T data);

        private void RenderExpanded(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect controlRect = position;
            controlRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(controlRect, property.isExpanded, "");
            var controlLabel = new GUIContent("Data Reference", "The static data reference for this element");
            EditorGUI.PropertyField(controlRect, property.FindPropertyRelative(StaticDataPropertyName), controlLabel);
            controlRect.y += EditorGUIUtility.singleLineHeight + GUIStatics.Controls.VerticalPadding;
            EditorGUI.LabelField(controlRect, "No linking model defined");
        }           
    }
}
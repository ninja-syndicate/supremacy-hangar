using UnityEditor;
using UnityEngine;

namespace SupremacyHangar.Editor.ContentLoader
{
    public abstract class BaseMappingDrawer<TMapping> : PropertyDrawer
    {
        protected abstract string StaticDataPropertyName { get; }
        protected abstract string AssetDataPropertyName { get; }
        
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
        
        protected abstract string StaticDataPropertySummary(TMapping data);
        protected abstract string AssetPropertySummary(TMapping data);
        protected abstract void CheckForErrors(TMapping data);
        
        private void RenderSummary(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            float controlWidth = position.width * 0.5f - GUIStatics.Controls.HorizontalPadding;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "");
            Rect dataRect = new Rect(position.x, position.y, controlWidth, position.height);
            Rect otherRect = new Rect(position.x + dataRect.width + GUIStatics.Controls.HorizontalPadding, position.y, controlWidth, position.height);

            string targetLabel = null;
            var targetObj = property.GetActualObjectForSerializedProperty<TMapping>(fieldInfo, ref targetLabel);
            EditorGUI.LabelField(dataRect, StaticDataPropertySummary(targetObj));
            EditorGUI.LabelField(otherRect, AssetPropertySummary(targetObj));
        }

        private void RenderExpanded(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect controlRect = position;
            controlRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(controlRect, property.isExpanded, "");
            var controlLabel = new GUIContent("Data Reference", "The static data reference for this element");
            EditorGUI.PropertyField(controlRect, property.FindPropertyRelative(StaticDataPropertyName), controlLabel);
            controlRect.y += EditorGUIUtility.singleLineHeight + GUIStatics.Controls.VerticalPadding;
            controlLabel = new GUIContent("Asset Reference", "The unity asset/addressable reference for this element");
            EditorGUI.PropertyField(controlRect, property.FindPropertyRelative(AssetDataPropertyName), controlLabel);
        }           
    }
}
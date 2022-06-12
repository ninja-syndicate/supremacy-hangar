using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SupremacyHangar.Editor.Addressables
{
    [CustomPropertyDrawer(typeof(Runtime.Addressables.FactionMapping))]
    public class FactionMappingDrawer : PropertyDrawer
    {
        private const float HorizontalPadding = 5.0f;
        private const float VerticalPadding = 2.0f;
        
        //TODO: test this inside a visual element thingy
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var template = EditorGUIUtility.Load("AssetMappingElement.uxml") as VisualTreeAsset;
            VisualElement container = template.CloneTree();
            var dataField = new PropertyField(property.FindPropertyRelative("dataFaction"));
            container.Add(dataField);
            return container;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float size = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded) size = size * 2 + VerticalPadding * 1;
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
            float controlWidth = position.width * 0.5f - HorizontalPadding;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "");
            Rect dataRect = new Rect(position.x, position.y, controlWidth, position.height);
            Rect otherRect = new Rect(position.x + dataRect.width + HorizontalPadding, position.y, controlWidth, position.height);

            string dataLabel;
            var factionData =
                property.FindPropertyRelative("dataFaction").objectReferenceValue as SupremacyData.Runtime.Faction;
            if (factionData == null)
            {
                dataLabel = "No Data Linked";
            }
            else
            {
                dataLabel = $"Data: {factionData.HumanName}";
            }
            
            EditorGUI.LabelField(dataRect, dataLabel);
            EditorGUI.LabelField(otherRect, "Linking not defined");
        }
        
        private void RenderExpanded(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect controlRect = position;
            controlRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(controlRect, property.isExpanded, "");
            var controlLabel = new GUIContent("Data Reference", "The static data reference for this element");
            EditorGUI.PropertyField(controlRect, property.FindPropertyRelative("dataFaction"), controlLabel);
            controlRect.y += EditorGUIUtility.singleLineHeight + VerticalPadding;
            EditorGUI.LabelField(controlRect, "No linking model defined");
        }        
    }
}
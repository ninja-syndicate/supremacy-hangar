using UnityEditor;

namespace SupremacyData.Editor
{
    
    [CustomEditor(typeof(Runtime.Faction))]
    public class FactionEditor : BaseRecordEditor<Runtime.Faction>
    {
        public override void OnInspectorGUI()
        {
            if (!TargetRecordSet)
            {
                EditorGUILayout.LabelField("Invalid Data!");
                return;
            }

            RenderHumanName();
            RenderID();
            
            EditorGUILayout.TextField("LogoURL", TargetRecord.LogoURL);
            EditorGUILayout.TextField("BackgroundURL", TargetRecord.LogoURL);
            EditorGUILayout.ColorField("Primary Color", TargetRecord.PrimaryColor);
            EditorGUILayout.ColorField("Secondary Color", TargetRecord.SecondaryColor);
            EditorGUILayout.ColorField("Background Color", TargetRecord.BackgroundColor);
        }
    }
}
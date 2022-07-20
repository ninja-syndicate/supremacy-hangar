using UnityEditor;

namespace SupremacyData.Editor
{
    
    [CustomEditor(typeof(Runtime.WeaponSkin))]
    public class WeaponSkinEditor : BaseRecordEditor<Runtime.WeaponSkin>
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
        }
    }
}
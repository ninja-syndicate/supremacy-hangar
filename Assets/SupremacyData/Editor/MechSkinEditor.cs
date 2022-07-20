using UnityEditor;

namespace SupremacyData.Editor
{
    
    [CustomEditor(typeof(Runtime.MechSkin))]
    public class MechSkinEditor : BaseRecordEditor<Runtime.MechSkin>
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
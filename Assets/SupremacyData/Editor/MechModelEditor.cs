using UnityEditor;

namespace SupremacyData.Editor
{
    
    [CustomEditor(typeof(Runtime.MechModel))]
    public class MechModelEditor : BaseRecordEditor<Runtime.MechModel>
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
            RenderType();
            RenderBrand();
        }

        private void RenderType()
        {
            switch (TargetRecord.Type)
            {
                case Runtime.MechModel.ModelType.Humanoid:
                    EditorGUILayout.SelectableLabel("Humanoid Type");
                    break;
                case Runtime.MechModel.ModelType.Platform:
                    EditorGUILayout.SelectableLabel("Platform Type");
                    break;
                default:
                    EditorGUILayout.SelectableLabel("Unknown Type");
                    break;
            }
        }

        private void RenderBrand()
        {
            if (TargetRecord.Brand == null)
            {
                EditorGUILayout.SelectableLabel("No Brand Assigned!");
                return;
            }
            EditorGUILayout.TextField("Brand", TargetRecord.Brand.HumanName);
            EditorGUILayout.TextField("Faction", TargetRecord.Brand.Faction.HumanName);
        }
    }
}
using UnityEditor;

namespace SupremacyData.Editor
{
    
    [CustomEditor(typeof(Runtime.WeaponModel))]
    public class WeaponModelEditor : BaseRecordEditor<Runtime.WeaponModel>
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
                case Runtime.WeaponModel.ModelType.LightningGun:
                    EditorGUILayout.SelectableLabel("Lightning Gun Type");
                    break;
                case Runtime.WeaponModel.ModelType.Minigun:
                    EditorGUILayout.SelectableLabel("Minigun Type");
                    break;
                case Runtime.WeaponModel.ModelType.MissileLauncher:
                    EditorGUILayout.SelectableLabel("Missile Launcher Type");
                    break;
                case Runtime.WeaponModel.ModelType.BFG:
                    EditorGUILayout.SelectableLabel("Big Friendly Gun Type");
                    break;
                case Runtime.WeaponModel.ModelType.Flamethrower:
                    EditorGUILayout.SelectableLabel("Flamethrower Type");
                    break;
                case Runtime.WeaponModel.ModelType.Flak:
                    EditorGUILayout.SelectableLabel("Flak Type");
                    break;
                case Runtime.WeaponModel.ModelType.Cannon:
                    EditorGUILayout.SelectableLabel("Cannon Type");
                    break;
                case Runtime.WeaponModel.ModelType.GrenadeLauncher:
                    EditorGUILayout.SelectableLabel("Grenade Launcher Type");
                    break;
                case Runtime.WeaponModel.ModelType.MachineGun:
                    EditorGUILayout.SelectableLabel("Machine Gun Type");
                    break;
                case Runtime.WeaponModel.ModelType.LaserBeam:
                    EditorGUILayout.SelectableLabel("Laser Beam Type");
                    break;
                case Runtime.WeaponModel.ModelType.Sword:
                    EditorGUILayout.SelectableLabel("Sword Type");
                    break;
                case Runtime.WeaponModel.ModelType.SniperRifle:
                    EditorGUILayout.SelectableLabel("Sniper Rifle Type");
                    break;
                case Runtime.WeaponModel.ModelType.PlasmaRifle:
                    EditorGUILayout.SelectableLabel("Plasma Rifle Type");
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
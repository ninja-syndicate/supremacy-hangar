using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.MechChassisMapping))]
    public class MechChassisMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.MechChassisMapping>
    {
        protected override string StaticDataPropertyName => "dataMechModel";
        protected override string AssetDataPropertyName => "mechReference";

        protected override string StaticDataPropertySummary(Runtime.ContentLoader.MechChassisMapping data)
        {
            return data.DataMechModel != null ? data.DataMechModel.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.MechChassisMapping data)
        {
            if (data.MechReference != null && data.MechReference.editorAsset != null)
            {
                return data.MechReference.editorAsset.name; 
            }
            return "No Mech Asset Ref";
        }
    }
}
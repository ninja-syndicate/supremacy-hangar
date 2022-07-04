using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.WeaponModelMapping))]
    public class WeaponModelMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.WeaponModelMapping>
    {
        protected override string StaticDataPropertyName => "data";
        
        protected override string AssetDataPropertyName => "reference";

        protected override string StaticDataPropertySummary(Runtime.ContentLoader.WeaponModelMapping data)
        {
            return data.Data != null ? data.Data.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.WeaponModelMapping data)
        {
            if (data.Reference != null && data.Reference.editorAsset != null)
            {
                return data.Reference.editorAsset.name;
            }
            return "No Weapon Mapping Reference";
        }
    }
}
using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.WeaponSkinMapping))]
    public class WeaponSkinMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.WeaponSkinMapping>
    {
        protected override string StaticDataPropertyName => "data";
        
        protected override string AssetDataPropertyName => "reference";
        
        protected override string StaticDataPropertySummary(Runtime.ContentLoader.WeaponSkinMapping data)
        {
            return data.Data != null ? data.Data.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.WeaponSkinMapping data)
        {
            if (data.Reference != null && data.Reference.editorAsset != null)
            {
                return data.Reference.editorAsset.name;
            }
            return "No Weapon Mapping Reference";
        }
    }
}
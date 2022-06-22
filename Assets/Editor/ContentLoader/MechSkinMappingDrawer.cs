using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.MechSkinMapping))]
    public class MechSkinMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.MechSkinMapping>
    {
        protected override string StaticDataPropertyName => "dataMechSkin";
        
        protected override string AssetDataPropertyName => "skinReference";
        
        protected override string StaticDataPropertySummary(Runtime.ContentLoader.MechSkinMapping data)
        {
            if (data == null) return "No Data";
            CheckForErrors(data);
            return data.DataMechSkin != null ? data.DataMechSkin.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.MechSkinMapping data)
        {
            if (data == null) return "No Data";
            CheckForErrors(data);
            if (data.SkinReference != null && data.SkinReference.editorAsset != null)
            {
                return data.SkinReference.editorAsset.name;
            }
            return "No Mech Skin Ref";
        }

        protected override void CheckForErrors(Runtime.ContentLoader.MechSkinMapping data)
        {
            if (data.SkinReference.editorAsset == null || data.DataMechSkin == null)
                data.ContainsError = true;
            else
                data.ContainsError = false;
        }
    }
}
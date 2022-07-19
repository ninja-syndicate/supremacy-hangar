using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.UtilitySkinMapping))]
    public class UtilitySkinMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.UtilitySkinMapping>
    {
        protected override string StaticDataPropertyName => "data";
        
        protected override string AssetDataPropertyName => "reference";
        
        protected override string StaticDataPropertySummary(Runtime.ContentLoader.UtilitySkinMapping data)
        {
            return data.Data != null ? data.Data.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.UtilitySkinMapping data)
        {
            if (data.Reference != null && data.Reference.editorAsset != null)
            {
                return data.Reference.editorAsset.name;
            }
            return "No Utlity skin Mapping Reference";
        }
    }
}
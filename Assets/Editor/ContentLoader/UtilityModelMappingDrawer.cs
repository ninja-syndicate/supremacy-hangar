using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.UtilityModelMapping))]
    public class UtilityModelMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.UtilityModelMapping>
    {
        protected override string StaticDataPropertyName => "data";
        
        protected override string AssetDataPropertyName => "reference";

        protected override string StaticDataPropertySummary(Runtime.ContentLoader.UtilityModelMapping data)
        {
            return data.Data != null ? data.Data.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.UtilityModelMapping data)
        {
            if (data.Reference != null && data.Reference.editorAsset != null)
            {
                return data.Reference.editorAsset.name;
            }
            return "No Utility Mapping Reference";
        }
    }
}
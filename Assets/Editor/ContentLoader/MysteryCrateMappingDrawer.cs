using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.MysteryCrateMapping))]
    public class MysteryCrateMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.MysteryCrateMapping>
    {
        protected override string StaticDataPropertyName => "dataMysteryCrate";
        
        protected override string AssetDataPropertyName => "mysteryCrateReference";
        
        protected override string StaticDataPropertySummary(Runtime.ContentLoader.MysteryCrateMapping data)
        {
            if (data == null) return "No Data";
            CheckForErrors(data);
            return data.DataMysteryCrate != null ? data.DataMysteryCrate.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.MysteryCrateMapping data)
        {
            if (data == null) return "No Data"; 
            CheckForErrors(data);
            if (data.MysteryCrateReference != null && data.MysteryCrateReference.editorAsset != null)
            {
                return data.MysteryCrateReference.editorAsset.name;
            }
            return "No Mystery Crate Ref";
        }

        protected override void CheckForErrors(Runtime.ContentLoader.MysteryCrateMapping data)
        {
            if (data.MysteryCrateReference.editorAsset == null || data.DataMysteryCrate == null)
                data.ContainsError = true;
            else
                data.ContainsError = false;
        }
    }
}
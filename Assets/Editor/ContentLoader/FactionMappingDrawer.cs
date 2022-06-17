using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.FactionMapping))]
    public class FactionMappingDrawer : BaseMappingDrawer<Runtime.ContentLoader.FactionMapping>
    {
        protected override string StaticDataPropertyName => "dataFaction";
        protected override string AssetDataPropertyName => "connectivityGraph";
        protected override string StaticDataPropertySummary(Runtime.ContentLoader.FactionMapping data)
        {
            return data.DataFaction != null ? data.DataFaction.name : "No Static Data";
        }

        protected override string AssetPropertySummary(Runtime.ContentLoader.FactionMapping data)
        {
            return data.ConnectivityGraph != null ? data.ConnectivityGraph.editorAsset.name : "No Connectivity Graph";
        }
    }
}
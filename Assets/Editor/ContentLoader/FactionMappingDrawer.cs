using UnityEditor;
using UnityEngine;

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
            return data.ConnectivityGraph.editorAsset != null ? data.ConnectivityGraph.editorAsset.name : "No Connectivity Graph";
        }

        protected override void SetValidity(Runtime.ContentLoader.FactionMapping data)
        {
            if (data.ConnectivityGraph.editorAsset == null || data.DataFaction == null)
                data.ContainsError = true;
            else
                data.ContainsError = false;
        }
    }
}
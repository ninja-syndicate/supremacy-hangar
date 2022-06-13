using SupremacyData.Runtime;
using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.FactionMapping))]
    public class FactionMappingDrawer : BaseMappingDrawer<Faction>
    {
        protected override string StaticDataPropertyName => "dataFaction";

        protected override string StaticDataPropertySummary(Faction data)
        {
            return data.HumanName;
        }
    }
}
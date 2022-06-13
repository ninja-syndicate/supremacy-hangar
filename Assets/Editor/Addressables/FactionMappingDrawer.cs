using SupremacyData.Runtime;
using UnityEditor;

namespace SupremacyHangar.Editor.Addressables
{
    [CustomPropertyDrawer(typeof(Runtime.Addressables.FactionMapping))]
    public class FactionMappingDrawer : BaseMappingDrawer<Faction>
    {
        protected override string StaticDataPropertyName => "dataFaction";

        protected override string StaticDataPropertySummary(Faction data)
        {
            return data.HumanName;
        }
    }
}
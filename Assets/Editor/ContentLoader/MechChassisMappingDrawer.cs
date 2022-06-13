using SupremacyData.Runtime;
using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.MechChassisMapping))]
    public class MechChassisMappingDrawer : BaseMappingDrawer<MechModel>
    {
        protected override string StaticDataPropertyName => "dataMechModel";
        protected override string StaticDataPropertySummary(MechModel data)
        {
            return data.HumanName;
        }
    }
}
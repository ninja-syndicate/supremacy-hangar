using SupremacyData.Runtime;
using UnityEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    [CustomPropertyDrawer(typeof(Runtime.ContentLoader.MechSkinMapping))]
    public class MechSkinMappingDrawer : BaseMappingDrawer<MechSkin>
    {
        protected override string StaticDataPropertyName => "dataMechSkin";
        protected override string StaticDataPropertySummary(MechSkin data)
        {
            return data.HumanName;
        }
    }
}
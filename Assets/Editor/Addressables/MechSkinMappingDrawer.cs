using SupremacyData.Runtime;
using UnityEditor;

namespace SupremacyHangar.Editor.Addressables
{
    [CustomPropertyDrawer(typeof(Runtime.Addressables.MechSkinMapping))]
    public class MechSkinMappingDrawer : BaseMappingDrawer<MechSkin>
    {
        protected override string StaticDataPropertyName => "dataMechSkin";
        protected override string StaticDataPropertySummary(MechSkin data)
        {
            return data.HumanName;
        }
    }
}
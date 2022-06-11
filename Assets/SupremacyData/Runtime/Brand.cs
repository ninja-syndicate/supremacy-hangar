using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Brand : BaseRecord
    {
        public string HumanName => humanName;
        private Faction Faction => faction;

        [SerializeField] internal string humanName;
        [SerializeField] internal Faction faction;
    }
}
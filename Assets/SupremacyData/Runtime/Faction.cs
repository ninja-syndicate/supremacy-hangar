using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Faction : BaseRecord
    {
        public string HumanName => humanName;

        [SerializeField] internal string humanName;
    }
}
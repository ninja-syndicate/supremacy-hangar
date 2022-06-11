using UnityEngine;

namespace SupremacyData.Runtime
{
    public class BattleAbility : BaseRecord
    {
        public string HumanName => humanName;
        //This is the cooldown in seconds.
        public int Cooldown => cooldown;
        public string Description => description;
        
        [SerializeField] internal string humanName;
        //This is the cooldown in seconds.
        [SerializeField] internal int cooldown;
        [SerializeField] internal string description;
    }
}
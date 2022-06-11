using UnityEngine;

namespace SupremacyData.Runtime
{
    public class BattleAbility : BaseRecord
    {
        //This is the cooldown in seconds.
        public int Cooldown => cooldown;
        public string Description => description;
        
        //This is the cooldown in seconds.
        [SerializeField] internal int cooldown;
        [SerializeField] internal string description;
    }
}
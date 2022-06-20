using UnityEngine;

namespace SupremacyData.Runtime
{
    public class MysteryCrate : BaseRecord
    {
        public enum ModelType
        {
            Weapon,
            Mech,
        };

        public Faction Faction => faction;
        public ModelType Type => type;
        
        [SerializeField] [SerializeReference] internal Faction faction;
        [SerializeField] [SerializeReference] internal ModelType type;
    }
}
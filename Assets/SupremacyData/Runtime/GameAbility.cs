using UnityEngine;

namespace SupremacyData.Runtime
{
    public class GameAbility : BaseRecord
    {
        public enum AbilityLevel
        {
            Mech,
            Faction
        }
        
        public Faction Faction => faction;
        public BattleAbility BattleAbility => battleAbility;
        public Color Color => color;
        public Color TextColor => textColor;
        public AbilityLevel Level => level;
        
        [SerializeField] [SerializeReference] internal Faction faction;
        [SerializeField] [SerializeReference] internal BattleAbility battleAbility;
        [SerializeField] internal Color color;
        [SerializeField] internal Color textColor;
        [SerializeField] internal string imageURL;
        [SerializeField] internal string description;
        [SerializeField] internal AbilityLevel level;
    }
}
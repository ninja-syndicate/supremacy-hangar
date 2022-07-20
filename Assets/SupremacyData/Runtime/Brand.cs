using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Brand : BaseRecord
    {
        public Faction Faction => faction;
        [SerializeField][SerializeReference] internal Faction faction;
    }
}
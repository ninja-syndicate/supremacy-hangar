using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Brand : BaseRecord
    {
        private Faction Faction => faction;
        [SerializeField][SerializeReference] internal Faction faction;
    }
}
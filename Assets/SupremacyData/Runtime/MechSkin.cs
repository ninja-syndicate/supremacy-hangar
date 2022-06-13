using UnityEngine;

namespace SupremacyData.Runtime
{
    public class MechSkin : BaseRecord
    {
        public MechModel MechModel => mechModel;
        
        [SerializeField] [SerializeReference] internal MechModel mechModel;
    }
}
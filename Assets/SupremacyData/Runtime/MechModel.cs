using UnityEngine;

namespace SupremacyData.Runtime
{
    public class MechModel : BaseRecord
    {
        public enum ModelType
        {
            Humanoid,
            Platform,
        }
        
        public Brand Brand => brand;
        public ModelType Type => type;
        public MechSkin DefaultSkin => defaultSkin;
        
        [SerializeField] [SerializeReference] internal Brand brand;
        [SerializeField] [SerializeReference] internal ModelType type;
        [SerializeField] [SerializeReference] internal MechSkin defaultSkin;
    }
}
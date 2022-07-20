using UnityEngine;

namespace SupremacyData.Runtime
{
    public class UtilityModel : BaseRecord
    {
        public enum ModelType
        {
            ShieldGenerator,
            Accelorator,
            Interceptor,
            DroneLauncher,
        }
        
        public Brand Brand => brand;
        public ModelType Type => type;

        public UtilitySkin DefaultSkin => defaultSkin;
        
        [SerializeField] [SerializeReference] internal Brand brand;
        [SerializeField] [SerializeReference] internal UtilitySkin defaultSkin;
        [SerializeField] [SerializeReference] internal ModelType type;
    }
}
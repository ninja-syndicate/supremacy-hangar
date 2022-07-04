using UnityEngine;

namespace SupremacyData.Runtime
{
    public class WeaponModel : BaseRecord
    {
        private Brand Brand => brand;
        public WeaponSkin DefaultSkin => defaultSkin;
        
        [SerializeField] [SerializeReference] internal Brand brand;
        [SerializeField] [SerializeReference] internal WeaponSkin defaultSkin;
    }
}
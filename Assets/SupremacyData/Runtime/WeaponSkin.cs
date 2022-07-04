using UnityEngine;

namespace SupremacyData.Runtime
{
    public class WeaponSkin : BaseRecord
    {
        public WeaponModel WeaponModel => weaponModel;
        
        [SerializeField] [SerializeReference] internal WeaponModel weaponModel;
    }
}
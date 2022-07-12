using UnityEngine;

namespace SupremacyData.Runtime
{
    public class WeaponSkin : BaseRecord
    {
        public WeaponModel WeaponModel => weaponModel;
        public WeaponModel.ModelType Type => type;
        
        [SerializeField] [SerializeReference] internal WeaponModel weaponModel;
        [SerializeField] [SerializeReference] internal WeaponModel.ModelType type;
    }
}
using UnityEngine;

namespace SupremacyData.Runtime
{
    public class WeaponModel : BaseRecord
    {
        public enum ModelType
        {
            LightningGun,
            Minigun,
            MissileLauncher,
            BFG,
            Flamethrower,
            Flak,
            Cannon,
            GrenadeLauncher,
            MachineGun,
            LaserBeam,
            Sword,
            SniperRifle,
            PlasmaRifle,
        }        
        
        private Brand Brand => brand;
        public ModelType Type => type;

        public WeaponSkin DefaultSkin => defaultSkin;
        
        [SerializeField] [SerializeReference] internal Brand brand;
        [SerializeField] [SerializeReference] internal WeaponSkin defaultSkin;
        [SerializeField] [SerializeReference] internal ModelType type;
    }
}
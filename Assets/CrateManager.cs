using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class CrateManager : MonoBehaviour
    {
        private SignalBus _bus;
        private AddressablesManager addressablesManager;
        
        private bool _subscribed;

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform spawnPoint;

        [Inject]
        public void Contruct(SignalBus bus, AddressablesManager manager)
        {
            _bus = bus;
            addressablesManager = manager;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<FillCrateSignal>(FillCrate);
            _bus.Unsubscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<FillCrateSignal>(FillCrate);
            _bus.Subscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = true;
        }

        private void FillCrate(FillCrateSignal signal)
        {
            addressablesManager.MapSiloToAsset(signal.CrateContents);
            //Set item to spawn in crate
            switch (signal.CrateContents)
            {
                case Mech mech:
                    addressablesManager.TargetMech = mech.MechChassisDetails.MechReference; 
                    addressablesManager.TargetSkin = mech.MechSkinDetails.SkinReference;
                    break;
                case Weapon weapon:
                    //addressablesManager.TargetMech = weapon;
                    //addressablesManager.TargetSkin = null;
                    break;
                default:
                    Debug.LogWarning($"Unexpected type of {signal.CrateContents.GetType()} - cowardly refusing to fill the crate", this);
                    break;
            }
            addressablesManager.SpawnMech(spawnPoint, true);
        }

        private void OpenCrate()
        {
            _animator.SetBool("open", true);
        }
    }
}

using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class CrateDoorController : MonoBehaviour
    {
        private SignalBus _bus;
        
        private bool _subscribed;

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform spawnPoint;

        [Inject]
        public void Contruct(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = true;
        }

        private void OpenCrate()
        {
            _animator.SetBool("open", true);
        }
    }
}

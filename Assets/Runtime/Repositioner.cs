using System;
using System.ComponentModel;
using SupremacyHangar.Runtime.Environment;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime
{
    public class Repositioner : MonoBehaviour
    {
        private SignalBus _bus;
        private bool _subscribed;
        
        [SerializeField]
        private CharacterController _characterController;

        [Inject]
        public void Initialize(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        private void SubscribeToSignal()
        {
            _bus.Subscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = true;
        }

        public void OnEnable()
        {
            if (_bus == null || _subscribed) return;
            SubscribeToSignal();
        }

        public void OnDisable()
        {
            if (!_subscribed) return; 
            _bus.Unsubscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = false;
        }

        //trying to access destroyed object???? (not destroyed)
        public void MoveToZero(RepositionObjectSignal signal)
        {
            ToggleCharacterController();
            Debug.Log($"{name} got move", this);
            transform.position -= signal.Position;

            ToggleCharacterController();
        }

        private void ToggleCharacterController()
        {
            if (_characterController)
                _characterController.enabled = !_characterController.enabled;
        }
    }
}

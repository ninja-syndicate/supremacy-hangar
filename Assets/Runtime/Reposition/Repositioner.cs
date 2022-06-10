using System;
using System.ComponentModel;
using UnityEngine;
using Zenject;

<<<<<<<< HEAD:Assets/Runtime/Environment/Repositioner.cs
namespace SupremacyHangar.Runtime.Environment
========
namespace SupremacyHangar.Runtime.Reposition
>>>>>>>> develop:Assets/Runtime/Reposition/Repositioner.cs
{
    public class Repositioner : MonoBehaviour
    {
        private SignalBus _bus;
        private bool _subscribed;
        
        private CharacterController _characterController;

        private void Awake()
        {
            if(TryGetComponent(out CharacterController controller))
            {
                _characterController = controller;
            }
        }

        [Inject]
        public void Initialize(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = true;
        }

        public void OnEnable()
        {
            SubscribeToSignal();
        }

        public void OnDisable()
        {
            if (!_subscribed) return; 
            _bus.Unsubscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = false;
        }

        public void MoveToZero(RepositionObjectSignal signal)
        {
            ToggleCharacterController();
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

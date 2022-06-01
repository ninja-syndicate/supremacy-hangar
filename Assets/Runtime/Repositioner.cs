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
        
        [Inject]
        public void Initialize(SignalBus bus)
        {
            _bus = bus;
            _bus.Subscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = true;
        }

        public void OnEnable()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = true;
        }

        public void OnDisable()
        {
            _bus.Unsubscribe<RepositionObjectSignal>(MoveToZero);
            _subscribed = false;
        }

        public void MoveToZero()
        {
            Debug.Log($"{name} got move", this);
            transform.position = Vector3.zero;
        }
    }
}

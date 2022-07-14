using System;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloState : MonoBehaviour
    {
        public StateName CurrentState => state;
        //public SiloItem Contents => contents;

        public enum StateName
        {
            NotLoaded,
            LoadingSilo,
            EmptySiloLoaded,
            LoadingSiloContent,
            Loaded,
            LoadedWithCrate,
            LoadingCrateContent,
            Unloading,
        }

        [SerializeField] private int siloOffset;
        
        public SiloItem Contents;
        
        [SerializeField] private StateName state;

        public event Action<StateName> OnStateChanged;

        public Transform SpawnLocation { get; set; }

        private SignalBus _bus;

        public bool CanOpenCrate { get; private set; } = false;
        private bool _subscribed;

        [Inject]
        public void Construct(SiloItem[] hallwayContents, SignalBus bus)
        {
            Contents = hallwayContents[siloOffset];
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
            _bus.Unsubscribe<CanOpenCrateSignal>(CrateCanOpen);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<CanOpenCrateSignal>(CrateCanOpen);
            _subscribed = true;
        }

        private void CrateCanOpen()
        {
            CanOpenCrate = true;
            OnStateChanged?.Invoke(CurrentState);
        }

        public void UserInteraction()
        {
            var nextState = state;
            switch (CurrentState)
            {
                case StateName.NotLoaded:
                    nextState = StateName.LoadingSilo;
                    break;
                case StateName.LoadedWithCrate:
                    if(CanOpenCrate)
                        nextState = StateName.LoadingCrateContent;
                    break;
                default:
                    Debug.LogError($"Current state has no action {CurrentState}", this);
                    break;
            }

            if (nextState != state)
            {
                state = nextState;
                OnStateChanged?.Invoke(nextState);
            }
        }

        public void NextState(StateName nextState)
        {
            state = nextState; 
            OnStateChanged?.Invoke(nextState);
        }
    }
}
using System;
using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Silo
{
    public class SiloState : MonoBehaviour
    {
        public StateName CurrentState => state;
        public SiloItem Contents => contents;

        public Faction CurrentFaction { get; private set; }
        
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
        public int SiloNumber { get; private set; }

        [SerializeField] private int siloOffset;
        
        private SiloItem contents;
        
        [SerializeField] private StateName state;

        public event Action<StateName> OnStateChanged;

        public Transform SpawnLocation { get; set; }

        private SignalBus _bus;

        public bool CanOpenCrate { get; private set; } = false;
        private bool _subscribed;
        private EnvironmentManager _envManager;
        [Inject]
        public void Construct(
            EnvironmentManager environmentManager, AddressablesManager addressablesManager,
            SiloItem[] hallwayContents, SignalBus bus)
        {
            _envManager = environmentManager;
            SiloNumber = environmentManager.SiloOffset + siloOffset;
            CurrentFaction = addressablesManager.CurrentFaction;
            contents = hallwayContents[siloOffset];
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

        public void ChangeSiloContentToCrate(SiloItem crateContent)
        {
            switch(crateContent)
            {
                case Mech:
                case Weapon:
                    contents = crateContent;
                    _envManager.UpdatePlayerInventory(siloOffset, crateContent);
                    break;

            }
        }
    }
}
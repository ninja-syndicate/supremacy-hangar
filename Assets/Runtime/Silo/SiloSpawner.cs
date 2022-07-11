using UnityEngine;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.Environment.Connections;
using TMPro;
using SupremacyHangar.Runtime.Actors.Silo;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Types;
using System;
using System.Collections;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloSpawner : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;

        [SerializeField]
        private ConnectivityJoin to_Connect_to;

        public ConnectivityJoin ToConnectTo => to_Connect_to; 

        [SerializeField] private Collider siloDoorTrigger;
        [SerializeField] private Collider otherSiloInteractionTrigger;
                
        public bool SiloSpawned = false;

        [SerializeField] private Animator myWindowAnim;

        private SignalBus _bus;
        private bool _subscribed;
        private bool siloClosing = false;

        //ToDo link with progress bar text changer
        [SerializeField] private TMP_Text loadButtonText;

        private SiloState siloState;

        [Inject]
        public void Constuct(EnvironmentManager environmentManager, SignalBus bus, SiloState siloState)
        {
            _environmentManager = environmentManager;
            _bus = bus;
            SubscribeToSignal();
            this.siloState = siloState;
            this.siloState.OnStateChanged += OnSiloStateChanged;
        }

        private void OnSiloStateChanged(SiloState.StateName newState)
        {
            switch (newState)
            {
                case SiloState.StateName.LoadingSilo:
                    PrepareSilo();
                    otherSiloInteractionTrigger.enabled = false;
                    break;
                case SiloState.StateName.LoadingCrateContent:
                    otherSiloInteractionTrigger.enabled = false;
                    break;
                case SiloState.StateName.LoadedWithCrate:
                case SiloState.StateName.Loaded:
                    otherSiloInteractionTrigger.enabled = true;
                    ChangeButtonToOpen();
                    break;
            }
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<CloseSiloSignal>(CloseSilo);
            _bus.Unsubscribe<SiloUnloadedSignal>(SiloClosed);
            _bus.Unsubscribe<SiloFilledSignal>(OpenSilo);
            _bus.Unsubscribe<AssetLoadedWithSpawnSignal>(NextSiloState);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<CloseSiloSignal>(CloseSilo);
            _bus.Subscribe<SiloUnloadedSignal>(SiloClosed);
            _bus.Subscribe<SiloFilledSignal>(OpenSilo);
            _bus.Subscribe<AssetLoadedWithSpawnSignal>(NextSiloState);
            _subscribed = true;
        }

        private void NextSiloState(AssetLoadedWithSpawnSignal signal)
        {
            switch (siloState.CurrentState)
            {
                case SiloState.StateName.LoadingSilo:
                    siloState.SpawnLocation = signal.SpawnPoint;
                    if(signal.SpawnPoint != null)
                        siloState.NextState(SiloState.StateName.EmptySiloLoaded);
                    break;
            }
        }

        public void CloseSilo()
        {
            //Lock doors on silo unload
            siloDoorTrigger.enabled = false;

            siloClosing = true;
            Debug.Log($"close {name}");
            //Close window on silo unload
            myWindowAnim.SetBool("IsOpen", false);

        }

        private void SiloClosed()
        {
            siloClosing = false;
            loadButtonText.text = "Pressurize";

            Debug.Log($"closed {name}");
            if (SiloSpawned)
                SpawnSilo();

            switch (siloState.CurrentState)
            {
                case SiloState.StateName.Loaded:
                case SiloState.StateName.LoadedWithCrate:
                    siloState.NextState(SiloState.StateName.NotLoaded);
                    break;
            }
        }

        public void PrepareSilo()
        {
            //Prevent same silo spawning again
            if (SiloSpawned) return;
            
            SiloSpawned = true;
            Debug.Log($"prep {name}");
            //Clean-up existing silo (Only one silo at a time)
            _environmentManager.UnloadSilo(this);
            
            SpawnSilo();
        }

        private void SpawnSilo()
        {
            Debug.Log($"spawn {name} silo {siloClosing} || {SiloSpawned}");
            //Spawn silo
            if (!siloClosing && SiloSpawned) _environmentManager.SpawnSilo(this);
        }

        public void OpenSilo()
        {
            if (!SiloSpawned) return;
            //unlock doors
            siloDoorTrigger.enabled = true;

            Debug.Log($"open {name}");
            //open window
            myWindowAnim.SetBool("IsOpen", true);

            //change console button value
            ChangeButtonToOpen();
        }

        private void ChangeButtonToOpen()
        {
            if (siloState.Contents is MysteryCrate && siloState.CanOpenCrate)
                loadButtonText.text = "Open";
        }
    }
}

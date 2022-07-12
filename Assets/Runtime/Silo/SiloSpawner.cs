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
        [SerializeField] private AudioSource windowAudio;
        [SerializeField] private AudioClip closeSoundClip;
        [SerializeField] private AudioClip openSoundClip;

        private SignalBus _bus;
        private bool _subscribed;
        private bool siloClosing = false;

        //ToDo link with progress bar text changer
        [SerializeField] private TMP_Text loadButtonText;

        private SiloState siloState;

        private bool siloNeedsSpawning = false;
        private bool loadingCrateContent = false;

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
            if (!otherSiloInteractionTrigger) return;
            switch (newState)
            {
                case SiloState.StateName.LoadingSilo:
                    PrepareSilo();
                    otherSiloInteractionTrigger.enabled = false;
                    break;
                case SiloState.StateName.LoadingCrateContent:
                    loadingCrateContent = true;
                    otherSiloInteractionTrigger.enabled = false;
                    break;
                case SiloState.StateName.LoadedWithCrate:
                    ChangeButtonToOpen();
                    break;
                case SiloState.StateName.Loaded:
                    if(loadingCrateContent) otherSiloInteractionTrigger.enabled = true;
                    loadingCrateContent = false;
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
            _bus.Unsubscribe<UnlockOtherSilo>(ToggleOtherSiloInteraction);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<CloseSiloSignal>(CloseSilo);
            _bus.Subscribe<SiloUnloadedSignal>(SiloClosed);
            _bus.Subscribe<SiloFilledSignal>(OpenSilo);
            _bus.Subscribe<AssetLoadedWithSpawnSignal>(NextSiloState);
            _bus.Subscribe<UnlockOtherSilo>(ToggleOtherSiloInteraction);
            _subscribed = true;
        }

        private void ToggleOtherSiloInteraction()
        {
            if(SiloSpawned)
                otherSiloInteractionTrigger.enabled = !otherSiloInteractionTrigger.enabled;
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

            //Close window on silo unload
            myWindowAnim.SetBool("IsOpen", false);
            windowAudio.clip = closeSoundClip;
            windowAudio.Play();

        }

        private void SiloClosed()
        {
            siloClosing = false;
            loadButtonText.text = "Pressurize";

            if (siloNeedsSpawning)
            {
                SpawnSilo();
                siloNeedsSpawning = false;
            }

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
            siloNeedsSpawning = true;

            //Clean-up existing silo (Only one silo at a time)
            _environmentManager.UnloadSilo(this);
            
            SpawnSilo();
        }

        private void SpawnSilo()
        {
            //Spawn silo
            if (!siloClosing && SiloSpawned) _environmentManager.SpawnSilo(this);
        }

        public void OpenSilo()
        {
            if (!SiloSpawned) return;
            //unlock doors
            siloDoorTrigger.enabled = true;

            //open window
            myWindowAnim.SetBool("IsOpen", true);
            windowAudio.clip = openSoundClip;
            windowAudio.Play();

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

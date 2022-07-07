using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.Environment.Connections;
using SupremacyData.Runtime;
using TMPro;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloPositioner : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;

        [SerializeField]
        private ConnectivityJoin to_Connect_to;

        public ConnectivityJoin ToConnectTo => to_Connect_to; 

        [SerializeField] private Collider siloDoorTrigger;
        [SerializeField] private Collider siloInteractionTrigger;

        public bool ContainsCrate { get; set; } = false;
        public bool canOpenCrate { get; set; } = false;
        
        public bool SiloSpawned = false;

        [SerializeField] private Animator myWindowAnim;

        private SignalBus _bus;
        private bool _subscribed;
        private bool siloClosing = false;

        //ToDo link with progress bar text changer
        [SerializeField] private TMP_Text loadButtonText;

        [Inject]
        public void Constuct(EnvironmentManager environmentManager, SignalBus bus)
        {
            _environmentManager = environmentManager;
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
            _bus.Unsubscribe<CloseSiloSignal>(CloseSilo);
            _bus.Unsubscribe<SiloUnloadedSignal>(SiloClosed);
            _bus.Unsubscribe<SiloFilledSignal>(OpenSilo);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<CloseSiloSignal>(CloseSilo);
            _bus.Subscribe<SiloUnloadedSignal>(SiloClosed);
            _bus.Subscribe<SiloFilledSignal>(OpenSilo);
            _subscribed = true;
        }

        public void CloseSilo()
        {
            //Lock doors on silo unload
            siloDoorTrigger.enabled = false;

            siloClosing = true;
            //Close window on silo unload
            myWindowAnim.SetBool("IsOpen", false);
        }

        private void SiloClosed()
        {
            siloClosing = false;
            ContainsCrate = false;
            loadButtonText.text = "Pressurize";

            if (SiloSpawned)
                SpawnSilo();
        }

        public void PrepareSilo()
        {
            //Prevent same silo spawning again
            if (SiloSpawned) return;
            
            SiloSpawned = true;
            siloInteractionTrigger.enabled = false;

            //Clean-up existing silo (Only one silo at a time)
            _environmentManager.UnloadSilo();
            
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

            //unlock other silo Trigger
            siloInteractionTrigger.enabled = true;

            //open window
            myWindowAnim.SetBool("IsOpen", true);

            //change console button value
            if (ContainsCrate)
            {
                canOpenCrate = true;
                loadButtonText.text = "Open";
            }
        }
    }
}

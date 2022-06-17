using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.Environment.Connections;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloPositioner : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;

        [SerializeField]
        private ConnectivityJoin to_Connect_to;

        public ConnectivityJoin ToConnectTo => to_Connect_to; 

        [SerializeField]
        private Collider siloDoorTrigger;

        public bool SiloSpawned { get; set; } = false;

        [SerializeField] Animator myWindowAnim;

        [Inject]
        public void Constuct(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        public void DisableDoor()
        {
            //Lock doors on silo unload
            siloDoorTrigger.enabled = false;

            //Close window on silo unload
            myWindowAnim.SetBool("IsOpen", false);
        }

        public void SpawnSilo()
        {
            //Prevent same silo spawning again
            if (SiloSpawned) return;
            SiloSpawned = true;

            //Clean-up existing silo (Only one silo at a time)
            _environmentManager.UnloadAssets();

            //Spawn silo
            _environmentManager.SpawnSilo(this);
        }

        public void OpenSilo()
        {
            //unlock doors
            siloDoorTrigger.enabled = true;

            //open window
            myWindowAnim.SetBool("IsOpen", true);
        }
    }
}

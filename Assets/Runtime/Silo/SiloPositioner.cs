using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupremacyHangar.Runtime.Environment;
using Zenject;
using SupremacyHangar.Runtime.ScriptableObjects;

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


        [Inject]
        public void Constuct(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        private void OnDisable()
        {
            //Lock doors on silo unload
            siloDoorTrigger.enabled = false;

            //Close window on silo unload
            //siloWindowAnim.SetBool("open", false);
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

            //unlock doors
            siloDoorTrigger.enabled = true;

            //open window
            //siloWindowAnim.SetBool("open", true);
        }
    }
}

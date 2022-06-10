using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentSpawner : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;

        [SerializeField]
        private EnvironmentSpawner otherEnvironmentSpawner;

        [SerializeField]
        private EnvironmentPrefab myConnectors;

        public EnvironmentPrefab MyConnectors => myConnectors;

        [SerializeField]
        private string myEnvironmentConnector;
        
        [SerializeField]
        private string to_Connect_to;

        [SerializeField]
        private Animator DoorAnim;

        public bool Entered { get; private set; } = false;

        public bool Spawned = false;
        
        [SerializeField]
        private bool increment;

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            myConnectors.ColliderList.Add(GetComponent<Collider>());
            _environmentManager = environmentManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            Entered = true;
            if (otherEnvironmentSpawner.Spawned || otherEnvironmentSpawner.Entered)
            {
                DoorAnim.SetBool("IsOpen", true);
                if (Spawned)
                {
                    _environmentManager.resetConnection();
                }
                return;
            }
            
            //Remove silo before proceeding
            if (_environmentManager.SiloExists)
                _environmentManager.UnloadAssets();

            SpawnSection();

            DoorAnim.SetBool("IsOpen", true);
        }

        private void OnTriggerExit(Collider other)
        {
            Entered = false;
            if(!otherEnvironmentSpawner.Entered)
                DoorAnim.SetBool("IsOpen", false);

            if (otherEnvironmentSpawner.Entered == false && Spawned == false)
            {
                _environmentManager.resetConnection(true);
            }
            else if(Spawned)
            {
                _environmentManager.resetConnection();
            }
        }

        private void SpawnSection()
        {
            Spawned = true;

            if (increment)
            {
                _environmentManager.ChangeDirection(true);
            }
            else if (Spawned)
            {
                _environmentManager.ChangeDirection(false);
            }

            _environmentManager.SpawnPart(myEnvironmentConnector, to_Connect_to, myConnectors);
        }
    }
}

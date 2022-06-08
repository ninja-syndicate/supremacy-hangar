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

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            myConnectors.ColliderList.Add(GetComponent<Collider>());
            _environmentManager = environmentManager;
        }

        private void EnableDoors()
        {
            Debug.Log("Enable Doors", this);
            _environmentManager.toggleDoor(MyConnectors);
        }

        private void DisableDoors()
        {
            Debug.Log("Disable Doors", this);
            _environmentManager.toggleDoor(MyConnectors);
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
                    Debug.Log($"..2 {_environmentManager.currentEnvironment.currentGameObject}", this);
                }
                return;
            }
            
            //Remove silo before proceeding
            if (_environmentManager.SiloExists)
                _environmentManager.unloadAssets();

            //Debug.Log($"{name} spawned Section ", this);
            spawnSection();

            //_doorSignalHandler.DoorOpen(myConnectors.gameObject);
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
                //Debug.Log($".. {_environmentManager.currentEnvironment.currentGameObject}", this);
            }
            else if(Spawned)
            {
                _environmentManager.resetConnection();
            }
        }
        public bool directionChanged = false;
        public void spawnSection()
        {
            Spawned = true;

            if (to_Connect_to.Contains("2"))
            {
                Debug.Log("dir = forward", this);
                directionChanged = true;
                _environmentManager.ChangeDirection(true);
            }
            else if (Spawned)
            {
                Debug.Log("dir = backward", this);
                directionChanged = true;
                _environmentManager.ChangeDirection(false);
            }
            _environmentManager.spawnPart(myEnvironmentConnector, to_Connect_to, myConnectors, DoorAnim);
            otherEnvironmentSpawner.directionChanged = false;
        }
    }
}

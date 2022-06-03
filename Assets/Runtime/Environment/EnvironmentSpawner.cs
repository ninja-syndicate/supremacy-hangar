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

        public bool entered { get; private set; } = false;

        public bool spawned { get; set; } = false;

        private void Start()
        {}

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            myConnectors.ColliderList.Add(GetComponent<Collider>());
            _environmentManager = environmentManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            entered = true;
            if (otherEnvironmentSpawner.spawned || otherEnvironmentSpawner.entered)
            {
                DoorAnim.SetBool("IsOpen", true);
                return;
            }
            
            //Remove silo before proceeding
            if (_environmentManager.SiloExists)
                _environmentManager.unloadAssets();

            //Debug.Log($"{name} spawned Section ", this);
            spawnSection();

            DoorAnim.SetBool("IsOpen", true);
        }

        private void OnTriggerExit(Collider other)
        {
            entered = false;
            DoorAnim.SetBool("IsOpen", false); 
            
            if(!otherEnvironmentSpawner.entered && spawned)
                _environmentManager.resetConnection();

        }

        public void spawnSection()
        {
            _environmentManager.spawnPart(myEnvironmentConnector, to_Connect_to, myConnectors, DoorAnim);
            spawned = true;
        }
    }
}

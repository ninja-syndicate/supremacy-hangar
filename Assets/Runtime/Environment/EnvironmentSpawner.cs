using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class EnvironmentSpawner : MonoBehaviour
    {
        private EnvironmentManager EnvironmentManager;

        [SerializeField]
        private EnvironmentPrefab myConnectors;

        [SerializeField]
        private int environmentPrefabIndex = 0;

        [SerializeField]
        private string myEnvironmentConnector;
        
        [SerializeField]
        private string to_Connect_to;

        [SerializeField]
        private Collider otherCollider;

        //ToDo make single (like silo doors)
        [SerializeField]
        private Animator[] DoorAnims;

        private void Awake()
        {
            //Doors share the same environmentPrefab
            if (myConnectors.ColliderList.Count == 0)
            {
                myConnectors.ColliderList.Add(otherCollider);
                myConnectors.ColliderList.Add(GetComponent<Collider>());
            }
        }

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            EnvironmentManager = environmentManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            
            spawnSection();

            foreach (Animator anim in DoorAnims)
                anim.SetBool("isOpen", true);

            otherCollider.enabled = false;
            EnvironmentManager.currentEnvironment.currentGameObejct.GetComponent<RoomHandler>().previousDoor = myConnectors.ColliderList[1];
            myConnectors.ColliderList[1].enabled = false;
        }

        public void spawnSection()
        {
            EnvironmentManager.spawnPart(myEnvironmentConnector, environmentPrefabIndex, to_Connect_to, myConnectors, otherCollider, DoorAnims);
        }
    }
}

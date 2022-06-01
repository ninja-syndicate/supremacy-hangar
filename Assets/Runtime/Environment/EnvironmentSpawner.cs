using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupremacyHangar.Runtime.Environment;
using Zenject;

namespace SupremacyHangar
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

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            EnvironmentManager = environmentManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnvironmentManager.spawnPart(myEnvironmentConnector, environmentPrefabIndex, to_Connect_to, myConnectors, otherCollider);

            otherCollider.enabled = false;
            EnvironmentManager.currentEnvironment.currentGameObejct.GetComponent<RoomHandler>().previousDoor = GetComponent<Collider>();
            GetComponent<Collider>().enabled = false;
        }
    }
}

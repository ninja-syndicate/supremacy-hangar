using SupremacyHangar.Runtime.Environment;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Environment
{
    public class RoomTracker : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;

        [SerializeField] private EnvironmentSpawner[] mySpawners;

        [SerializeField] private EnvironmentPrefab myConnections;

        [SerializeField] private GameObject myRoot;
        public GameObject MyRoot => myRoot;

        private bool newObject = true;

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        public void UpdateRoom()
        {
            if (!newObject)
            {
                foreach (EnvironmentSpawner s in mySpawners)
                {
                    s.Spawned = false;
                }
                _environmentManager.setCurrentEnvironment(myConnections.connectedTo);

                //unload previous room/s & connectors
                _environmentManager.UnloadAssets();
                _environmentManager.UnloadSilo(false);
            }
        }

        public void NotNewRoom()
        {
            newObject = false;
        }
    }
}

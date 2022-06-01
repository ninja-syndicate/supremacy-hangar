using System.Collections;
using SupremacyHangar.Runtime.Environment;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SupremacyHangar
{
    public class RoomHandler : MonoBehaviour
    {
        private EnvironmentManager EnvironmentManager;

        private Collider hallwayConnector;

        public Collider previousDoor;

        [SerializeField]
        private Animator myDoor;

        [Inject]
        public void Construct(EnvironmentManager environmentManager, Collider prevDoor = null)
        {
            EnvironmentManager = environmentManager;
            //objectsToUnload = unloadObjects;
            previousDoor = prevDoor;
            //Debug.Log(objectsToUnload.Count);
        }

        // Start is called before the first frame update
        void Start()
        {
            hallwayConnector = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Room Trigger");
            //Close door


            //set this room as current
            EnvironmentManager.setCurrentEnvironment(gameObject);

            //Reactive collider
            if (previousDoor)
                previousDoor.enabled = true;

            //unload previous room/s & connectors
            EnvironmentManager.unloadAssets();
        }
    }
}

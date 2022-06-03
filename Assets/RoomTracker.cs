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


        private Animator myAnim;

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
        }

        private void Start()
        {
            myAnim = GetComponent<Animator>();
        }

        private bool newObject = true;

        private void Update()
        {
            if (newObject && myAnim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Open"))
                newObject = false;
        }

        public void updateRoom()
        {
            if (!newObject)
            {
                foreach (EnvironmentSpawner s in mySpawners)
                {
                    if (s.entered)
                        return;

                    s.spawned = false;
                }
                //Debug.Log("New room", this);
                _environmentManager.setCurrentEnvironment(myConnections.connectedTo);

                //unload previous room/s & connectors
                _environmentManager.unloadAssets();
            }
        }
    }
}

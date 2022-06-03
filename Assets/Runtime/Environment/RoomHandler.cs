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

        public Animator[] myDoor;

        [Inject]
        public void Construct(EnvironmentManager environmentManager, Animator[] animators, Collider prevDoor = null)
        {
            EnvironmentManager = environmentManager;
            myDoor = animators;
            previousDoor = prevDoor;
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
            foreach (Animator anim in myDoor)
                anim.SetBool("isOpen", false);

            if(myDoor.Length > 0)
                StartCoroutine(unloadAssets());            
        }

        private IEnumerator unloadAssets()
        {
            while (!myDoor[0].GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Default") &&
                !myDoor[0].GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Default"))
                yield return null;// new WaitForSeconds(myDoor[0].GetCurrentAnimatorStateInfo(0).normalizedTime);
            
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

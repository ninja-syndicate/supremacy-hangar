using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Environment
{
    public class DoorCollisionHandler : MonoBehaviour
    {
        [SerializeField]
        private Collider doorCollider;

        public void DisableDoorCollider()
        {
            doorCollider.enabled = false;
        }

        public void EnableDoorCollider()
        {
            doorCollider.enabled = true;
        }
    }
}
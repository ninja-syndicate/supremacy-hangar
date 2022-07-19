using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar.Runtime.Audio
{
    public class RoomToneSwap : MonoBehaviour
    {
        public AudioClip newClip;
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                RoomToneManager.instance.SwapAmbiance(newClip);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                RoomToneManager.instance.ReturnToDefault();
            }
        }
            

}
}

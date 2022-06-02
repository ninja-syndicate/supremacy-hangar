using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupremacyHangar
{
    [RequireComponent(typeof(Animator))]
    public class siloDoorTrigger : MonoBehaviour
    {
        public Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            _animator.SetBool("IsOpen", true);
        }

        private void OnTriggerExit(Collider other)
        {
            _animator.SetBool("IsOpen", false);
        }
    }
}

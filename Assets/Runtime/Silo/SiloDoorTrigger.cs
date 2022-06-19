using UnityEngine;

namespace SupremacyHangar.Runtime.Silo
{
    [RequireComponent(typeof(Animator))]
    public class SiloDoorTrigger : MonoBehaviour
    {
        private Animator _animator;

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

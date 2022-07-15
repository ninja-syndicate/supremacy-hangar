using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    //TODO: Refactor this
    [RequireComponent(typeof(AudioSource))]
    public class CrateDoorController : MonoBehaviour
    {
        private SignalBus _bus;
        
        private bool _subscribed;

        [SerializeField] private Animator _animator;
        private AudioSource myAudioSource;
        [SerializeField] private AudioClip OpenCrateClip;
        [SerializeField] private AudioClip CloseCrateClip;

        [Inject]
        public void Contruct(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
            myAudioSource = gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<OpenCrateSignal>(OpenCrate);
            _subscribed = true;
        }

        private void OpenCrate()
        {
            _animator.SetBool("open", true);
            myAudioSource.clip = OpenCrateClip;
            myAudioSource.Play();
        }
    }
}

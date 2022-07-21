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

        [SerializeField] private CrateMusicHandler crateMusicHandler;
        [SerializeField] private Animator _animator;
        private AnimatorClipInfo[] _animatorClipInfo;
        private AudioSource myAudioSource;
        [SerializeField] private AudioClip OpenCrateClip;
        [SerializeField] private AudioClip CloseCrateClip;

        private bool stingerPlayedYet;

        [Inject]
        public void Contruct(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        void Start()
        {
            myAudioSource = gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
            stingerPlayedYet = false;

            _animatorClipInfo = this._animator.GetCurrentAnimatorClipInfo(0);
        }

        void Update()
        {
            _animatorClipInfo = _animator.GetCurrentAnimatorClipInfo(0);
            
            if (_animatorClipInfo[0].clip.name == "Open" && !stingerPlayedYet)
            {
                //TriggerUponAnimationEnd(5);
                stingerPlayedYet = true;
            }
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

        private void TriggerUponAnimationEnd(int temp)
        {
            crateMusicHandler.PlayCrateMusic(temp);
        }

        private void OpenCrate()
        {
            _animator.SetBool("open", true);
            myAudioSource.clip = OpenCrateClip;
            myAudioSource.Play();
        }
    }
}

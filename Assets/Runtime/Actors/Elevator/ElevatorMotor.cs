using System;
using SupremacyHangar.Runtime.Actors.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using UnityMath = Unity.Mathematics;

namespace SupremacyHangar.Runtime.Actors.Elevator
{
    public class ElevatorMotor : MonoBehaviour
    {
        public int CurrentStop { get; private set; }
        
        public event Action<int> OnStopChanged;
        
        private Vector3[] myStops;
        [FormerlySerializedAs("velocity"), SerializeField] protected float speed;

        private bool playerPresent;
        private FirstPersonController playerController;
        private int myNextStop;
        private UnityMath.float3 myCurrentPos;
        private bool elevatorStartReady;
        private bool elevatorLoopReady;

        [SerializeField] private AudioClip start;
        [SerializeField] private AudioClip loop;
        [SerializeField] private AudioClip end;

        protected SignalBus _bus;
        protected bool _subscribed;

        private bool isPaused = false;

        [SerializeField] private AudioSource myAudioSource;

        public void Awake()
        {
            SetupAudio();
        }

        private void SetupAudio()
        {
            if (myAudioSource != null) return;
            if (TryGetComponent(out myAudioSource)) return;
            Debug.LogError("Audio Source is null", this);
        }

        [Inject]
        public void Inject(SignalBus bus)
        {
            _bus = bus;
            elevatorStartReady = false;
            SubscribeToSignal();
        }

        public void OnEnable()
        {
            SubscribeToSignal();
        }

        public virtual void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<ResumeGameSignal>(TogglePause);
            _bus.Unsubscribe<PauseGameSignal>(TogglePause);

            _subscribed = false;
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            switch (isPaused)
            {
                case true when myAudioSource.clip == loop:
                    myAudioSource.Pause();
                    break;
                case false when myAudioSource.clip == loop:
                    myAudioSource.Play();
                    break;
            }
        }

        protected virtual void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<ResumeGameSignal>(TogglePause);
            _bus.Subscribe<PauseGameSignal>(TogglePause);

            _subscribed = true;
        }

        protected void InitializeMotor(Vector3[] newStops, int initialStop)
        {
            myStops = newStops;
            myCurrentPos = newStops[initialStop];
            CurrentStop = initialStop;
            myNextStop = CurrentStop;
            OnStopChanged?.Invoke(CurrentStop);

            if (TryGetComponent(out myAudioSource)) return;

            Debug.LogError("Cannot find and set audio source");
            enabled = false;
        }

        public virtual void Update()
        {
            if(isPaused) return;
            if (CurrentStop != -1) return;
            Move(Time.deltaTime);

            //Elevator starting to move but the Start sound isn't playing
            if (elevatorStartReady)
            {
                myAudioSource.loop = false;
                myAudioSource.clip = start;
                myAudioSource.Play();
                elevatorStartReady = false;
            }
            //Elevator now moving and start sound is playing. Is the start sound done? If so play the loop.
            else if (myAudioSource.clip == start && !myAudioSource.isPlaying && !elevatorStartReady)
            {
                myAudioSource.loop = true;
                myAudioSource.clip = loop;
                myAudioSource.Play();
            }
            
            if (UnityMath.math.distancesq(myCurrentPos, myStops[myNextStop]) < Mathf.Epsilon)
            {
                //TODO: refactor into a elevator stopping func.
                CurrentStop = myNextStop;
                myAudioSource.Stop();
                myAudioSource.loop = false;
                myAudioSource.clip = end;
                elevatorStartReady = false;
                myAudioSource.Play();
                OnStopChanged?.Invoke(myNextStop);
            }
        }

        public virtual void MoveToNextStop()
        {
            if (UnityMath.math.distancesq(myCurrentPos, myStops[myNextStop]) > Mathf.Epsilon) return;
            myNextStop++;
            CurrentStop = -1;
            OnStopChanged?.Invoke(CurrentStop);
            if (myNextStop >= myStops.Length) myNextStop = 0;

            elevatorStartReady = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out playerController)) return;
            playerPresent = true;
        }

        public void OnTriggerExit(Collider other)
        {
            playerController.PlatformVelocity = UnityMath.float3.zero;
            playerPresent = false;
            playerController = null;
        }

        void Move(float deltaTime)
        {
            // get our current, and next move vectors;
            UnityMath.float3 desiredPos = myStops[myNextStop];
            // get the movement vector
            UnityMath.float3 nextMove = desiredPos - myCurrentPos;

            // we use square distance as it's quicker to calculate
            float sqDistanceToDesired = UnityMath.math.lengthsq(nextMove);
            // max distance we can move in this frame (squared to easily compare with above)
            float distanceThisFrame = speed * deltaTime;
            float sqDistanceThisFrame = distanceThisFrame * distanceThisFrame;

            // if we can move more than the max distance, it's easy.
            if (sqDistanceThisFrame <= sqDistanceToDesired)
            {
                UnityMath.float3 thisMove = UnityMath.math.normalize(nextMove) * distanceThisFrame;
                myCurrentPos += thisMove;
                if (playerPresent) playerController.PlatformVelocity = thisMove;
                transform.localPosition = myCurrentPos;
                return;
            }

            //otherwise we move straight to the stop.
            if (playerPresent) playerController.PlatformVelocity = desiredPos - myCurrentPos;
            transform.localPosition = desiredPos;
            myCurrentPos = desiredPos;
        }
    }
}

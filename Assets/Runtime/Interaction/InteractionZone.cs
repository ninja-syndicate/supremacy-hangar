using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Silo;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public class InteractionZone : MonoBehaviour
    {
        private bool hasCollided = false;

        [SerializeField]
        private Text labelText;

        [SerializeField]
        private SiloPositioner spawner;

        [Inject]
        private AddressablesManager _addressablesManager;
        
        [Inject]
        private SiloContent[] _siloContent;

        [Inject]
        private SupremacyDictionary _supremacyDictionary;

        [SerializeField]
        private int siloIndex = 0;

        private SignalBus _bus;
        private bool _subscribed;

        [SerializeField]
        private InteractionType _interactionType;

        private InteractionSignalHandler _signalHandler;

        public float speed = 2f;
        private float maxHeight;
        public float minHeight;
        Vector3 moveDirection = Vector3.down; // *assuming your platform starts at the top

        private void Start()
        {
            maxHeight = transform.localPosition.y;
        }

        [Inject]
        public void Constuct(SignalBus bus, InteractionSignalHandler interactionSignalHandler)
        {
            _bus = bus;
            _signalHandler = interactionSignalHandler;
            SubscribeToSignal();
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<InteractionSignal>(OnInteraction);
            _subscribed = true;
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _bus.Unsubscribe<InteractionSignal>(OnInteraction);
                _subscribed = false;
            }
        }

        private void OnInteraction(InteractionSignal signal)
        {
            if (hasCollided == false) return;
            Debug.Log("Interaction", this);
            //Check message then do corresponding task
            switch(signal.Type)
            {
                case InteractionType.Silo:
                    FillSilo();
                    break;
                case InteractionType.Elevator:
                    Elevator();
                    break;
                default:
                    Debug.LogError($"Unkown signal type {signal.Type}", this);
                    break;
            }
        }

        private void FillSilo()
        {
            if (_siloContent[siloIndex].type.Contains("mech"))
            {
                _addressablesManager.TargetMech = _supremacyDictionary.MechDictionary[_siloContent[siloIndex].chassisId];
                _addressablesManager.TargetSkin = _supremacyDictionary.AllSkinsDictionary[_siloContent[siloIndex].chassisId][_siloContent[siloIndex].skinId];
            }
            else
            {
                _addressablesManager.TargetMech = _supremacyDictionary.LootBoxDictionary[_siloContent[siloIndex].id];
                _addressablesManager.TargetSkin = null;
            }
            spawner.SpawnSilo();
        }

        private bool enableElevator = false;
        private void Elevator()
        {
            enableElevator = !enableElevator;
        }

        private void Update()
        {
            if (enableElevator)
                move();
        }

        private void move()
        {
            if (transform.localPosition.y >= maxHeight)
            {
                moveDirection = Vector3.down; 
                enableElevator = false;
            }
            else if (transform.localPosition.y <= minHeight)
            {
                moveDirection = Vector3.up;
                enableElevator = false;
            }

            //Todo make smoother
            transform.Translate(moveDirection * Time.deltaTime * speed);
        }

        private void OnTriggerEnter(Collider other)
        {
            _signalHandler.ChangePlayerInteraction(_interactionType);
            hasCollided = true;
            labelText.enabled = true;
        }

        private void OnTriggerExit(Collider other)
        {
            labelText.enabled = false;
            hasCollided = false;
        }
    }
}
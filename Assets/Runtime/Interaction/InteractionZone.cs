using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Silo;
using System;
using Runtime.Addressables;
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
        private SiloItem[] _siloContent;

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
        [SerializeField]
        private float minHeight;
        Vector3 moveDirection = Vector3.down; // *assuming your platform starts at the top

        private bool empty = false;
        private bool enableElevator = false;

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
            switch(_siloContent[siloIndex])
            {
                case Mech mech:
                    empty = false;
                    _addressablesManager.TargetMech = _supremacyDictionary.MechDictionary[mech.mech_id];
                    _addressablesManager.TargetSkin = _supremacyDictionary.AllSkinsDictionary[mech.mech_id][mech.skin_id];
                    break;
                case MysteryBox box:
                    empty = false;
                    _addressablesManager.TargetMech = _supremacyDictionary.LootBoxDictionary[box.ownership_id];
                    _addressablesManager.TargetSkin = null;
                    break;
                default:
                    Debug.LogWarning($"Unexpected type of {_siloContent[siloIndex].Type} making silo empty");
                    empty = true;
                    break;
            }

            if(!empty) spawner.SpawnSilo();
        }

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
            Vector3 direction = moveDirection * Time.deltaTime * speed;

            //Todo make smoother
            transform.Translate(direction);
            MovePlayer(direction);

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
        }

        private void MovePlayer(Vector3 direction)
        {
            _playerController.enabled = false;

            _player.transform.Translate(direction);

            _playerController.enabled = true;
        }

        private CharacterController _playerController;
        private GameObject _player;
        private void OnTriggerEnter(Collider other)
        {
            _player = other.gameObject;
            _playerController = _player.GetComponent<CharacterController>();

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
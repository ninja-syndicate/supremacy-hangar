using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Silo;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Interaction
{
    public class OldInteractionZone : MonoBehaviour
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
        private bool containsPlayer = false;

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
                    _addressablesManager.TargetMech = mech.MechChassisDetails.MechReference;
                    _addressablesManager.TargetSkin = mech.MechSkinDetails.SkinReference;
                    break;
                case MysteryBox box:
                    empty = false;
                    _addressablesManager.TargetMech = box.MysteryCrateDetails.MysteryCrateReference;
                    _addressablesManager.TargetSkin = null;
                    break;
                default:
                    Debug.LogWarning($"Unexpected type of {_siloContent[siloIndex].Type} making silo empty");
                    empty = true;
                    break;
            }

            if(!empty) spawner.PrepareSilo();
        }

        private void Elevator()
        {
            enableElevator = !enableElevator;
        }

        public void FixedUpdate()
        {
            if (enableElevator) ElevatorMove();

        }

        private Vector3 lastMove = Vector3.zero;

        private void ElevatorMove()
        {
            lastMove = moveDirection * (Time.deltaTime * speed);
            var currentPosition = transform.localPosition;
            var nextPosition = currentPosition + lastMove;
            if (nextPosition.y >= maxHeight)
            {
                var proportion = (maxHeight - currentPosition.y) / lastMove.y;
                lastMove *= proportion;
                nextPosition = currentPosition + lastMove;
                moveDirection = Vector3.down;
                enableElevator = false;
            } else if (nextPosition.y <= minHeight)
            {
                var proportion = (minHeight - currentPosition.y) / lastMove.y;
                lastMove *= proportion;
                nextPosition = currentPosition + lastMove;
                moveDirection = Vector3.up;
                enableElevator = false;
            }

            transform.localPosition = nextPosition;
            if (containsPlayer)  _playerController.Move(lastMove);
        }

        private CharacterController _playerController;
        private GameObject _player;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out _playerController)) return;
            _player = other.gameObject;
            _signalHandler.ChangePlayerInteraction(_interactionType);
            containsPlayer = true;
            hasCollided = true;
            if (labelText != null) labelText.enabled = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject != _player) return;
            _player = null;
            _playerController = null;
            if (labelText != null) labelText.enabled = false;
            hasCollided = false;
            containsPlayer = false;
        }
    }
}
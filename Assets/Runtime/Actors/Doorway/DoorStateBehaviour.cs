using SupremacyHangar.Runtime.Environment;
using UnityEngine;

namespace SupremacyHangar.Runtime.Actors.Doorway
{
    [SharedBetweenAnimators]
    public class DoorStateBehaviour : StateMachineBehaviour
    {
        private enum DoorColliderOptions
        {
            None,
            Enable,
            Disable
        }

        [SerializeField] private DoorColliderOptions doorColliderOption;
        [SerializeField] private bool updateRoom;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (updateRoom) UpdateRoomInTracker(animator);
            if (doorColliderOption != DoorColliderOptions.None)
            {
                HandleDoorCollider(animator, doorColliderOption);
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (updateRoom) MarkRoomNotNewInTracker(animator);
        }

        private void HandleDoorCollider(Animator animator, DoorColliderOptions option)
        {
            if (!animator.gameObject.TryGetComponent(out DoorCollisionHandler doorHandler))
            {
                Debug.LogError("Could not find Door Collision Handler!", animator.gameObject);
                return;
            }

            switch (option)
            {
                case DoorColliderOptions.Disable:
                    doorHandler.DisableDoorCollider();
                    break;
                case DoorColliderOptions.Enable:
                    doorHandler.EnableDoorCollider();
                    break;
                default:
                    Debug.LogError($"Unknown Door Collider Option: {option}", animator.gameObject);
                    break;
            }
        }

        public void UpdateRoomInTracker(Animator animator)
        {
            if (!animator.gameObject.TryGetComponent(out RoomTracker roomTracker))
            {
                Debug.LogError("Could not find Room Tracker!", animator.gameObject);
            }

            roomTracker.UpdateRoom();
            
        }

        public void MarkRoomNotNewInTracker(Animator animator)
        {
            if (!animator.gameObject.TryGetComponent(out RoomTracker roomTracker))
            {
                Debug.LogError("Could not find Room Tracker!", animator.gameObject);
            }

            roomTracker.NotNewRoom();
            
        }        
        
    }
}

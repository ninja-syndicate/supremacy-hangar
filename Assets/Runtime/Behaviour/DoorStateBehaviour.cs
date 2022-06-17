using SupremacyHangar.Runtime.Environment;
using UnityEngine;

namespace SupremacyHangar.Runtime.Behaviour
{
    [SharedBetweenAnimators]
    public class DoorStateBehaviour : StateMachineBehaviour
    {   
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.gameObject.TryGetComponent(out RoomTracker roomTracker))
            {
                roomTracker.UpdateRoom();
            }
            else
            {
                Debug.LogError("Could not find Room Tracker!", animator.gameObject);
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.gameObject.TryGetComponent(out RoomTracker roomTracker))
            {
                roomTracker.NotNewRoom();
            }
            else
            {
                Debug.LogError("Could not find Room Tracker!", animator.gameObject);
            }
        }
    }
}

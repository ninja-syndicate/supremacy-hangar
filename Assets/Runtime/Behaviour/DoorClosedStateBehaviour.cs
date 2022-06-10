using SupremacyHangar.Runtime.Environment;
using UnityEngine;

namespace SupremacyHangar
{
    [SharedBetweenAnimators]
    public class DoorClosedStateBehaviour : StateMachineBehaviour
    {   
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.gameObject.TryGetComponent(out DoorCollisionHandler doorHandler))
            {
                doorHandler.EnableDoorCollider();
            }
            else
            {
                Debug.LogError("Could not find Door Collision Handler!", this);
            }
        }
    }
}

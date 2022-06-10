using SupremacyHangar.Runtime.Environment;
using UnityEngine;

namespace SupremacyHangar
{
    [SharedBetweenAnimators]
    public class DoorOpenStateBehaviour : StateMachineBehaviour
    {   
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.gameObject.TryGetComponent(out DoorCollisionHandler doorHandler))
            {
                doorHandler.DisableDoorCollider();
            }
            else
            {
                Debug.LogError("Could not find Door Collider Handler!", this);
            }
        }
    }
}

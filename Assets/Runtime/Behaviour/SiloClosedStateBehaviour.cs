using SupremacyHangar.Runtime.Silo;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Behaviour
{
    public class SiloClosedStateBehaviour : StateMachineBehaviour
    {
        private SiloSignalHandler _siloSignalHandler;
        private bool firstSpawn = true;

        [Inject]
        public void Constuct(SiloSignalHandler siloSignalHandler)
        {
            _siloSignalHandler = siloSignalHandler;
        }
                
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(!firstSpawn)
                _siloSignalHandler.SiloUnloaded();
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            firstSpawn = false;
        }
    }
}

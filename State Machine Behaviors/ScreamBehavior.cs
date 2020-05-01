using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // When we start screaming, we should set the navmesh agent speed to 0
        AIStateMachine stateMachine = animator.GetComponent<AIStateMachine>();

        // We only set the speed of the navmesh to 0 when we don't use root position,
        // otherwise, the navmesh isn't going to be giving any desired velocity and the character will be fully driven by the walk/run animation root rotation
        // causing him to just run in a straight line.
        if (stateMachine.currentState != null && !stateMachine.currentState.useRootPosition)
        {
            stateMachine.navMeshAgent.speed = 0f;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //AIStateMachine stateMachine = animator.GetComponent<AIStateMachine>();
        //stateMachine.navMeshAgent.speed = 0f;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AIStatePursuit statePursuit = animator.GetComponent<AIStatePursuit>();
        if (!statePursuit.useRootPosition)
            statePursuit.ResetSpeed();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationChangeBehavior : StateMachineBehaviour
{
    [SerializeField] private float _individualAnimationLength = 10f;
    [SerializeField] private string _parameter = null;
    [SerializeField] private List<float> _values = new List<float>();

    private int _currentAnimationIndex = 0;
    private float _timer = 0f;

    private AIStateMachine _stateMachine = null;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _currentAnimationIndex = 0;
        _timer = 0f;

        if (_values.Count > 0) animator.SetFloat(_parameter, _values[0]);

        _stateMachine = animator.GetComponent<AIStateMachine>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timer += Time.deltaTime;
        if (_timer >= _individualAnimationLength)
        {
            float previousValue = _values[_currentAnimationIndex];

            _timer = 0f;
            _currentAnimationIndex++;
            if (_currentAnimationIndex >= _values.Count) _currentAnimationIndex = 0;

            if (_stateMachine != null)
                _stateMachine.StartChangeAnimationCoroutine(_values[_currentAnimationIndex], animator, previousValue, _parameter);

            animator.SetFloat(_parameter, _values[_currentAnimationIndex]);
        }
    }


    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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

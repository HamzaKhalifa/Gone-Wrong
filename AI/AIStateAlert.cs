using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateAlert : AIState
{
    [SerializeField] private float _alertDelay = 10f;
    [SerializeField] private float _turningDelay = 3f;
    [SerializeField] private bool _alertAnimation = false;

    private float _alertTimer = 0f;
    private float _turningTimer = 0f;

    // Animator Hashes
    private int _alertHash = Animator.StringToHash("Idle");

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        if (_stateMachine != null)
        {
            if (_stateMachine.animator != null)
            {
                _stateMachine.animator.SetInteger(_stateMachine.speedhHash, 0);
            }
        }
        _alertTimer = 0f;
        // We set the turning timer to turning delay in order to turn immediately after we enter this state
        _turningTimer = _turningDelay;

        // If we have an alert animation, then we won't be turning the zombie in place, we are going to play the alert animation instead
        if (_alertAnimation && _stateMachine.animator != null)
        {
            _stateMachine.animator.SetFloat(_alertHash, 1);
        }
    }

    public override AIStateType OnUpdate()
    {
        AIStateType sharedUpdateStateType = base.SharedUpdate(AIStateType.Alert);
        if (sharedUpdateStateType != AIStateType.Alert)
        {
            return sharedUpdateStateType;
        }

        _alertTimer += Time.deltaTime;
        if (_alertTimer >= _alertDelay)
        {
            _alertTimer = 0f;
            return AIStateType.Idle;
        }

        if (!_stateMachine.isCrawling && !_alertAnimation)
        {
            _turningTimer += Time.deltaTime;
            if (_turningTimer >= _turningDelay)
            {
                _turningTimer = 0f;
                if (_stateMachine.animator != null)
                {
                    int whichTurn = Random.Range(0, 2);
                    _stateMachine.animator.SetBool(whichTurn == 0 ? _stateMachine.turningRightHash : _stateMachine.turningLeftHash, true);
                    _stateMachine.animator.SetBool(whichTurn == 0 ? _stateMachine.turningLeftHash : _stateMachine.turningRightHash, false);
                }
            }
        }

        return AIStateType.Alert;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        if (_alertAnimation && _stateMachine.animator != null)
        {
            _stateMachine.animator.SetFloat(_alertHash, 0);
        }

        _stateMachine.animator.SetBool(_stateMachine.turningRightHash, false);
        _stateMachine.animator.SetBool(_stateMachine.turningLeftHash, false);
    }
}

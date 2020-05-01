using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateIdle : AIState
{
    [SerializeField] [Range(1, 10)] float _idleTime = 5f;

    private float _idleTimer = 0f;

    public float idleTime { get { return _idleTime; } }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        _idleTimer = 0f;

        if (_stateMachine != null)
        {
            if(_stateMachine.animator != null)
                _stateMachine.animator.SetInteger(_stateMachine.speedhHash, 0);
        }
    }

    public override AIStateType OnUpdate()
    {
        AIStateType sharedUpdateStateType = base.SharedUpdate(AIStateType.Idle);

        if (sharedUpdateStateType != AIStateType.Idle)
        {
            return sharedUpdateStateType;
        }

        _idleTimer += Time.deltaTime;
        if (_idleTimer > _idleTime)
        {
            _idleTimer = 0f;

            if (_stateMachine.IsStateEnabled(AIStateType.Patrol))
            {
                return AIStateType.Patrol;
            } else
            {
                return AIStateType.Idle;
            }
        }

        return AIStateType.Idle;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        _idleTimer = 0f;
    }

    public void PlayIdleSound()
    {
        if (_stateMachine.mouthAudioSource != null && _sounds.Count > 0)
        {
            _stateMachine.mouthAudioSource.Stop();
            _stateMachine.mouthAudioSource.clip = _sounds[Random.Range(0, _sounds.Count)];
            _stateMachine.mouthAudioSource.Play();
        }
    }
}

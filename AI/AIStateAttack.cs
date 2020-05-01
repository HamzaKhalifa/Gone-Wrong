using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateAttack : AIState
{
    [SerializeField] private bool _intersectAttack = true;
    [SerializeField] List<AudioClip> _weaponSounds = new List<AudioClip>();
    [SerializeField] private bool _lookAtThePlayer = true;

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        if (_stateMachine != null)
        {
            // Set the attack looping animation
            if (_stateMachine.animator != null)
            {
                // Also set a random attack at the beginning:
                _stateMachine.isAttacking = true;

                // Now set the attack loop
                _stateMachine.animator.SetBool(_stateMachine.isAttackingHash, true);
            }

            // Set the navmesh destination to null
            if (_stateMachine.navMeshAgent != null)
            {
                _stateMachine.navMeshAgent.ResetPath();
            }
        }
    }

    public override AIStateType OnUpdate()
    {
        AIStateType sharedUpdateStateType = base.SharedUpdate(AIStateType.Attacking);
        if (sharedUpdateStateType != AIStateType.Attacking)
        {
            return sharedUpdateStateType;
        }

        if (_stateMachine.currentTarget != null)
        {
            float distanceToTarget = (_stateMachine.currentTarget.position - transform.position).magnitude;
            // If we are far from the target, we return to pursuing
            if (distanceToTarget > _stateMachine.stoppingDistance)
            {
                if(_intersectAttack || !_intersectAttack && !_stateMachine.isAttacking)
                    return AIStateType.Pursuit;
            }

            // We keep lerping the zombie towards the character position
            if (_lookAtThePlayer)
            {
                Quaternion destinationRotation = Quaternion.LookRotation(_stateMachine.currentTarget.position - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, destinationRotation, 2 * Time.deltaTime);
            }
        } // If there is no target
        else
        {
            return AIStateType.Idle;
        }

        return AIStateType.Attacking;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        if (_stateMachine != null && _stateMachine.animator != null)
        {
            _stateMachine.animator.SetBool(_stateMachine.isAttackingHash, false);
        }
    }

    public void PlayAttackSound()
    {
        if (_stateMachine.mouthAudioSource != null && _sounds.Count > 0)
        {
            _stateMachine.mouthAudioSource.Stop();
            _stateMachine.mouthAudioSource.clip = _sounds[Random.Range(0, _sounds.Count)];
            _stateMachine.mouthAudioSource.Play();
        }

        if (GoneWrong.AudioManager.instance != null && _weaponSounds.Count > 0)
        {
            AudioClip clip = _weaponSounds[Random.Range(0, _weaponSounds.Count)];
            if (clip != null)
                GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 1, 1, transform.position);
        }
    }

    public void SetAttacking(int isAttacking)
    {
        _stateMachine.isAttacking = isAttacking != 0 ;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AIStatePursuit : AIState
{
    [SerializeField] private bool _TurnToPlayerAfterDone = false;
    [SerializeField] private float _screamAnimationDelay = 2f;

    private float _newDestinationDelay = 1f;
    private float _newDestinationTimer = 0f;

    // Animator hashes
    private int _screamHash = Animator.StringToHash("Scream");
    private int _agonizeHash = Animator.StringToHash("Agonize");
    private int _struggleHash = Animator.StringToHash("Struggle");

    private float _screamAnimationTimer = 0f;
    private float _timeSpentNotMoving = 0f;
    private float _initialSpeed = 0f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        _initialSpeed = _stateMachine.navMeshAgent.speed;
        if (!_useRootPosition)
        {
            _stateMachine.navMeshAgent.speed = _initialSpeed * 2;
        }

        if (_stateMachine.navMeshAgent != null && _stateMachine.currentTarget != null)
        {
            // Setting the new destination to the target
            try
            {
                _stateMachine.navMeshAgent.SetDestination(_stateMachine.currentTarget.position);
            } catch (Exception e) {
                Debug.Log(e);
            }
        }

        _newDestinationTimer = 0f;

        // Before we start the pursuit, we scream first:
        if (!_stateMachine.takingDamage && _stateMachine.currentTarget.type == TargetType.Player && _stateMachine.canScream && _stateMachine.screamer)
        {
            // If we do not use root position, we should set the speed to 0 when screaming
            if (!_useRootPosition)
                _stateMachine.navMeshAgent.speed = 0;

            if (_useRootRotation)
                _stateMachine.transform.rotation = Quaternion.LookRotation(_stateMachine.currentTarget.position - _stateMachine.transform.position);

            _stateMachine.animator.SetTrigger(_screamHash);

            _screamAnimationTimer = 0f;

            _stateMachine.ResetScreamTimer();
        }

        _timeSpentNotMoving = 0f;
    }

    public override AIStateType OnUpdate()
    {
        if (_screamAnimationDelay > 0 && _screamAnimationTimer < _screamAnimationDelay && _stateMachine.screamer)
        {
            _screamAnimationTimer += Time.deltaTime;

            return AIStateType.Pursuit;
        }

        AIStateType sharedUpdateStateType = base.SharedUpdate(AIStateType.Pursuit);
        if (sharedUpdateStateType != AIStateType.Pursuit)
        {
            return sharedUpdateStateType;
        }

        if (_stateMachine.currentTarget != null)
        {

            // If we have a path, we just go for it
            if (_stateMachine.navMeshAgent.hasPath && !_stateMachine.navMeshAgent.isPathStale)
            {
                _stateMachine.navMeshAgent.speed = _initialSpeed * 2;
                /*if (!_useRootPosition && _stateMachine.takingDamage)
                {
                    _stateMachine.navMeshAgent.speed = 0f;
                }*/

                _stateMachine.animator.SetInteger(_stateMachine.speedhHash, 2);

                // We keep lerping the zombie towards his desired velocity if the distance between him and the player is superior to the minial distance before we start focusing on the player
                if ((_stateMachine.transform.position - _stateMachine.currentTarget.transform.position).magnitude > 4)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_stateMachine.navMeshAgent.desiredVelocity), 5 * Time.deltaTime);
                else
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_stateMachine.currentTarget.transform.position - _stateMachine.transform.position).normalized, 5 * Time.deltaTime);
                }

                // Test if the navmeshagent isn't moving although it has a path and the path isn't stale
                if (_stateMachine.navMeshAgent.velocity.magnitude < 3)
                {
                    _timeSpentNotMoving += Time.deltaTime;
                    if (_timeSpentNotMoving >= 2)
                    {
                        _timeSpentNotMoving = 0f;
                        //_stateMachine.animator.SetTrigger(_struggleHash);
                        //_stateMachine.animator.SetBool(_agonizeHash, true);
                    }
                } else
                {
                    //_stateMachine.animator.ResetTrigger(_struggleHash);
                    _stateMachine.animator.SetBool(_agonizeHash, false);
                    _timeSpentNotMoving = 0f;
                }
            }
            else
            {
                _stateMachine.animator.SetInteger(_stateMachine.speedhHash, 0);
                _timeSpentNotMoving = 0f;
                // If we are setting the speed hash of animator to 0, we also need to set the navmesh agent speed to 0 in case we are controlling the nav mesh manually (no root position)
                if (!_useRootPosition)
                {
                    _stateMachine.navMeshAgent.speed = 0f;
                }
            }

            /*if (_stateMachine.navMeshAgent.isPathStale)
            {
                _stateMachine.animator.SetBool(_agonizeHash, true);
            } else
            {
                _stateMachine.animator.SetBool(_agonizeHash, false);
            }*/


            // Refreshing path periodically:
            _newDestinationTimer += Time.deltaTime;
            if (_newDestinationTimer >= _newDestinationDelay || _stateMachine.navMeshAgent.isPathStale)
            {
                _newDestinationTimer = 0f;
                if (_stateMachine.navMeshAgent != null) {
                    try
                    {
                        _stateMachine.navMeshAgent.SetDestination(_stateMachine.currentTarget.position);
                    } catch(Exception e) { Debug.Log(e.Message); }
                }
            }


            // Check if distance between zombie and target is inferior to a certain threshold
            if ((_stateMachine.currentTarget.position - transform.position).magnitude <= _stateMachine.stoppingDistance) {
                // If we are going after the player or a vehicle
                if (_stateMachine.currentTarget.type == TargetType.Player) {
                    // If we are going after the player and the distance to the player is inferior to stopping distance, then we start attacking
                    if (_stateMachine.currentTarget.transform.GetComponent<GoneWrong.Player>() != null
                        && (_stateMachine.currentTarget.transform.GetComponent<GoneWrong.Player>().transform.position - transform.position).magnitude <= _stateMachine.stoppingDistance)
                    {
                        return AIStateType.Attacking;
                    } else if (_stateMachine.currentTarget.transform.GetComponent<Vehicle>() != null
                        && (_stateMachine.currentTarget.transform.GetComponent<Vehicle>().transform.position - transform.position).magnitude <= _stateMachine.stoppingDistance)
                    {
                        return AIStateType.Attacking;
                    }
                    // If we are close to the target but nothing in sight, we get into alert mode
                    else
                    {
                        _stateMachine.currentTarget = null;
                        return AIStateType.Alert;
                    }
                }

                // If we are going after an audio source
                if (_stateMachine.currentTarget.type == TargetType.Audio)
                {
                    // We still need to set the current target to null because the zombie could still have a path set to his navmeshagent
                    // but the distance between him and the target is inferior to the stopping distance 
                    _stateMachine.currentTarget = null;

                    if (_TurnToPlayerAfterDone)
                    {
                        _stateMachine.transform.rotation = Quaternion.LookRotation(GoneWrong.Player.instance.transform.position - _stateMachine.transform.position);
                    }

                    return AIStateType.Alert;
                }
            }

            return AIStateType.Pursuit;
        } else
        {
            // Only go back to alert mode when there is no path assigned
            if (!_stateMachine.navMeshAgent.hasPath && !_stateMachine.navMeshAgent.pathPending)
                return AIStateType.Alert;
            else return AIStateType.Pursuit;
        }
    }

    public override void OnStateExit()
    {
        _stateMachine.animator.SetBool(_struggleHash, false);
        _stateMachine.animator.SetBool(_agonizeHash, false);

        if (!_useRootPosition)
        {
            _stateMachine.navMeshAgent.speed = _initialSpeed;
        }

        if (_stateMachine.IsAgentOnNavMesh(_stateMachine.gameObject) && _stateMachine.navMeshAgent.enabled)
            _stateMachine.navMeshAgent.ResetPath();

        base.OnStateExit();
    }

    public void ResetSpeed()
    {
        _stateMachine.navMeshAgent.speed = _initialSpeed * 2;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIStatePatrol : AIState
{
    [SerializeField] NavigationPath _navigationPath = null;
    [SerializeField] bool _running = false;
    [SerializeField] bool _randomPatrol = false;
    [SerializeField] float _turnThresholdForRun = 20f;
    [SerializeField] float _turnThresholdForWalk = 50f;

    private int _currentDestination = -1;

    public NavigationPath navigationPath { set { _navigationPath = value; } }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
 
        // Setting the first destination
        if (_navigationPath != null
            && _stateMachine.navMeshAgent != null
            && _navigationPath.navigationPoints.Count > 0
            && _navigationPath.navigationPoints[0].transform != null
            && _currentDestination == -1
            )
        {
            _currentDestination = _randomPatrol ? UnityEngine.Random.Range(0, _navigationPath.navigationPoints.Count) : 0;
            _stateMachine.navMeshAgent.SetDestination(_navigationPath.navigationPoints[_currentDestination].position);
        }
    }

    public override AIStateType OnUpdate()
    {
        AIStateType sharedUpdateStateType = base.SharedUpdate(AIStateType.Patrol);
        if (sharedUpdateStateType != AIStateType.Patrol)
        {
            return sharedUpdateStateType;
        }

        float angle = Vector3.Angle(transform.forward, _stateMachine.navMeshAgent.steeringTarget - transform.position);
        float magnitude = _stateMachine.navMeshAgent.desiredVelocity.magnitude;
        bool turnRight = Vector3.Cross(transform.forward, _stateMachine.navMeshAgent.steeringTarget - transform.position).y > 0 ? true : false;

        float angleThreshold = _running ? _turnThresholdForRun : _turnThresholdForWalk;

        if (magnitude > 0 && angle < angleThreshold)
        {
            _stateMachine.animator.SetInteger(_stateMachine.speedhHash, _running ? 2 : 1);
        } else
        {
            _stateMachine.animator.SetInteger(_stateMachine.speedhHash, 0);
        }

        if (angle >= angleThreshold && !_stateMachine.isCrawling)
        {
            int turnHash = turnRight ? _stateMachine.turningRightHash : _stateMachine.turningLeftHash;
            _stateMachine.animator.SetBool(turnHash, true);
        } else
        {
            _stateMachine.animator.SetBool(_stateMachine.turningRightHash, false);
            _stateMachine.animator.SetBool(_stateMachine.turningLeftHash, false);

            // We are going to keep rotating the zombie manually (Ignore the root rotation)
            if (angle > 1f)
            {
                Quaternion destinationRotation = Quaternion.LookRotation(_stateMachine.navMeshAgent.steeringTarget - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, destinationRotation, 2 * Time.deltaTime);
            }
        }

        SetNextDestination();

        return AIStateType.Patrol;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        try
        {
            if (_stateMachine.navMeshAgent != null)
                _stateMachine.navMeshAgent.ResetPath();
        } catch (Exception e) { Debug.Log(e.Message); }

        _stateMachine.animator.SetBool(_stateMachine.turningRightHash, false);
        _stateMachine.animator.SetBool(_stateMachine.turningLeftHash, false);
    }

    private void SetNextDestination()
    {
        if (_navigationPath != null
            && _stateMachine.navMeshAgent != null
            && _navigationPath.navigationPoints.Count > 0
            && !_stateMachine.navMeshAgent.pathPending
            && _stateMachine.gameObject.activeSelf)
        {
            if (_stateMachine.navMeshAgent.enabled && _stateMachine.navMeshAgent.remainingDistance <= _stateMachine.navMeshAgent.stoppingDistance)
            {
                if (!_stateMachine.navMeshAgent.hasPath || _stateMachine.navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    if (_randomPatrol)
                    {
                        int nextDestination = _currentDestination;
                        do
                        {
                            nextDestination = UnityEngine.Random.Range(0, _navigationPath.navigationPoints.Count);
                        } while (nextDestination == _currentDestination);
                        _currentDestination = nextDestination;
                    }
                    else
                    {
                        _currentDestination++;
                        if (_currentDestination >= _navigationPath.navigationPoints.Count)
                        {
                            _currentDestination = 0;
                        }
                    }
                    _stateMachine.navMeshAgent.SetDestination(_navigationPath.navigationPoints[_currentDestination].position);
                }
            }
        }
    }

}

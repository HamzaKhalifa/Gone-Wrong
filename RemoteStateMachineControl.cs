using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteStateMachineControl : MonoBehaviour
{
    [SerializeField] List<Transform> _destinations = new List<Transform>();
    [SerializeField] AIStateMachine _stateMachine = null;
    [SerializeField] private float _distanceThreshold = 4f;

    private int _currentDestination = -1;
    private GoneWrong.Player player = null;
    private bool _reachedLastDestination = false;

    private void Start()
    {
        player = GoneWrong.Player.instance;
    }

    private void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = (_stateMachine.transform.position - player.transform.position).magnitude;
            if (distanceToPlayer < _distanceThreshold)
            {
                SetNextDestination();
            }
        }
    }

    public void SetNextDestination()
    {
        if (_stateMachine.navMeshAgent != null
            && !_stateMachine.navMeshAgent.pathPending
            && !_stateMachine.navMeshAgent.hasPath
            && _stateMachine.currentTarget == null
            && !_reachedLastDestination)
        {
            _currentDestination++;
            if (_currentDestination >= _destinations.Count) {
                _currentDestination = 0;
                // We do the metamorphosis if possible
                _stateMachine.MetamorphosisTrigger();

                // We are also going to prevent the player from moving
                GoneWrong.Player.instance.canMove = false;

                _reachedLastDestination = true;
                return; 
            }

            Target target = new Target();
            target.transform = _destinations[_currentDestination];
            target.type = TargetType.Audio;
            target.position = _destinations[_currentDestination].position;

            _stateMachine.currentTarget = target;
        }
    }

}

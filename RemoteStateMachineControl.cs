using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RemoteStateMachineControl : MonoBehaviour
{
    [SerializeField] private bool _disappear = false;
    [SerializeField] List<Transform> _destinations = new List<Transform>();
    [SerializeField] AIStateMachine _stateMachine = null;
    [SerializeField] private float _distanceThreshold = 4f;
    [SerializeField] private UnityEvent _reachedDestinationEvents = null;
    [SerializeField] private AudioClip _disappearanceSound = null;

    private int _currentDestination = -1;
    private GoneWrong.Player player = null;
    private bool _reachedLastDestination = false;

    private void Start()
    {
        player = GoneWrong.Player.instance;
    }

    private void Update()
    {
        if (_stateMachine == null || !_stateMachine.gameObject.activeSelf) return;

        if (player != null)
        {
            float distanceToPlayer = (_stateMachine.transform.position - player.transform.position).magnitude;
            if (distanceToPlayer < _distanceThreshold)
            {
                // If we disappear, directly change the stateMachine's place
                if (_disappear)
                {
                    Disappear();
                    
                } else 
                    SetNextDestination();
            }
        }
    }

    public void Disappear()
    {
        _currentDestination++;

        DisappearanceAnimation(_stateMachine.transform);

        if (_destinations.Count > _currentDestination)
        {
            _stateMachine.navMeshAgent.enabled = false;
            _stateMachine.transform.position = _destinations[_currentDestination].position;
            _stateMachine.transform.rotation = _destinations[_currentDestination].rotation;
            _stateMachine.navMeshAgent.enabled = true;
        }
        else
        {
            // We arrived at the last position
            _reachedLastDestination = true;

            if (_reachedDestinationEvents != null)
            {
                _reachedDestinationEvents.Invoke();
            }

            // We play the disappearance sound
            if (GoneWrong.AudioManager.instance != null && _disappearanceSound != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(_disappearanceSound, 1, 0, 0, _stateMachine.transform.position);
            }

            // So we destroy the stateMachine object
            if (_stateMachine != null)
                Destroy(_stateMachine.gameObject);
        }
    }

    public void DisappearanceAnimation(Transform where)
    {
        // We instantiate an animation
        if (EffectsManager.instance != null)
        {
            EffectsManager.instance.Fire(where);
            EffectsManager.instance.Explosion(where);
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

                if (_reachedDestinationEvents != null)
                {
                    _reachedDestinationEvents.Invoke();
                }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIRunToFeed : MonoBehaviour
{
    [SerializeField] private Transform _destination = null;

    private NavMeshAgent _agent = null;
    private Animator _animator = null;
    private AudioSource _audioSource = null;


    private bool _startedFeeding = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _agent.SetDestination(_destination.position);
    }

    private void Update()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _animator.SetBool("Feeding", true);
            if (!_startedFeeding)
            {
                _audioSource.Play();
                _audioSource.loop = true;

                _startedFeeding = true;
                _agent.enabled = false;
            }
        }

        Quaternion targetRotation = Quaternion.LookRotation(_agent.desiredVelocity);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        _agent.velocity = _animator.deltaPosition / Time.deltaTime;
    }

}

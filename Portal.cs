using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private AIStateMachine _stateMachine = null;
    [SerializeField] private float _triggerDistance = 4f;

    // Cache
    private ParticleSystem _particleSystem = null;
    private AudioSource _audioSource = null;
    private GoneWrong.Player _player = null;

    private bool _activated = false;

    private void Start()
    {
        // Getting cache variables
        _particleSystem = GetComponent<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
        _player = GoneWrong.Player.instance;

        _particleSystem.Stop();
        _audioSource.Stop();

        _activated = false;

        if (_stateMachine != null)
        {
            _stateMachine.transform.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_player != null && _stateMachine != null && _particleSystem != null)
        {
            float distance = (_player.transform.position - transform.position).magnitude;
            if (distance <= _triggerDistance && !_activated)
            {
                _activated = true;
                _particleSystem.Play();
                if (_audioSource != null)
                {
                    _audioSource.Play();
                }
                _stateMachine.transform.gameObject.SetActive(true);
            }
        }
    }
}

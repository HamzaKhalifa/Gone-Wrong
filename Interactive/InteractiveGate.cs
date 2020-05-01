using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGate : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _sounds = new List<AudioClip>();

    // Cached variables
    private Animator _animator = null;
    private AudioSource _audioSource = null;

    // Private
    private int _interactAnimatorHash = Animator.StringToHash("Interact");

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_interactAnimatorHash);
        }
    }

    public void HandleSound(bool play)
    {
        if (_sounds.Count > 0 && _audioSource != null)
        {
            if (play)
            {
                AudioClip clip = _sounds[Random.Range(0, _sounds.Count)];
                if (clip != null)
                {
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
            } else
            {
                _audioSource.Stop();
            }
        }
    }
}

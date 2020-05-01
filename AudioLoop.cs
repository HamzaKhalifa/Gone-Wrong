using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioLoop : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _sounds = new List<AudioClip>();

    // Cache variables
    private AudioSource _audioSource = null;

    private int _currentIndex = 0;
    private float _currentAudioLength = 0f;
    private float _timer = 0f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        if (_sounds.Count > 0 && _audioSource != null)
        {
            _audioSource.clip = _sounds[_currentIndex];
            _audioSource.Play();
            _currentAudioLength = _sounds[_currentIndex].length;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _currentAudioLength)
        {
            _timer = 0f;

            _currentIndex++;
            if (_currentIndex >= _sounds.Count) _currentIndex = 0;

            _audioSource.clip = _sounds[_currentIndex];
            _audioSource.Play();
            _currentAudioLength = _sounds[_currentIndex].length;
        }
    }
}

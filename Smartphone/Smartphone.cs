using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smartphone : MonoBehaviour
{
    public static Smartphone instance = null;

    [SerializeField] private AudioClip _lookSound = null;
    [SerializeField] private AudioClip _stopLookingSound = null;
    [SerializeField] private bool _looking = false;

    // Animator hashes
    private int _lookingHash = Animator.StringToHash("Looking");

    // Cache variables
    private Animator _animator = null;

    public bool looking { get { return _looking; } }

    private void Awake()
    {
        instance = this;

        _animator = GetComponent<Animator>();
        if (_animator != null)
        {
            _animator.SetBool(_lookingHash, _looking);
        }
    }

    public void Look(bool look)
    {
        _looking = look;

        // We play the look or stop looking sound
        AudioClip clip = _looking ? _lookSound : _stopLookingSound;
        if (GoneWrong.AudioManager.instance != null && clip != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 0);
        }

        // If we stop looking, we set the selected message of smartphoneUI to none
        if (SmartphoneUI.instance != null)
        {
            SmartphoneUI.instance.DeselectMessage();
        }

        // We play the look animation (or stop looking animation)
        _animator.SetBool(_lookingHash, _looking);
    }
}

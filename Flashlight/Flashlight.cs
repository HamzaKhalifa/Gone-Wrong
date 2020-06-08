using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public static Flashlight instance = null;

    [SerializeField] private AudioClip _lookSound = null;
    [SerializeField] private AudioClip _stopLookingSound = null;
    [SerializeField] private Transform _lightTransform = null;

    private int _lookingHash = Animator.StringToHash("Looking");
    private int _runningHash = Animator.StringToHash("Running");

    private bool _looking = false;
    private IEnumerator _activateLightCoroutine = null;

    // Cache variables
    private Animator _animator = null;

    public bool looking { get { return _looking; } }

    private void Awake()
    {
        instance = this;

        _animator = GetComponent<Animator>();

        Look(true);
    }

    public void Look(bool look)
    {
        if (look)
        {
            // Instead of activating the flashlight, we deactivate its children (we need this to still let the coroutine of this object to play)
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            Invoke("Deactivate", .3f);
        }

        if (_animator != null)
        {
            _looking = look;

            ActivateDeactivateLight(_looking, _looking ? .5f : 0f);

            // We play the look or stop looking sound
            AudioClip clip = _looking ? _lookSound : _stopLookingSound;
            if (GoneWrong.AudioManager.instance != null && clip != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 0);
            }

            _animator.SetBool(_lookingHash, look);
        }
    }

    public void Deactivate()
    {
        // Instead of activating the flashlight, we dedeactivate its children (we need this to still let the coroutine of this object to play)
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ActivateDeactivateLight(bool activate, float time)
    {
        // We don't deactivet the light when the flash light is looking
        if (!activate && _looking) return;

        if (_activateLightCoroutine != null) StopCoroutine(_activateLightCoroutine);
        _activateLightCoroutine = ActivateLightCouroutine(activate, time);
        StartCoroutine(_activateLightCoroutine);
    }

    public IEnumerator ActivateLightCouroutine(bool activate, float time)
    {   
        yield return new WaitForSeconds(time);

        if (_lightTransform != null)
        {
            _lightTransform.gameObject.SetActive(activate);
        }

        _activateLightCoroutine = null;
    }

    public void SetRun()
    {
        if (GoneWrong.Player.instance != null && _animator != null)
        {
            _animator.SetBool(_runningHash, GoneWrong.Player.instance.isRunning);
        } 
    }
}

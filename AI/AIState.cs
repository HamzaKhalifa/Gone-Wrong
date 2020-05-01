using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIState : MonoBehaviour
{
    [SerializeField] protected bool _useRootPosition = true;
    [SerializeField] private bool _useRootRotation = true;
    [SerializeField] protected List<AudioClip> _sounds = new List<AudioClip>();
    [SerializeField] protected List<AudioClip> _metamorphosisSounds = new List<AudioClip>();
    [SerializeField] bool _loopSound = true;
    [SerializeField] private bool _controlledSoundTime = false;
    [SerializeField] private float _soundDelay = 5f;
    [SerializeField] private List<string> _stateAnimationNames = new List<string>();


    protected AIStateMachine _stateMachine = null;
    private int _soundIndex = -1;
    private float _currentAudioCounter = 0f;

    public int soundIndex { set { _soundIndex = value; } }
    public bool useRootPosition { get { return _useRootPosition; } }

    public virtual void OnStateEnter()
    {
        // Reset the sound variables. Because we could leave them not equal to their initial values when we leave the state
        _soundIndex = -1;
    }

    public AIStateType SharedUpdate(AIStateType stateType)
    {
        // Managing sound
        if (_loopSound && _stateMachine.mouthAudioSource != null && _sounds.Count > 0 && _stateMachine.canPlayStateSound)
        {
            bool canPlaySound = false;

            // Only play the sound of the current at specific animations (mentioned via the inspector)
            foreach (string animationName in _stateAnimationNames)
            {
                if (_stateMachine.animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
                {
                    canPlaySound = true;
                    break;
                }
            }

            // If the current state is pursuit and the animator speed is 0, then we don't play the pursuit sound
            if (_stateMachine.currentStateType == AIStateType.Pursuit &&
                _stateMachine.animator.GetInteger(_stateMachine.speedhHash) == 0)
            {
                canPlaySound = false;
            }

            if (_soundIndex == -1)
            {

                if (canPlaySound)
                {
                    List<AudioClip> sounds = _sounds;
                    if (_stateMachine.isTransformed) sounds = _metamorphosisSounds;

                    _soundIndex = Random.Range(0, sounds.Count);
                    _stateMachine.mouthAudioSource.clip = sounds[_soundIndex];
                    _stateMachine.mouthAudioSource.Play();

                    _currentAudioCounter = 0f;
                } else
                {
                    _stateMachine.mouthAudioSource.clip = null;
                    _stateMachine.mouthAudioSource.Stop();
                }
            } else
            {
                _currentAudioCounter += Time.deltaTime;
                if (_currentAudioCounter >= _stateMachine.mouthAudioSource.clip.length && !_controlledSoundTime
                    || _currentAudioCounter >= _soundDelay && _controlledSoundTime
                    // If we are metamorphosed, we don't wait till the previous sound ends, we change rapidly between the sounds
                    || _currentAudioCounter >= _stateMachine.mouthAudioSource.clip.length - 1f && _stateMachine.isTransformed)
                {
                    _currentAudioCounter = 0f;
                    _stateMachine.mouthAudioSource.Stop();
                    _stateMachine.mouthAudioSource.clip = null;
                    _soundIndex = -1;
                }
            }
        }

        // Check if we saw a target
        if(_stateMachine.currentTarget != null
            && _stateMachine.currentStateType != AIStateType.Attacking
            && _stateMachine.currentStateType != AIStateType.Pursuit)
        {
            return AIStateType.Pursuit;
        }

        return stateType;
    }

    public virtual AIStateType OnUpdate() {
        return AIStateType.None;
    }

    public void OnAnimatorUpdated()
    {
        if (_stateMachine.navMeshAgent != null) { 
            if (_useRootPosition)
            {
                _stateMachine.navMeshAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime;
            }

            if (_useRootRotation)
            {
                transform.rotation = _stateMachine.animator.rootRotation;
            }
        }
    }

    public virtual void OnStateExit() {
    }

    public void RegisterState(AIStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
}

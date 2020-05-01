using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum AIStateType { None, Idle, Patrol, Pursuit, Alert, Feeding, Attacking }

public enum TargetType { None, Player, Food, Audio }
public class Target
{
    public Transform transform;
    public Vector3 position;
    public TargetType type;
}

public class BodyPartSnapShot {
    public Transform transform = null;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
}

public class AIStateMachine : MonoBehaviour
{
    [Header("For testing")]
    [SerializeField] private string _targetName = "";

    [Header("Animator controllers")]
    [SerializeField] private bool _randomAnimatorController = false;
    [SerializeField] List<RuntimeAnimatorController> _animatorControllers = new List<RuntimeAnimatorController>();

    [Header("Properties")]
    [SerializeField] private bool _screamer = true;
    [SerializeField] private bool _standStill = false;
    [SerializeField] private bool _feelsGunHits = true;
    [SerializeField] private bool _feelsMeleeHits = true;
    [SerializeField] private bool _feelsLegHits = true;
    [SerializeField] private bool _feelsHeadHits = true;
    [SerializeField] private bool _destroyOnDead = true;
    [SerializeField] private bool _isCrawling = false;
    [SerializeField][Range(20, 360)] private float _fieldOfView = 90f;
    [SerializeField] private float _stoppingDistance = 1f;
    [SerializeField] private float _vehicleStoppingDistance = 3f;
    [SerializeField] private float _health = 100f;
    [SerializeField] private List<GameObject> _attacks = new List<GameObject>();
    [SerializeField] private List<AudioClip> _painSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _footstepSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _sounds = new List<AudioClip>();
    [SerializeField] private int _footStepsAudioMixerIndex = 1;
    [SerializeField] private AudioClip _screamSound = null;
    [SerializeField] private bool _seeThroughWalls = false;
    [SerializeField] private UnityEvent _deathEvents = null;

    [Header("For Metamorphosis")]
    [SerializeField] private Material _normalShapeMaterial = null;
    [SerializeField] private Material _metamorphosisMaterial = null;
    [SerializeField] private Transform _metamorphosisFlames = null;

    private AIStateType _currentStateType = AIStateType.None;
    private AIState _currentState = null;
    private Target _currentTarget = null;
    private bool _dead = false;
    private bool _isScreaming = false;
    private PeriodicEnemySpawner _spawner = null;

    // Cache variables
    private NavMeshAgent _navMeshAgent = null;
    private Animator _animator = null;
    private AudioSource _mouthAudioSource = null;

    private Dictionary<AIStateType, AIState> _statesDictionary = new Dictionary<AIStateType, AIState>();
    private TargetTrigger _targetTrigger = null;
    private Vector3 _collisionWithPlayerPosition = Vector3.zero;
    private IEnumerator _getHitCoroutine = null;
    private List<BodyPartSnapShot> _bodyPartsSnapshots = new List<BodyPartSnapShot>();
    private bool _takingDamage = false;
    private float _initialVehicleStoppingDistance = 0f;
    private float _screamDelay = 20f;
    private float _screamTimer = 0f;
    private bool _isAttacking = false;
    private List<SkinnedMeshRenderer> _skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
    private bool _isTransformed = false;
    private bool _canPlayStateSound = true;

    // Animator Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _turningRightHash = Animator.StringToHash("TurningRight");
    private int _turningLeftHash = Animator.StringToHash("TurningLeft");
    private int _isAttackingHash = Animator.StringToHash("IsAttacking");
    private int _backHitHash = Animator.StringToHash("BackHit");
    private int _frontHitRightHash = Animator.StringToHash("FrontHitRightSide");
    private int _frontHitLeftHash = Animator.StringToHash("FrontHitLeftSide");
    private int _fallingBackHash = Animator.StringToHash("FallingBack");
    private int _fallingForwardHash = Animator.StringToHash("FallingForward");
    private int _deadHash = Animator.StringToHash("Dead");
    private int _standupFromFront = Animator.StringToHash("StandupFromFront");
    private int _standupFromBack = Animator.StringToHash("StandupFromBack");
    private int _frontHeadShotFromLeftHash = Animator.StringToHash("FrontHeadFromLeft");
    private int _frontHeadShotFromRightHash = Animator.StringToHash("FrontHeadFromRight");
    private int _rightShoulderHitAndFall = Animator.StringToHash("RightShoulderHitAndFall");
    private int _leftShoulderHitAndFall = Animator.StringToHash("LeftShoulderHitAndFall");
    private int _heavyBackHit = Animator.StringToHash("HeavyBackHit");
    private int _backHeadShotHash = Animator.StringToHash("BackHead");
    private int _rightLegHitHash = Animator.StringToHash("RightLegHit");
    private int _leftLegHitHash = Animator.StringToHash("LeftLegHit");
    private int _rightLegHitAndFallHash = Animator.StringToHash("RightLegHitAndFall");
    private int _leftLegHitAndFallHash = Animator.StringToHash("LeftLegHitAndFall");
    private int _backRightLegHitAndFallHash = Animator.StringToHash("BackRightLegHitAndFall");
    private int _backLeftLegHitAndFallHash = Animator.StringToHash("BackLeftLegHitAndFall");
    private int _backHeavyHitHash = Animator.StringToHash("BackHeavyHit");
    private int _frontHeavyHitHash = Animator.StringToHash("FrontHeavyHit");
    private int _reviveHash = Animator.StringToHash("Revive");
    private int _attackNumberHash = Animator.StringToHash("AttackNumber");
    private int _metamorphosisHash = Animator.StringToHash("Metamorphosis");
    private int _attachedDemonHash = Animator.StringToHash("Attached");

    // Properties
    public AIState currentState { get { return _currentState; } }
    public bool canPlayStateSound { get { return _canPlayStateSound; } }
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navMeshAgent { get { return _navMeshAgent; } }
    public AudioSource mouthAudioSource { get { return _mouthAudioSource; } }
    public int speedhHash { get { return _speedHash; } }
    public int turningRightHash { get { return _turningRightHash; } }
    public int turningLeftHash { get { return _turningLeftHash; } }
    public int isAttackingHash { get { return _isAttackingHash; } }
    public float fieldOfView { get { return _fieldOfView; } }
    public Target currentTarget { get { return _currentTarget; } set { _currentTarget = value; } }
    public AIStateType currentStateType { get { return _currentStateType; } }
    public float stoppingDistance { get {
            if (_currentTarget != null)
            {
                if (_currentTarget.transform.GetComponent<GoneWrong.Player>() != null)
                {
                    // If we are going after the player, we return the normal stopping distance
                    return _stoppingDistance;
                } else
                {
                    // Else, if we are going after a vehicle or something else, we return the _vehicleStoppingDistance
                    return _vehicleStoppingDistance;
                }
            }
            return _stoppingDistance;
        } set { _stoppingDistance = value; } }
    public bool dead { get { return _dead; } }
    public bool isCrawling { get { return _isCrawling; } }
    public IEnumerator gtHitCouroutine { get { return _getHitCoroutine; } }
    public bool takingDamage { get { return _takingDamage; } set { _takingDamage = value; } }
    public AudioClip screamSound { get { return _screamSound; } }
    public bool canScream { get { return _screamTimer >= _screamDelay; } }
    public bool seeThroughWalls { get { return _seeThroughWalls; } }
    public bool screamer { get { return _screamer; } }
    public bool isScreaming { get { return _isScreaming; } set { _isScreaming = value; } }
    public bool isAttacking { get { return _isAttacking; } set {
            _isAttacking = value;
            int whichAttack = Random.Range(0, 3);

            if (!_isAttacking) _animator.SetInteger(_attackNumberHash, whichAttack);
        } }
    public PeriodicEnemySpawner spawner { set { _spawner = value; } }
    public bool isTransformed { get { return _isTransformed; } }

    private void Start()
    {
        // Getting cache variables
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _mouthAudioSource = GetComponent<AudioSource>();
        _targetTrigger = GetComponentInChildren<TargetTrigger>();

        HandleAnimatorController();

        // Getting the skinned mesh renderers for the metamorphosis
        if (_metamorphosisMaterial != null)
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(SkinnedMeshRenderer meshRenderer in skinnedMeshRenderers)
            {
                _skinnedMeshRenderers.Add(meshRenderer);
            }
        }

        // We always have the rotation of the navmesh deactivated!
        // In fact, we either rotate with the root motion rotation of the turn around animation
        // Or we rotate it manually through code.
        // In sum, the navmesh is only useful for calculating the path: steertingTarget :) 
        _navMeshAgent.updateRotation = false;

        if (_targetTrigger != null)
        {
            _targetTrigger.RegisterTargetTrigger(this);
        }

        // Storing states
        _statesDictionary.Add(AIStateType.Idle, GetComponent<AIStateIdle>());
        _statesDictionary.Add(AIStateType.Patrol, GetComponent<AIStatePatrol>());
        _statesDictionary.Add(AIStateType.Pursuit, GetComponent<AIStatePursuit>());
        _statesDictionary.Add(AIStateType.Attacking, GetComponent<AIStateAttack>());
        _statesDictionary.Add(AIStateType.Alert, GetComponent<AIStateAlert>());

        // Registering states
        _statesDictionary[AIStateType.Idle].RegisterState(this);
        _statesDictionary[AIStateType.Patrol].RegisterState(this);
        _statesDictionary[AIStateType.Pursuit].RegisterState(this);
        _statesDictionary[AIStateType.Attacking].RegisterState(this);
        _statesDictionary[AIStateType.Alert].RegisterState(this);

        if (_statesDictionary[AIStateType.Idle] != null)
        {
            ChangeState(AIStateType.Idle);
        }

        // Get all body parts snapshot
        Transform[] bodyParts = transform.GetComponentsInChildren<Transform>();
        foreach(Transform bodyPart in bodyParts)
        {
            BodyPartSnapShot bodyPartSnapshot = new BodyPartSnapShot();
            bodyPartSnapshot.transform = bodyPart;
            _bodyPartsSnapshots.Add(bodyPartSnapshot);
        }

        // At the beginning, we should always be able to scream
        _screamTimer = _screamDelay;

        _initialVehicleStoppingDistance = _vehicleStoppingDistance;
    }

    private void Update()
    {
        // For testing:
        _targetName = _currentTarget != null ? _currentTarget.transform.name : "";

        if (_dead) return;

        // Incrementing the scream timer (so that we don't spam screaming)
        if (_screamTimer < _screamDelay)
        {
            _screamTimer += Time.deltaTime;
        }

        // To handle opening doors:
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 1f, LayerMask.GetMask("Interactive"));
        if(hits.Length > 0)
        {
            Debug.Log("Interacted with a door");
            InteractiveDoor door = hits[0].transform.GetComponent<InteractiveDoor>();
            if (door != null)
            {
                if (!door.open && door.canOpen)
                {
                    door.Interact(transform);
                    ChangeState(AIStateType.Idle);
                }
            }
        }

        if (_currentState != null)
        {
            AIStateType nextStateType = _currentState.OnUpdate();

            if (_currentStateType != nextStateType)
            {
                ChangeState(nextStateType);
            }
        }
    }

    private void OnAnimatorMove()
    {
        if (_currentState != null)
        {
            _currentState.OnAnimatorUpdated();
        }
    }

    public bool IsStateEnabled(AIStateType stateType)
    {
        if (_statesDictionary.ContainsKey(stateType))
        {
            return _statesDictionary[stateType].enabled;
        }

        return false;
    }

    public void ChangeState(AIStateType nextStateType)
    {
        // If we are standing still, we stay in idle mode
        if (_standStill) nextStateType = AIStateType.Idle;

        if (_statesDictionary.ContainsKey(nextStateType))
        {
            // End the current state:
            if (_currentState != null)
                _currentState.OnStateExit();

            // Switch to the new state
            _currentState = _statesDictionary[nextStateType];
            _currentStateType = nextStateType;
            _currentState.OnStateEnter();
        } 
    }

    public void Attack(int index)
    {
        if (_attacks.Count > index)
        {
            _attacks[index].SetActive(true);
        }
    }

    public void TerminateAttack(int index)
    {
        if(_attacks.Count > index)
        {
            _attacks[index].SetActive(false);
        }
    }

    public void TerminateAllAttacks()
    {
        foreach(GameObject attack in _attacks)
        {
            if (attack != null)
                attack.SetActive(false);
        }
    }

    public void TakeDamage(float amount, Vector3 hitSourcePosition, RaycastHit hit, bool gun = true, int fromRight = -1)
    {
        if (_dead) return;

        // Calculating from which side we got hit
        Vector3 toHitPointVector = hit.point - transform.position;
        bool rightHalfHit = Vector3.Cross(transform.forward, toHitPointVector).y > 0 ? true : false;
        // We do this process only if fromright is different from -1
        // We are attacking from right if fromRight is equal to 1
        rightHalfHit = fromRight == -1 ? rightHalfHit : fromRight != 1;

        // We emit blood particles at the hit point
        if (EffectsManager.instance != null && EffectsManager.instance.bloodParticles != null)
        {
            ParticleSystem bloodParticles = EffectsManager.instance.bloodParticles;
            bloodParticles.transform.position = hit.point;
            bloodParticles.transform.rotation = Quaternion.LookRotation(transform.position - GoneWrong.Player.instance.transform.position);
            //bloodParticles.transform.rotation = transform.rotation * Quaternion.Euler(new Vector3(0, rightHalfHit ? -90 : 90, 0));
            //bloodParticles.Emit(10000);
            bloodParticles.Emit(1);
        }

        _health = Mathf.Max(0, _health - amount);

        // We calculate the angle between our forward vector and that leading to the source
        Vector3 toSourceVector = hitSourcePosition - transform.position;
        float angle = Vector3.Angle(transform.forward, toSourceVector);

        // Play pain sound
        if(_painSounds.Count > 0)
        {
            _mouthAudioSource.Stop();
            _mouthAudioSource.clip = _painSounds[Random.Range(0, _painSounds.Count)];
            _mouthAudioSource.Play();
        }

        if (_animator != null) {
            if (angle > 90)
            {
                // This is a back hit
                
                if (_health > 0)
                {
                    if (_takingDamage) return;

                    if (hit.transform.gameObject.CompareTag("RightLeg") && gun && _feelsLegHits)
                    {
                        if (_currentStateType == AIStateType.Pursuit)
                            _animator.SetTrigger(_backRightLegHitAndFallHash);
                        else _animator.SetTrigger(_rightLegHitHash);
                    } else if (hit.transform.gameObject.CompareTag("LeftLeg") && gun && _feelsLegHits) {
                        if (_currentStateType == AIStateType.Pursuit)
                            _animator.SetTrigger(_backLeftLegHitAndFallHash);
                        else _animator.SetTrigger(_leftLegHitHash);
                    } else if (hit.transform.gameObject.CompareTag("Head") && _backHeadShotHash != -1 && gun && _feelsHeadHits)
                    {
                        _animator.SetTrigger(_backHeadShotHash);
                    } else
                    {
                        if (!gun && _feelsMeleeHits)
                        {
                            _animator.SetTrigger(_backHitHash);
                        } else if (gun && _feelsGunHits)
                        {
                            _animator.SetTrigger(_heavyBackHit);
                        }
                    }
                } else
                {
                    Die();
                    _animator.SetBool(_fallingForwardHash, true);
                }
            } else
            {
                // This is a front hit
                if (_health > 0)
                {
                    if (_takingDamage) return;

                    if (hit.transform.gameObject.CompareTag("RightLeg") && gun && _feelsLegHits)
                    {
                        if (_currentStateType == AIStateType.Pursuit)
                            _animator.SetTrigger(_rightLegHitAndFallHash);
                        else
                            _animator.SetTrigger(_rightLegHitHash);
                    }
                    else if (hit.transform.gameObject.CompareTag("LeftLeg") && gun && _feelsLegHits)
                    {
                        if(_currentStateType == AIStateType.Pursuit)
                            _animator.SetTrigger(_leftLegHitAndFallHash);
                        else
                            _animator.SetTrigger(_leftLegHitHash);
                    } else if (hit.transform.gameObject.CompareTag("Head") && gun && _feelsHeadHits)
                    {
                        _animator.SetTrigger(rightHalfHit ? _frontHeadShotFromLeftHash : _frontHeadShotFromRightHash);
                    }
                    else
                    {
                        // If it's a melle attack, we always play the same animation
                        if (!gun && _feelsMeleeHits)
                        {
                            _animator.SetTrigger(rightHalfHit ? _frontHitRightHash : _frontHitLeftHash);
                        }
                        else if (gun && _feelsGunHits)
                        {
                            _animator.SetTrigger(rightHalfHit ? _rightShoulderHitAndFall : _leftShoulderHitAndFall);
                        }
                    }
                }
                else
                {
                    Die();
                    _animator.SetBool(_fallingBackHash, true);
                }
            }

            if (_currentTarget == null && !_dead)
                ChangeState(AIStateType.Alert);
        }
    }

    public void Die()
    {
        if (!_dead)
        {
            if (_spawner != null)
            {
                _spawner.UnregisterEnemy();
            }

            // We invoke the dead events
            if (_deathEvents != null)
            {
                _deathEvents.Invoke();

                // After triggering an event, an event monster should be deactivated so that when we save the game,
                // he will be understood as a dead boss (or event monster)
                if (SceneManager.GetActiveScene().name != "Hospital")
                {
                    if (_destroyOnDead)
                        Invoke("DestroyGameObject", 5);
                }
            }

            // We instantiate the item
            if (EnemyTeleporter.instance != null)
            {
                EnemyTeleporter.instance.SpawnItem(transform.position);
            }
        }

        _dead = true;
        _animator.SetBool(_deadHash, true);
        _navMeshAgent.enabled = false;
    }

    public void Revive()
    {
        ChangeState(AIStateType.Idle);

        // We set all the dead animations to false
        if (_animator != null)
        {
            _animator.SetBool(_fallingBackHash, false);
            _animator.SetBool(_fallingForwardHash, false);
            _animator.SetBool(_deadHash, false);

            // Then we set the animator revive trigger
            _animator.SetTrigger(_reviveHash);
        }

        _health = 100f;
        if (_navMeshAgent != null)
            _navMeshAgent.enabled = true;

        _dead = false;
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    public void Ragdoll(bool doRagdoll)
    {
        Rigidbody[] bodyParts = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody part in bodyParts)
        {
            if (part.transform.gameObject.layer == LayerMask.NameToLayer("BodyPart"))
            {
                part.isKinematic = !doRagdoll;
                part.transform.GetComponent<Collider>().isTrigger = !doRagdoll;
            }
        }
    }

    public void PlayFootstepSound()
    {
        if(_footstepSounds.Count > 0 && GoneWrong.AudioManager.instance != null)
        {
            AudioClip clip = _footstepSounds[Random.Range(0, _footstepSounds.Count)];
            if (clip != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1f, 1, _footStepsAudioMixerIndex, transform.position);
            }
        }
    }

    public void Scream()
    {
        if (GoneWrong.AudioManager.instance != null && _screamSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_screamSound, 1, 1, 1, transform.position);
        }

        if (AudioThreatManager.instance != null)
        {
            AudioThreatManager.instance.MakeNoise(15, 1, transform.position);
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        // To detect if we are within the vehicle attack trigger or not:
        if (col.CompareTag("VehicleAttackTrigger"))
        {
            _vehicleStoppingDistance = (transform.position - col.transform.position).magnitude;

        }

        // To detect the collision with a car
        if (col.transform.CompareTag("Car"))
        {
            if (_dead) return;

            Vehicle carController = col.transform.GetComponentInParent<Vehicle>();
            carController.TakeDamage(5);

            if (carController.GetComponent<Rigidbody>().velocity.magnitude < 4)
            {
                return;
            }

            if (GoneWrong.AudioManager.instance != null )
            {
                if (carController != null && carController.collisionWithBodySound != null)
                    GoneWrong.AudioManager.instance.PlayOneShotSound(carController.collisionWithBodySound, 1, 0, 1);

                if (_painSounds.Count > 0)
                {
                    AudioClip painSound = _painSounds[Random.Range(0, _painSounds.Count)];
                    if (painSound != null)
                    {
                        GoneWrong.AudioManager.instance.PlayOneShotSound(painSound, 1, 1, 1, transform.position);
                    }
                }
            }

            _health -= 30f;

            // Add force to send the zombie flying

            transform.LookAt(col.transform.position);
            // We are gonna get hit and play the hit animation
            // But we are gonna calculate the angle we make with the hit source to check from where it came
            float angle = Vector3.Angle(transform.forward, col.transform.position - transform.position);
            if (angle > 90)
            {
                // This is a hit from behind:
                /*if (_health >= 0)
                    _animator.SetTrigger(_backHeavyHitHash);
                else _animator.SetTrigger(_fallingBackHash);*/
                _animator.SetTrigger(_backHeavyHitHash);
            } else
            {
                // This is a hit from the front:
                /*if (_health >= 0)
                    _animator.SetTrigger(_backHeavyHitHash);
                else _animator.SetTrigger(_fallingForwardHash);*/

                _animator.SetTrigger(_frontHeavyHitHash);
            }

            if (_health <= 0)
            {
                Die();
            }
        
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("VehicleAttackTrigger"))
        {
            _vehicleStoppingDistance = _initialVehicleStoppingDistance;
        }
    }

    public void OnDisable()
    {
        // When the zombie get disabled (usually by the quality manager), we should set the target to null
        // So that when we re-enable him, we shouldn't find him attacking the air
        _currentTarget = null;
        _currentStateType = AIStateType.Idle;
    }

    public void ResetScreamTimer()
    {
        _screamTimer = 0f;
    }

    public bool IsAgentOnNavMesh(GameObject agentObject)
    {
        // Don't set this too high, or NavMesh.SamplePosition() may slow down
        float onMeshThreshold = 1;

        Vector3 agentPosition = agentObject.transform.position;
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(agentPosition, out hit, onMeshThreshold, NavMesh.AllAreas))
        {
            // Check if the positions are vertically aligned
            if (Mathf.Approximately(agentPosition.x, hit.position.x)
                && Mathf.Approximately(agentPosition.z, hit.position.z))
            {
                // Lastly, check if object is below navmesh
                return agentPosition.y >= hit.position.y;
            }
        }

        return false;
    }

    public void HandleStandingStill(bool stand)
    {
        _standStill = stand;
    }

    public void StartChangeAnimationCoroutine(float targetValue, Animator animator, float previousValue, string parameter)
    {
        IEnumerator coroutine = SetAnimatorValue(targetValue, animator, previousValue, parameter);
        StartCoroutine(coroutine);
    }

    public IEnumerator SetAnimatorValue(float targetValue, Animator animator, float previousValue, string parameter)
    {
        float timer = 0f;
        float delay = 1f;

        while (timer < delay)
        {
            timer += Time.deltaTime;

            float normalizedTime = timer / delay;
            if (targetValue > previousValue)
                animator.SetFloat(parameter, previousValue + (targetValue - previousValue) * normalizedTime);
            else
                animator.SetFloat(parameter, previousValue - (previousValue - targetValue) * normalizedTime);

            yield return null;
        }

        animator.SetFloat(parameter, targetValue);
    }

    public void MetamorphosisTrigger()
    {
        // We look at the player before metamorphosing
        transform.rotation = Quaternion.LookRotation(GoneWrong.Player.instance.transform.position - transform.position);

        if (_animator != null && _metamorphosisMaterial != null)
        {
            _isTransformed = true;

            if (_metamorphosisFlames != null)
            {
                _metamorphosisFlames.gameObject.SetActive(true);
            }
            _animator.SetTrigger(_metamorphosisHash);
        }
    }

    public void Metamorphosis(int backToNormal)
    {
        Material material = backToNormal == 1 ? _normalShapeMaterial : _metamorphosisMaterial;

        if (material != null)
        {
            if (_skinnedMeshRenderers.Count > 0)
            {
                foreach(SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderers)
                {
                    skinnedMeshRenderer.material = material;
                }
            }
        }

        Invoke("GetAttachedToPlayer", 4);
    }

    public void GetAttachedToPlayer()
    {
        if (_animator != null)
            _animator.SetBool(_attachedDemonHash, true);

        _navMeshAgent.enabled = false;
        GoneWrong.Player.instance.AttachDemon(transform);
        //this.enabled = false;
    }

    public void PlaySound(int index)
    {
        // We set the sound index of the current state to -1, so we could directly go back to playing it the next time
        _statesDictionary[_currentStateType].soundIndex = -1;

        if (_mouthAudioSource != null)
        {
            AudioClip clip = _sounds[index];
            if (clip != null)
            {
                // When we play an animation sound, we should prevent the current state sound from doing anything
                _canPlayStateSound = false;
                _mouthAudioSource.clip = clip;
                _mouthAudioSource.Play();
                // Then we revert back to current state sound after we are done playing the sound
                Invoke("RevertBackToStateSound", clip.length);
            }
        }
    }

    public void RevertBackToStateSound()
    {
        _canPlayStateSound = true;
    }

    public void InflictRemoteDamage(float damage)
    {
        if (GoneWrong.Player.instance != null)
        {
            if(!GoneWrong.Player.instance.dead)
                GoneWrong.Player.instance.TakeDamage(damage);
        }
    }

    private void OnEnable()
    {
        // We change the animator controller whenever we enable the zombie
        // We do the testing if in the function
        HandleAnimatorController();
    }

    public void HandleAnimatorController()
    {
        if (_randomAnimatorController && _animatorControllers.Count > 0)
        {
            RuntimeAnimatorController controller = _animatorControllers[Random.Range(0, _animatorControllers.Count)];
            if (controller != null)
            {
                Animator animator = _animator;
                if (animator == null)
                    animator = GetComponent<Animator>();

                animator.runtimeAnimatorController = controller;
            }
        }
    }
}

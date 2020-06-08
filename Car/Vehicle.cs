using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Vehicle : MonoBehaviour {

    public enum RotationAxis
    {
        X, Y, Z
    }

    public enum DrivingType {
        Type1,
        Type2,
    }

    public enum BrakingType
    {
        FrontWheels,
        BackWheels,
        AllWheels
    }

    [SerializeField] private bool _canPassToExternalCamera = false;
    [SerializeField] private bool _enginePitch = true;
    [SerializeField] private float _engineTorque = 300f;
    [SerializeField] private float _maxSteering = 30f;
    [SerializeField] private float _brakeTorque = 5000f;
    [SerializeField] private BrakingType _brakeType = BrakingType.AllWheels;
    [SerializeField] private float _maxSpeed = 99999f;

    [Header("RPMs")]
    [SerializeField] List<float> _rpms = new List<float>();

    [SerializeField] List<WheelCollider> _wheelColliders = new List<WheelCollider>();
    [SerializeField] List<Transform> _wheelTransforms = new List<Transform>();

    [Header("For Lights")]
    [SerializeField] List<MeshRenderer> _backLightsMeshes = new List<MeshRenderer>();
    [SerializeField] Material _backLightMaterial = null;
    [SerializeField] Material _backLightActivatedMaterial = null;
    [SerializeField] List<Light> _backLights = new List<Light>(); 

    [Header("Propeties")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private SharedFloat _vehicleHealthSharedFloat = null;

    [Header("For Camera")]
    [SerializeField] Transform _externalCameraPositionTransform = null;

    private float _steer = 0f;
    private float _torque = 0f;
    private bool _braking = false;
    private bool _switchingTorque = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip _idleSound = null;
    [SerializeField] private AudioClip _drivingSound = null;
    [SerializeField] private AudioClip _drivingBackwardSound = null;
    [SerializeField] private AudioClip _brakeSound = null;
    [SerializeField] private AudioClip _startUpSound = null;
    [SerializeField] private List<AudioClip> _collisionSounds = new List<AudioClip>();
    [SerializeField] private AudioClip _collisionWithBodySound = null;
    [SerializeField] List<AudioClip> _damageSounds = new List<AudioClip>();

    [Header("Car Internals")]
    [SerializeField] private Transform _helm = null;
    [SerializeField] private RotationAxis _helmRotationAxis = RotationAxis.Z;
    [SerializeField] private bool _inverseWheel = false;

    [Header("Destination reached events")]
    [SerializeField] private Transform _destination = null;
    [SerializeField] private UnityEvent _onDestinationReachedEvents = null;

    // Public accesors
    public AudioClip collisionWithBodySound { get { return _collisionWithBodySound; } }
    public List<AudioClip> damageSounds { get { return _damageSounds; } }
    public AudioClip startUpSound { get { return _startUpSound; } }

    Rigidbody rb;
    AudioSource _audioSource = null;
    private bool _skidding = false;
    private List<Smoke> _smokes = new List<Smoke>();
    private List<Light> _lights = new List<Light>();
    private bool _reachedDestination = false;

    private Camera _mainCamera = null; 

    private void Start()
    { 
        rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        _mainCamera = Camera.main;

        // We are getting all car smokes
        Smoke[] smokes = GetComponentsInChildren<Smoke>();
        foreach(Smoke smoke in smokes)
        {
            smoke.gameObject.SetActive(false);
            _smokes.Add(smoke);
        }

        Light[] lights = GetComponentsInChildren<Light>();
        foreach(Light light in lights)
        {
            light.gameObject.SetActive(false);
            _lights.Add(light);
        }

        enabled = false;
    }

    private void Update()
    {
        // For input
        _torque = Input.GetAxis("Vertical");
        _steer = Input.GetAxis("Horizontal");

        // Handle braking
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _braking = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _braking = false;
        }

        // To turn the helm
        if (_helm != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            Vector3 nextRotation = Vector3.zero;
            float sign = _inverseWheel ? -1 : 1;
            switch (_helmRotationAxis)
            {
                case RotationAxis.X:
                    nextRotation = new Vector3(sign * horizontal * 90f, _helm.transform.localRotation.eulerAngles.y, _helm.transform.localRotation.eulerAngles.z);
                    break;
                case RotationAxis.Y:
                    nextRotation = new Vector3(_helm.transform.localRotation.eulerAngles.x, sign * horizontal * 90f, _helm.transform.localRotation.eulerAngles.z);
                    break;
                case RotationAxis.Z:
                    nextRotation = new Vector3(_helm.transform.localRotation.eulerAngles.x, _helm.transform.localRotation.eulerAngles.y, sign * horizontal * 90f);
                    break;
            }
            _helm.localRotation = Quaternion.Lerp(_helm.localRotation, Quaternion.Euler(nextRotation), Time.deltaTime * 4f);
        }

        // To get out of the car
        if (Input.GetKeyDown(KeyCode.E))
        {
            GetComponentInChildren<InteractiveCar>().Interact(GoneWrong.Player.instance.transform);
            SwitchCamera(true);
        }

        // Switching the camera
        if (Input.GetKeyDown(KeyCode.C) && _canPassToExternalCamera)
        {
            SwitchCamera(!_mainCamera.enabled);
        }

        HandleSound();

        HandleLights();

        HandleCarReversal();

        // Updating health
        if (_vehicleHealthSharedFloat != null)
        {
            _vehicleHealthSharedFloat.value = _health;
        }

        // To regulate max speed:
        if (rb.velocity.magnitude >= _maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * _maxSpeed;
        }

        // Check for destination reached
        if (_destination != null && _onDestinationReachedEvents != null)
        {
            // Calculate the distance between the vehicle and the destination
            float distance = (transform.position - _destination.transform.position).magnitude;
            if (distance <= 15f)
            {
                if (!_reachedDestination)
                {
                    _onDestinationReachedEvents.Invoke();
                }

                _reachedDestination = true;
            }
        }
    }

    public void SetNewDestination(Transform destination, UnityEvent events)
    {
        _reachedDestination = false;
        _destination = destination;
        _onDestinationReachedEvents = events;
    }

    public void HandleLights()
    {
        if (_backLightMaterial == null || _backLightActivatedMaterial == null) return;

        for (int i = 0; i < _backLightsMeshes.Count; i++)
        {
            if (_backLightsMeshes[i] == null) continue;

            if ((_braking && rb.velocity.magnitude > 0) || _switchingTorque || _torque < 0)
            {
                _backLightsMeshes[i].material = _backLightActivatedMaterial;
            }
            else
            {
                _backLightsMeshes[i].material = _backLightMaterial;
            }
        }

        for (int i = 0; i < _backLights.Count; i++)
        {
            if (_backLights[i] == null) continue;

            if ((_braking && rb.velocity.magnitude > 0) || _switchingTorque || _torque < 0)
            {
                _backLights[i].gameObject.SetActive(true);
            }
            else
            {
                _backLights[i].gameObject.SetActive(false);
            }
        }
    }

    public void HandleCarReversal()
    {
        GoneWrong.Player player = GoneWrong.Player.instance;

        if (player == null) return;

        // The dot product of vector a and b is equal to: a.magnitude * b.magnitude * cos(angle)
        // In this case, the dot product is equal to con(angle) only because the vectors are normalized
        // the dot product's value is between -1 and 1.
        // -1 when the car up vector makes a 180 angle with the world down vector
        // 1 when the car up vector makes a 0 angle with the world down vector
        // When the angle is between -90 and 90, cos(angle) is superior to 0
        if (Vector3.Dot(transform.up, Vector3.down) > -0.5f)
        {
            player.inReversedCar = true;
        } else
        {
            player.inReversedCar = false;
        }
    }

    public void SwitchCamera(bool mainCamera)
    {
        if (ExternalCamera.instance == null || _externalCameraPositionTransform == null) return;

        ExternalCamera.instance.transform.position = transform.position;
        _mainCamera.enabled = mainCamera;

        // Set camera position
        Camera externalCameraCamera = ExternalCamera.instance.GetComponentInChildren<Camera>();

        // Setting the camera position
        externalCameraCamera.transform.position = _externalCameraPositionTransform.transform.position;
        externalCameraCamera.transform.rotation = _externalCameraPositionTransform.transform.rotation;

        _mainCamera.GetComponent<AudioListener>().enabled = mainCamera;
        ExternalCamera.instance.gameObject.SetActive(!mainCamera);
        ExternalCamera.instance.target = _externalCameraPositionTransform;
    }

    void FixedUpdate()
    {
        // If switch suddenly from moving forward to moving backwards.
        if (((_torque > 0 && Vector3.Angle(rb.velocity, transform.forward) > 90
            || _torque < 0 && Vector3.Angle(rb.velocity, transform.forward) < 90)
            && rb.velocity.magnitude > 0))
        {
            _switchingTorque = true;
        }
        else
        {
            _switchingTorque = false;
        }

        // Handle Torque
        for (int i = 0; i < _wheelColliders.Count; i++)
        {
            if (_health > 0)
                _wheelColliders[i].motorTorque = _torque * _engineTorque;
            else
            {
                // In case the car is destroyed, we can no longer move it
                _wheelColliders[i].motorTorque = 0f;
            }

            if (_braking || _switchingTorque)
            {
                bool doBrake = false;

                doBrake = (_brakeType == BrakingType.FrontWheels && i < 2
                    || _brakeType == BrakingType.AllWheels
                    || _brakeType == BrakingType.BackWheels && i > 1);

                if (doBrake)
                    _wheelColliders[i].brakeTorque = _brakeTorque;
            }
            else
            {
                _wheelColliders[i].brakeTorque = 0f;
            }
        }

        // Handle Steering
        if (_wheelColliders.Count >= 2)
        {
            _wheelColliders[0].steerAngle = _maxSteering * _steer;
            _wheelColliders[1].steerAngle = _maxSteering * _steer;
        }

        // Handle Transforms
        if (_wheelColliders.Count == _wheelTransforms.Count)
        {
            for (int i = 0; i < _wheelColliders.Count; i++)
            {
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;

                _wheelColliders[i].GetWorldPose(out position, out rotation);
                if (i < 4)
                    _wheelTransforms[i].position = position;
                _wheelTransforms[i].rotation = rotation;
            }
        }
    }

    // speed calculation 
    public float currentSpeed ()
    {
        float speed = rb.velocity.magnitude;
        speed = rb.velocity.magnitude * 3.6f; //is multiplied by a 3.6 kmh

        return speed;
    }

    private void HandleSound()
    {
        if (_audioSource == null) return;

        AudioClip clip;

        clip = _torque >= 0 ? _idleSound : _drivingBackwardSound;

        // For skidding/brake sound
        if (Input.GetKeyDown(KeyCode.Space) && rb.velocity.magnitude > 0)
        {
            _skidding = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) || rb.velocity.magnitude < 1f)
        {
            _skidding = false;
        }

        if (_skidding) {
            clip = _brakeSound;
        }
        else
        {
            // If we are not skidding but still have skidding as the current audio source
            if (_audioSource.clip == _brakeSound)
            {
                clip = _idleSound;
            }
        }

        // Managing pitch:
        if (_enginePitch)
        {
            float pitch = 0f;
            if (clip == _drivingBackwardSound)
            {
                // If we are driving backwards, the sound pitch should always be equal to 1
                pitch = 1;
            }
            else
            {
                if (_skidding)
                {
                    pitch = 1;
                }
                else
                {
                    // 3 - 1 expresses the pitch max andd min values
                    //pitch = (rb.velocity.magnitude / (_maxSpeed / (3 - 1))) + 1;
                    pitch = ((rb.velocity.magnitude * 2) / _maxSpeed) + 1;

                }
            }
            _audioSource.pitch = pitch;
        } else
        {
            _audioSource.pitch = 1;
            if (clip == _idleSound && _torque > 0)
            {
                clip = _drivingSound;
            }
        }

        if ((_audioSource.clip != clip || _audioSource.clip == null) && clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
            _audioSource.loop = true;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.layer == LayerMask.NameToLayer("Default") || collision.gameObject.layer == LayerMask.NameToLayer("DecorationBase")) && Time.time > 2f)
        {
            if (GoneWrong.AudioManager.instance != null && _collisionSounds.Count > 0)
            {
                AudioClip clip = _collisionSounds[Random.Range(0, _collisionSounds.Count)];
                if (clip != null)
                    GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 2, 0, 0, transform.position);
            }

            // Taking Damage
            TakeDamage(2.5f);
        }
    }

    private void OnEnable()
    {
        // We activate smokes
        foreach(Smoke smoke in _smokes)
        {
            smoke.gameObject.SetActive(true);
        }

        // We activate lights
        foreach (Light light in _lights)
        {
            light.gameObject.SetActive(true);
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    private void OnDisable()
    {
        // We deactive all sounds and remove the audio clip
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }

        // We deactivate smokes
        foreach (Smoke smoke in _smokes)
        {
            smoke.gameObject.SetActive(false);
        }

        // We also deactivate the light
        foreach(Light light in _lights)
        {
            light.gameObject.SetActive(false);
        }

        // We are gonna reset the light meshes
        if (_backLightMaterial == null || _backLightActivatedMaterial == null)
        {
            foreach (MeshRenderer meshRenderer in _backLightsMeshes) { 
                meshRenderer.material = _backLightMaterial;
            }
        }

        // And we set the rigidbody velocity to zero
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        _health = Mathf.Max(0, _health);
    }
}

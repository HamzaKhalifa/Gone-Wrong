﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace GoneWrong
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        public static Player instance = null;

        [Header("Player Stats")]
        [SerializeField] private string _playerName = "Hamza Khalifa";
        [SerializeField] private float _gravity = 5f;
        [SerializeField] private float _walkSpeed = 1.5f;
        [SerializeField] private float _runSpeed = 3f;
        [SerializeField] private GameObject _weaponHolder = null;
        [SerializeField] Inventory _playerInventory = null;
        [SerializeField] List<AudioClip> _damageSounds = new List<AudioClip>();
        [SerializeField] List<AudioClip> _runningSounds = new List<AudioClip>();

        [SerializeField] Image _bloodScreen = null;
        [SerializeField] Image _deadImage = null;
        [SerializeField] AudioClip _crouchSound = null;
        [SerializeField] private Transform _attachedDemonTransform = null;

        [Header("Shared variables")]
        [SerializeField] SharedFloat _healthSharedFloat = null;
        [SerializeField] SharedFloat _staminaSharedFloat = null;
        [SerializeField] SharedFloat _infectionSharedFloat = null;

        [SerializeField] SharedInt _equippedWeaponSharedInt = null;

        [Header("Audio")]
        [SerializeField] AudioClip[] _footStepsConcrete = null;
        [SerializeField] AudioClip[] _footStepsGrass = null;
        [SerializeField] AudioClip[] _footStepsWood = null;
        [SerializeField] AudioClip[] _footStepsMetal = null;
        [SerializeField] List<AudioClip> _tauntSounds = new List<AudioClip>();
        [SerializeField] private AudioClip _inventorySound = null;

        [Header("Player")]
        [SerializeField] private bool _neverActuallyDies = false;
        [SerializeField] private List<UnityEvent> _deadEvents = new List<UnityEvent>();
        [SerializeField] private List<Transform> _deadSpawns = new List<Transform>();

        // Cache
        private CharacterController _characterController = null;

        // Private
        private bool _isRunning = false;
        private bool _isAttacking = false;
        private bool _isShooting = false;
        private Vector3 _desiredMovement = Vector3.zero;
        private int _nextFootStep = 0;
        private int _equippedWeapon = 0;
        private LayerMask _interactiveLayer = -1;
        private IEnumerator _weaponSwitchCoroutine = null;
        private WeaponControl _equippedWeaponControl = null;
        private bool _dead = false;
        private bool _canRun = true;
        private List<AIStateMachine> _collidingEnemies = new List<AIStateMachine>();
        private float _initialCharacterControllerHeight = 0f;
        private bool _insideACar = false;
        private Vehicle _drivedVehicle = null;
        private float _initialCharacterControllerStepOffset = 0f;
        private bool _inReversedCar = false;
        private float _timeSpentBeingReversed = 0f;
        private bool _canMove = true;
        private int _weaponIndex = -1;
        private float _runningSoundTime = 0f;
        private float _runningSoundDelay = 0f;
        private IEnumerator _deadCoroutine = null;
        private int _numberOfTimesDead = 0;


        // Get the Horizontal and Vertical axes
        private float _vertical = 0f;
        private float _horizontal = 0f;

        // Properties
        public int weaponIndex { set { _weaponIndex = value; } }
        public bool canMove { get { return _canMove; } set { _canMove = value; } }
        public bool dead { get { return _dead; } }
        public Vector3 desiredMovement { get { return _desiredMovement; } }
        public int equippedWeapon { get { return _equippedWeapon; } }
        public WeaponControl equippedWeaponControl { get { return _equippedWeaponControl; } }
        public bool isRunning { get { return _isRunning; } }
        public bool isAttacking { get { return _isAttacking; } }
        public bool isShooting { get { return _isShooting; } }
        public GameObject weaponHolder { get { return _weaponHolder; } }
        public string playerName { get { return _playerName; } }
        public Vehicle drivedVehicle { get { return _drivedVehicle; } set { _drivedVehicle = value; } }
        public bool insideACar {
            get
            {
                return _insideACar;
            }
            set {
                _insideACar = value;

                // If we are inside a car, we are going to stop looking at everything
                if (_insideACar)
                {
                    Flashlight.instance.Look(false);
                    Smartphone.instance.Look(false);
                    SwitchWeapon(0);
                }
            }
        }
        public bool inReversedCar { set { _inReversedCar = value; } }
        public CharacterController characterController { get { return _characterController; } }

        public AudioClip footSteps
        {
            get
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(transform.position, -transform.up, out hitInfo, 10, LayerMask.GetMask("Default", "DecorationBase", "Wood", "Metal"))) {
                    if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "Default" && _footStepsConcrete.Length > 0)
                    {
                        AudioClip footStep = _footStepsConcrete[_nextFootStep];
                        _nextFootStep++;
                        if (_nextFootStep >= _footStepsConcrete.Length)
                            _nextFootStep = 0;

                        return footStep;
                    } else if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "DecorationBase")
                    {
                        if (_footStepsGrass.Length > 0)
                            return _footStepsGrass[Random.Range(0, _footStepsGrass.Length)];
                        else return null;
                    } else if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "Wood")
                    {
                        if (_footStepsWood.Length > 0)
                            return _footStepsWood[Random.Range(0, _footStepsWood.Length)];
                        else return null;
                    } else if (LayerMask.LayerToName(hitInfo.transform.gameObject.layer) == "Metal")
                    {
                        if (_footStepsMetal.Length > 0)
                            return _footStepsMetal[Random.Range(0, _footStepsMetal.Length)];
                        else return null;
                    }
                    else return null;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool isMoving
        {
            get
            {
                // Always return false if we can't move (so that we don't make the move animation in weapon control when we are loading a scene)
                if (!_canMove) return false;

                if (Mathf.Abs(_desiredMovement.x) > 0 || Mathf.Abs(_desiredMovement.z) > 0)
                {
                    if (_characterController.isGrounded) { return true; }
                }
                return false;
            }
        }

        private void Awake()
        {
            instance = this;

            // Setting default values of shared variables
            _healthSharedFloat.value = 100;
            _staminaSharedFloat.value = 100;
            _infectionSharedFloat.value = 100;

            // Getting cache variables
            _characterController = GetComponent<CharacterController>();
            if (_characterController != null)
                _initialCharacterControllerHeight = _characterController.height;

            _interactiveLayer = LayerMask.GetMask("Interactive");

            // We set the default equipped weapon control to the first one (None)
            _equippedWeaponControl = _weaponHolder.GetComponentInChildren<GoneWrong.WeaponControl>();

            _initialCharacterControllerStepOffset = _characterController.stepOffset;

            // We turn off the cursor after loading:
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            // If we are dead, we don't do anything
            if (_dead)
            {
                return;
            }

            // We shouldn't be able to move or do anything once we are having a cinematic playing
            if (!_canMove)
            {
                if (PlayerInventoryUI.instance != null)
                    PlayerInventoryUI.instance.gameObject.SetActive(false);
                
                if (_characterController.enabled)
                    _characterController.enabled = false;

                // If we can't move, we stop the player from running
                StopRunning();

                return;
            }
            else if (!_characterController.enabled) _characterController.enabled = true;

            // Handle car reversal:
            HandleCarReversal();

            // Activate Inventory
            if (Input.GetKeyDown(KeyCode.Tab) && PlayerInventoryUI.instance != null)
            {
                bool activate = !PlayerInventoryUI.instance.gameObject.activeSelf;

                if (activate)
                    PlayerInventoryUI.instance.Repaint(false);

                PlayerInventoryUI.instance.gameObject.SetActive(activate);
                if (PlayerHUD.instance != null)
                {
                    PlayerHUD.instance.gameObject.SetActive(!activate);
                }

                // Play inventory sound
                if (_inventorySound != null)
                {
                    AudioManager.instance.PlayOneShotSound(_inventorySound, 1, 0, 1);
                }

                if (activate)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            // We also don't do anything if we are inside a car
            if (_insideACar) {
                if (PlayerHUD.instance != null)
                    PlayerHUD.instance.DeactivateInteractiveText();
                return;
            }

            // Handle detecting items to interact with
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                RaycastHit[] hits;
                
                hits = Physics.RaycastAll(ray, 2f, _interactiveLayer);
                bool interaftiveInSight = false;
                if (hits.Length > 0)
                {
                    int higherPriority = 251;
                    int interactiveObjectIndex = -1;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        InteractiveObject interactiveObject = hits[i].collider.transform.GetComponent<InteractiveObject>();
                        if (interactiveObject != null && interactiveObject.priority < higherPriority)
                        {
                            higherPriority = interactiveObject.priority;
                            interactiveObjectIndex = i;
                        }
                    }

                    if (interactiveObjectIndex != -1)
                    {
                        InteractiveObject chosenInteractiveObject = hits[interactiveObjectIndex].collider.transform.GetComponent<InteractiveObject>();

                        // Trying to check if we have something of layer default (solid) in front of us and preventing us from interaction with the interactive object behind it
                        /*float distanceToSolid = float.MaxValue;
                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo, 2f, LayerMask.GetMask("Default")))
                        {
                            // Only take the solid into consideration if it's not attached to an interactive object
                            if (hitInfo.transform.GetComponentInParent<InteractiveObject>() == null)
                            {
                                distanceToSolid = Vector3.Distance(Camera.main.transform.position, hitInfo.point);
                            }
                        }*/

                        //float distanceToChosenInteractiveObject = Vector3.Distance(Camera.main.transform.position, chosenInteractiveObject.transform.position);
                        interaftiveInSight = true;
                        chosenInteractiveObject.ShowText();
                        // Now that we are showing the interactive object text, we can interact with it
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            chosenInteractiveObject.Interact(transform);
                        }

                        /*if (distanceToChosenInteractiveObject < distanceToSolid)
                        {
                            
                        } else
                        {
                            Debug.Log("Solid hiding the view is " + hitInfo.transform.name);
                        }*/
                    }
                }
                if (!interaftiveInSight && PlayerHUD.instance != null)
                {
                    PlayerHUD.instance.DeactivateInteractiveText();
                }
            }

            // We don't do anything if we have the inventory on
            if (PlayerInventoryUI.instance != null && PlayerInventoryUI.instance.gameObject.activeSelf)
            {
                return;
            }

            // For movement axes:
            // Get the Horizontal and Vertical axes
            _vertical = Input.GetAxis("Vertical");
            _horizontal = Input.GetAxis("Horizontal");

            // Crouch button
            if (_characterController != null) {
                if (Input.GetKeyDown(KeyCode.C) && _characterController.isGrounded)
                {
                    _characterController.height = _initialCharacterControllerHeight / 2;

                    if (_crouchSound != null && AudioManager.instance != null)
                    {
                        AudioManager.instance.PlayOneShotSound(_crouchSound, 1, 0, 0);
                    }
                }

                if (Input.GetKeyUp(KeyCode.C))
                {
                    _characterController.height = _initialCharacterControllerHeight;

                    if (_crouchSound != null && AudioManager.instance != null)
                    {
                        AudioManager.instance.PlayOneShotSound(_crouchSound, 1, 0, 0);
                    }
                }
            }

            // Run Button
            if (Input.GetKeyDown(KeyCode.LeftShift) && _characterController.isGrounded && _canRun
                // We shouldn't be able to run when it's the nightmare mansion scene
                && SceneManager.GetActiveScene().name != "Nightmare Mansion")
            {
                _isRunning = true;
                if (_equippedWeaponControl != null)
                    _equippedWeaponControl.SetRun();

                if (Flashlight.instance != null)
                    Flashlight.instance.SetRun();
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                StopRunning();
            }

            // Handling running breaths
            if (_runningSoundDelay > 0)
            {
                _runningSoundTime += Time.deltaTime;
                if (
                    // If we are running, we leave a certain threshold between heavy breaths
                    (_isRunning && _runningSoundTime >= _runningSoundDelay - .2f)
                    // Or we are not running but stamina is inferior to a certain value
                    || (!_isRunning && _runningSoundTime >= _runningSoundDelay && _staminaSharedFloat.value < 70f)
                    // But if we can't run, the margin beteween breaths should be bigger (he is recovering)
                    || (!_canRun && _runningSoundTime >= _runningSoundDelay))
                {
                    _runningSoundTime = 0f;
                    _runningSoundDelay = 0f;
                }
            }

            if (_runningSoundTime == 0
                && AudioManager.instance != null
                && _runningSounds.Count > 0
                && (_staminaSharedFloat.value < 70f || !_canRun))
            {
                AudioClip clip = _runningSounds[Random.Range(0, _runningSounds.Count)];

                if (clip != null)
                {
                    _runningSoundTime += Time.deltaTime;
                    _runningSoundDelay = clip.length;
                    AudioManager.instance.PlayOneShotSound(clip, .5f, 0, 0, transform.position);
                }
            }

            // Handle taunt
            if (Input.GetKeyDown(KeyCode.T) && AudioThreatManager.instance != null
                // We don't want to taunt in hospital
                && SceneManager.GetActiveScene().name != "Hospital")
            {
                if (AudioManager.instance != null && _tauntSounds.Count > 0)
                {
                    AudioClip tauntSound = _tauntSounds[Random.Range(0, _tauntSounds.Count)];
                    if (tauntSound != null)
                    {
                        AudioManager.instance.PlayOneShotSound(tauntSound, 1, 0, 0, transform.position);
                    }
                }

                AudioThreatManager.instance.MakeNoise(20, 1, transform.position);
            }

            // Handle decreasing stamina while running
            if (isRunning && _staminaSharedFloat != null)
            {
                float newStaminaValue = _staminaSharedFloat.value - Time.deltaTime * 10;
                _staminaSharedFloat.value = Mathf.Max(0, newStaminaValue);
            }
            // Handle Increasing stamina if not running
            else
            {
                if(_staminaSharedFloat.value != 100)
                {
                    _staminaSharedFloat.value = Mathf.Min(100, _staminaSharedFloat.value + 10 * Time.deltaTime);
                }
            } 

            // Handle Attack
            if (Input.GetKeyDown(KeyCode.Mouse0) && _equippedWeapon != 0)
            {
                if (_equippedWeapon != 4 && !_equippedWeaponControl.isReloading)
                {
                    _isShooting = true;
                    _equippedWeaponControl.SetShooting();
                } else if (_equippedWeapon == 4)
                {
                    _equippedWeaponControl.SetAttack();
                }
            }

            // Handle stop attacking
            if (Input.GetKeyUp(KeyCode.Mouse0) && _equippedWeapon != 0)
            {
                if (_equippedWeapon != 4)
                {
                    _isShooting = false;
                    _equippedWeaponControl.SetShooting();
                }
            }

            // Changing weapon
            if (Input.GetKeyDown(KeyCode.Y))
                _weaponIndex = 0;
            if (Input.GetKeyDown(KeyCode.U))
                _weaponIndex = 1;
            if (Input.GetKeyDown(KeyCode.I))
                _weaponIndex = 2;
            if (Input.GetKeyDown(KeyCode.O))
                _weaponIndex = 3;
            if (Input.GetKeyDown(KeyCode.P))
                _weaponIndex = 4;

            // Looking at the smartphone
            if (Smartphone.instance != null)
            {
                // If we click the smartphone button
                // or when we click to equip another weapon and the smartphone is equipped (we unquip it)
                if (Input.GetKeyDown(KeyCode.S) || (_weaponIndex != -1 && Smartphone.instance.looking))
                {
                    // If we equip the smartphone, we automatically go to hands mode (no more equipped weapon)
                    // By setting the weaponIndex to 0
                    if (!Smartphone.instance.looking)
                        _weaponIndex = 0;
                    Flashlight.instance.Look(false);
                    Smartphone.instance.Look(!Smartphone.instance.looking);
                }
            }

            // Handle flashlight
            if (Flashlight.instance != null && SceneManager.GetActiveScene().name != "Nightmare Mansion") {
                // We can still equip the flashlight if the selected weapon is a melee weapon
                if(Input.GetKeyDown(KeyCode.F) || (_weaponIndex != -1 && _weaponIndex != 4 && Flashlight.instance.looking))
                {
                    if (_weaponIndex != 4 && !Flashlight.instance.looking && _equippedWeapon != 4) _weaponIndex = 0;
                    Smartphone.instance.Look(false);
                    Flashlight.instance.Look(!Flashlight.instance.looking);
                }
            }

            if (_weaponIndex != -1)
            {
                _weaponSwitchCoroutine = SwitchWeaponCoroutine(_weaponIndex);
                StartCoroutine(_weaponSwitchCoroutine);
            }

            // Reload Weapon
            // We don't reload when the equipped weapon is either nothing (0) or a melee weapon (4)
            if (Input.GetKeyDown(KeyCode.R) && _equippedWeapon != 0 && _equippedWeapon != 4)
            {
                _equippedWeaponControl.Reload();
            }

            if (Input.GetKeyUp(KeyCode.R) && _equippedWeapon != 0 && _equippedWeapon != 4)
            {
                _equippedWeaponControl.StopReload();
            } 

            // Managing blood screen:
            if (_healthSharedFloat != null && _bloodScreen != null)
            {
                float newOpacity = Mathf.Lerp(_bloodScreen.color.a, Mathf.Max(Mathf.Min(.7f - _healthSharedFloat.value / 100, .6f), 0), Time.deltaTime / 2);
                _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, newOpacity);
            }
        }

        public void ResetBloodScreen()
        {
            if (_bloodScreen == null) return;

            float newOpacity = Mathf.Max(Mathf.Min(.7f - _healthSharedFloat.value / 100, .8f), 0);
            _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, newOpacity);
        }

        private void StopRunning()
        {
            _isRunning = false;

            if (_equippedWeaponControl != null)
                _equippedWeaponControl.SetRun();

            if (Flashlight.instance != null)
                Flashlight.instance.SetRun();
        }

        private void FixedUpdate()
        {
            // We don't do anything if we have the inventory on or if we are dead or if we are inside a car
            if (_dead || (PlayerInventoryUI.instance != null && PlayerInventoryUI.instance.gameObject.activeSelf) || _insideACar || !_canMove)
            {
                _desiredMovement = Vector3.zero;
                _isRunning = false;

                if (_equippedWeaponControl != null)
                    _equippedWeaponControl.SetRun();
                return;
            }

            float speed = _walkSpeed;
            if (_isRunning && _canRun)
            {
                speed = Mathf.Max(_walkSpeed + 1f, _runSpeed - (_runSpeed / 100) * (100 - _staminaSharedFloat.value));
                if (_staminaSharedFloat.value <= 0)
                {
                    _canRun = false;
                    _isRunning = false;
                    _equippedWeaponControl.SetRun();
                }
            }
            if (_staminaSharedFloat.value >= 100 && !_canRun)
            {
                _canRun = true;
            }

            float frontMove = _vertical * speed;
            float sideMove = _horizontal * speed;

            if (_characterController.isGrounded)
                _desiredMovement = transform.forward * frontMove + transform.right * sideMove;

            /*RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, LayerMask.GetMask("Default", "DecorationBase", "Stairs", "Wood", "Metal")))
            {
                // This means we are standing on a surface
                _desiredMovement = Vector3.ProjectOnPlane(_desiredMovement, hitInfo.normal);
            }*/

            // Gravity
            if (!_characterController.isGrounded)
                _desiredMovement.y -= _gravity;

            // Limiting our movement depending on the colliding enemies
            foreach (AIStateMachine collidingEnemy in _collidingEnemies)
            {
                if (collidingEnemy.dead) continue;

                Vector3 enemyDirection = collidingEnemy.transform.position - transform.position;
                if (Vector3.Angle(_desiredMovement, enemyDirection) < 60)
                {
                    _desiredMovement.x = 0;
                    //_desiredMovement.y = 0;
                }
            }

            _characterController.SimpleMove(_desiredMovement);
        }

        public void SwitchWeapon(int weaponIndex)
        {
            _weaponSwitchCoroutine = SwitchWeaponCoroutine(weaponIndex);
            StartCoroutine(_weaponSwitchCoroutine);
        }

        IEnumerator SwitchWeaponCoroutine(int weaponIndex)
        {
            _weaponIndex = -1;

            InventoryWeaponMount weaponMount = null;
            if (weaponIndex == 1) weaponMount = _playerInventory.rifle1;
            else if (weaponIndex == 2) weaponMount = _playerInventory.rifle2;
            if (weaponIndex == 3) weaponMount = _playerInventory.handgun;
            if (weaponIndex == 4) weaponMount = _playerInventory.melee;

            // If player inventory doesn't contain the weapon identified by weaponIndex, then we leave
            if (weaponIndex != 0 && weaponMount.item == null) 
            {
                _weaponSwitchCoroutine = null;
                yield break;
            }

            // When we switch the weapon and the weapon is not of type melee, we stop looking with the flashlight:
            if (Flashlight.instance != null)
            {
                if (Flashlight.instance.looking && weaponIndex != 4 && weaponIndex != 0)
                {
                    Flashlight.instance.Look(false);
                }

                if (weaponIndex == 4)
                {
                    Flashlight.instance.Look(true);
                }
            }

            float deslectionClipLength = 0f;

            _equippedWeapon = weaponIndex;


            // Then we set the previously selected weapon to inactive
            // We need to check if the weapon hasn't been destroyed in a weapon replacement case
            if (_equippedWeaponControl != null)
            {
                _equippedWeaponControl.gameObject.SetActive(false);
            }

            // If the new weapon is nothing, we select the flashlight, otherwise, we deactivate the light
            if (Flashlight.instance != null && !Flashlight.instance.looking)
            {
                Flashlight.instance.Look(_weaponIndex == 0);
                Flashlight.instance.ActivateDeactivateLight(false, 0);
            }

            // Equipped weapons starts from 0 with 0 representing the melee (no weapon) index.
            // Shared int on equipped weapon is equal to 0 when the first weapon is selected.
            // Shared int's 0 is, by identification equal to 1 in our equipped weapons
            if (_equippedWeaponSharedInt != null)
                _equippedWeaponSharedInt.value = weaponIndex - 1;

            WeaponControl newEquippedWeaponControl = null;
            // If the newly equipped weapon is no weapon, we access it directly
            if (weaponIndex == 0)
            {
                newEquippedWeaponControl = _weaponHolder.transform.GetChild(0).GetComponent<WeaponControl>();
            } // else, we access it through the player inventory
            else if (_playerInventory != null)
            {
                WeaponControl weaponControl = weaponMount.item.collectableWeapon.weaponControl;
                foreach(Transform child in weaponHolder.transform)
                {
                    if (child.GetComponent<WeaponControl>().inventoryWeapon == weaponMount.item)
                    {
                        newEquippedWeaponControl = child.GetComponent<WeaponControl>();
                    }
                }
            }

            if (_playerInventory != null && newEquippedWeaponControl != null)
            {
                // We activate the new weapons and animate it
                // But we only activate the weapon when it's not just the hands
                if (weaponIndex != 0)
                    newEquippedWeaponControl.transform.gameObject.SetActive(true);

                newEquippedWeaponControl.WeaponSelectOrDeselect(true);

                _equippedWeaponControl = newEquippedWeaponControl;

                // We wait for some time before we deactivate the unequipped weapon (all the weapons) 
                yield return new WaitForSeconds(deslectionClipLength - 1f);

                // Now, we deselect all other weapons
                foreach (Transform child in _weaponHolder.transform)
                {
                    if (child.GetComponent<WeaponControl>() != newEquippedWeaponControl)
                    {
                        child.transform.gameObject.SetActive(false);
                    }
                }
            }

            _weaponSwitchCoroutine = null;
        }

        public void TakeDamage(float damage)
        {
            // Play the damage sound
            if (_damageSounds.Count > 0 && AudioManager.instance != null)
            {
                AudioClip chosenDamageSound = _damageSounds[Random.Range(0, _damageSounds.Count)];
                if (chosenDamageSound != null)
                    AudioManager.instance.PlayOneShotSound(chosenDamageSound, 1, 0, 0);
            }

            if (_bloodScreen != null)
            {
                _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, 1);
            }

            _healthSharedFloat.value = Mathf.Max(0, _healthSharedFloat.value - damage);

            if (_healthSharedFloat.value <= 0)
            {
                _dead = true;

                // We should show the game over screen here
                GameOver();
            }
        }

        public void GameOver()
        {
            if (!_neverActuallyDies)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            if (_deadCoroutine == null)
            {
                _deadCoroutine = DeadCoroutine();
                StartCoroutine(_deadCoroutine);
            }

            //SceneManager.LoadScene(0);
        }

        IEnumerator DeadCoroutine()
        {
            // We need to empty the player inventory after we die
            if (!_neverActuallyDies)
                _playerInventory.InitializeInventory();

            // Disable character controller to stop the zombie from attacking us
            _characterController.enabled = false;

            float time = 0f;
            float delay = 2f;
            if (_deadImage == null) yield break;
            
            Text deadText = _deadImage.GetComponentInChildren<Text>();

            while(time <= delay)
            {
                time += Time.deltaTime;
                float opacity = Mathf.Lerp(0, 1, time / delay);
                _deadImage.color = new Color(_deadImage.color.r, _deadImage.color.g, _deadImage.color.b, opacity);
                if (deadText != null)
                    deadText.color = new Color(deadText.color.r, deadText.color.g, deadText.color.b, opacity);
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            // Then we start loading 
            if (Loading.instance != null && !_neverActuallyDies)
            {
                Loading.instance.SetLoading(true);
            }

            // If it's the nightmare, we load back the wasteland scene
            if (SceneManager.GetActiveScene().name == "Nightmare")
            {
                SceneManager.LoadScene("Wasteland");
            }
            else if (!_neverActuallyDies)
            {
                // We reload the current scene intead of going back to the main menu
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //SceneManager.LoadScene(0);
            }

            if (_neverActuallyDies)
            {
                time = 0;
                delay = 4;
                while (time <= delay )
                {
                    time += Time.deltaTime;
                    float opacity = Mathf.Lerp(1, 0, time / delay);
                    Debug.Log("opacity: " + opacity);
                    _deadImage.color = new Color(_deadImage.color.r, _deadImage.color.g, _deadImage.color.b, opacity);
                    if (deadText != null)
                        deadText.color = new Color(deadText.color.r, deadText.color.g, deadText.color.b, opacity);

                    
                    yield return null;
                }

                _deadImage.color = new Color(_deadImage.color.r, _deadImage.color.g, _deadImage.color.b, 0);
                deadText.color = new Color(deadText.color.r, deadText.color.g, deadText.color.b, 0);

                _healthSharedFloat.value = 100f;

                _dead = false;

                DeadEvents();
            }

            _deadCoroutine = null;
        }

        private void DeadEvents()
        {
            StartCoroutine(RemoveBloodScreen());
            _deadEvents[_numberOfTimesDead].Invoke();
            _characterController.enabled = false;
            transform.position = _deadSpawns[_numberOfTimesDead].position;
            _characterController.enabled = true;

            _numberOfTimesDead++;
        }

        private IEnumerator RemoveBloodScreen()
        {
            float time = 0;
            float delay = 4;
            while (time <= delay)
            {
                time += Time.deltaTime;
                float opacity = Mathf.Lerp(1, 0, time / delay);

                _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, opacity);

                yield return null;
            }

            _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, 0);

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Danger"))
            {
                Die();
            }

            if (other.CompareTag("Stairs") && _characterController != null)
            {
                _characterController.stepOffset = .3f;
            }
        }

        public void Die()
        {
            _healthSharedFloat.value -= 999f;
            _dead = true;
            _bloodScreen.color = new Color(_bloodScreen.color.r, _bloodScreen.color.g, _bloodScreen.color.b, 1);
            // We should show the game over screen here
            GameOver();
        }

        private void OnTriggerStay(Collider col)
        {
            if (col.gameObject.CompareTag("Enemy"))
            {
                AIStateMachine stateMachine = col.GetComponent<AIStateMachine>();
                if (!_collidingEnemies.Contains(stateMachine))
                {
                    //_collidingEnemies.Add(stateMachine);
                }
            }

            if (col.CompareTag("Stairs") && _characterController != null)
            {
                _characterController.stepOffset = .3f;
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.gameObject.CompareTag("Enemy"))
            {
                _collidingEnemies.Remove(col.GetComponent<AIStateMachine>());
            }

            if (col.CompareTag("Stairs") && _characterController != null)
            {
                _characterController.stepOffset = _initialCharacterControllerStepOffset;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.transform.CompareTag("Danger"))
            {
                Die();
            }
        }

        private void OnEnable()
        {
            if (Flashlight.instance != null)
            {
                Flashlight.instance.Look(true);
            }
        }

        public void HandleCarReversal()
        {
            float reversedDelay = 2f;

            if (_inReversedCar)
            {
                _timeSpentBeingReversed += Time.deltaTime;
                if (_timeSpentBeingReversed >= reversedDelay)
                {
                    Die();
                }
            } else
            {
                _timeSpentBeingReversed = 0;
            }
            
        }

        public void AttachDemon(Transform demon)
        {
            //if (Flashlight.instance != null) Flashlight.instance.Look(false);

            if (_attachedDemonTransform != null)
            {
                demon.transform.parent = _attachedDemonTransform;
                demon.transform.localPosition = Vector3.zero;
                demon.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            GoneWrong.Player.instance.canMove = true;

        }
    }
}

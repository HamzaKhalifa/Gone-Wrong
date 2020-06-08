using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoneWrong
{
    public enum GazType
    {
        FireExtinguisher,
        Fire
    }

    [System.Serializable]
    public class Damage
    {
        public float distance = 100f;
        public float damage = 10f;
    }

    public class WeaponControl : MonoBehaviour
    {
        [SerializeField] InventoryWeapon _inventoryWeapon = null;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private bool _decapitator = false;
        [SerializeField] private bool _damageDependsOnDistance = false;
        [SerializeField] private List<Damage> _damages = new List<Damage>();
        [SerializeField] private bool _gazWeapon = false;
        [SerializeField] private ParticleSystem _bloodParticles = null;

        [Header("For Melee")]
        [SerializeField] List<AudioClip> _hitSounds = new List<AudioClip>();
        [SerializeField] float _hitRange = 2f;

        [Header("For Guns")]
        [SerializeField] Bullet _bullet = null;
        [SerializeField] Transform _bulletPosition = null;
        [SerializeField] AudioClip _bulletSound = null;
        [SerializeField] AudioClip _outOfAmmoSound = null;
        [SerializeField] GameObject _bulletHole = null;
        [SerializeField] ParticleSystem _impact = null;
        [SerializeField] GameObject _multipleImpactObject = null;
        [SerializeField] ParticleSystem _weaponSmoke = null;
        [SerializeField] int _smokeEmitted = 5;
        [SerializeField] GameObject _muzzleFlash = null;

        [Header("For automatic gun")]
        [SerializeField] private bool _isAutomatic = false;
        [SerializeField] private float _fireRate = .2f;
        [SerializeField] private float _recoil = .1f;
        private float _fireTimer = 0f;
        private float _initialZValue = 0f;
        private float _maxZValue = 0f;
        private IEnumerator _recoilCoroutine = null;

        [Header("For Gaz")]
        [SerializeField] GazType _gazType = GazType.FireExtinguisher;

        [SerializeField] AudioClip _putAwaySound = null;
        [SerializeField] AudioClip _selectSound = null;
        [SerializeField] List<AudioClip> _reloadSounds = new List<AudioClip>();

        [Header("Shared variables")]
        [SerializeField] Inventory _playerInventory = null;
        [SerializeField] SharedInt _equippedWeapon = null;

        #region Cache Fields

        private Animator _animator = null;
        private AudioSource _audioSource = null;
        private Player _player = null;

        #endregion

        #region Private Fields

        private float _putAwayDelay = .1f;
        private float _putAwayTimer = 0f;
        private float _selectDelay = .1f;
        private float _selectTimer = 0f;
        private float _muzzleFlashActiveDelay = .05f;
        private float _muzzleFlashTimer = 0f;
        private bool _isReloading = false;
        private LayerMask _bulletLayerMask = -1;
        private bool _isRunning = false;

        #endregion


        #region Animator Hashes

        private int _isMovingHash = Animator.StringToHash("IsMoving");
        private int _isRunningHash = Animator.StringToHash("IsRunning");
        private int _selectedHash = Animator.StringToHash("Selected");
        private int _isShootingHash = Animator.StringToHash("IsShooting");
        private int _realoadWeaponHash = Animator.StringToHash("Reload");
        private int _attackHash = Animator.StringToHash("Attack");

        #endregion

        #region Public Accessors

        public Animator animator { get { return _animator; } }
        public Inventory playerInventory { get { return _playerInventory; } }
        public SharedInt equippedWeapon { get { return _equippedWeapon; } }
        public InventoryWeapon inventoryWeapon { get { return _inventoryWeapon; } }
        public bool isReloading { get { return _isReloading; } set { _isReloading = value; } }
        public bool isRunning { set { _isRunning = value; } }

        #endregion

        private void Start()
        {
            _bulletLayerMask = LayerMask.GetMask("Default", "BodyPart");

            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _player = GoneWrong.Player.instance;

            if (_isAutomatic)
            {
                _fireTimer = _fireRate;
                _initialZValue = transform.localPosition.z;
                _maxZValue = _initialZValue + _recoil;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_player != null)
            {
                _animator.SetBool(_isMovingHash, GoneWrong.Player.instance.isMoving);

                if (_isAutomatic)
                {
                    if (_player.isShooting && !_isRunning)
                    {
                        if (_fireTimer >= _fireRate)
                        {
                            Fire();
                            _fireTimer = 0f;

                            // Handle recoil (in automatic fire, there is no animation, so we handle the recoil ourselves)
                            _recoilCoroutine = Recoil();
                            StartCoroutine(_recoilCoroutine);
                        }

                        _fireTimer += Time.deltaTime;
                    }
                    else
                    {
                        _fireTimer = _fireRate;
                        _animator.enabled = true;
                    }
                }
            }

            if (_muzzleFlashTimer <= 0 && _muzzleFlash != null)
            {
                _muzzleFlash.gameObject.SetActive(false);
            }

            _putAwayTimer = Mathf.Max(0, _putAwayTimer -= Time.deltaTime);
            _selectTimer = Mathf.Max(0, _selectTimer -= Time.deltaTime);
            _muzzleFlashTimer = Mathf.Max(0, _muzzleFlashTimer -= Time.deltaTime);

        }

        private void OnEnable()
        {
            // In case we disable the weapon while reloading, the reloading is still gonna be set to true,
            // So we won't be able to fire
            // So we reset it to false
            _isReloading = false;
        }

        public void Fire()
        {
            InventoryWeaponMount weaponMount = null;
            if (_equippedWeapon.value == 0) weaponMount = _playerInventory.rifle1;
            else if (_equippedWeapon.value == 1) weaponMount = _playerInventory.rifle2;
            else if (_equippedWeapon.value == 2) weaponMount = _playerInventory.handgun;
            if (_equippedWeapon.value == 3) weaponMount = _playerInventory.melee;

            if (_playerInventory != null && _equippedWeapon != null)
            {
                // If we have rounds left in our weapon, we fire.
                if (weaponMount.rounds > 0)
                {
                    // Muzzle flash activation
                    if (_muzzleFlashTimer <= 0 && _muzzleFlash != null)
                    {
                        _muzzleFlashTimer = _muzzleFlashActiveDelay;
                        _muzzleFlash.gameObject.SetActive(!_muzzleFlash.gameObject.activeSelf);
                    }

                    // Bullet sound
                    if (_bulletSound != null && AudioManager.instance != null && !_gazWeapon)
                    {
                        AudioManager.instance.PlayOneShotSound(_bulletSound, 1, 0, 0);
                    }

                    // Emit Smoke
                    if (_bulletPosition != null)
                    {
                        if (_weaponSmoke != null)
                        {
                            _weaponSmoke.transform.position = _bulletPosition.position;
                            _weaponSmoke.Emit(_smokeEmitted);
                        }

                        if (_impact != null)
                        {
                            _impact.transform.position = _bulletPosition.position;
                            _impact.Emit(10);
                        }

                        if (_multipleImpactObject != null && !_gazWeapon)
                        {
                            _multipleImpactObject.transform.position = _bulletPosition.position;
                            foreach (Transform child in transform)
                            {
                                ParticleSystem childParticleSystem = child.GetComponent<ParticleSystem>();
                                if (childParticleSystem != null)
                                {
                                    childParticleSystem.Emit(10);
                                }
                            }
                        }
                    }

                    // Instantiate a real bullet in the scene (case of a rocket launcher or a heavy weapon with slower ammo speed)
                    if (_bulletPosition != null && _bullet != null)
                    {
                        Bullet bullet = Instantiate(_bullet, _bulletPosition.position, Quaternion.identity);
                        bullet.transform.forward = -transform.forward;
                    }
                    // We make a simple ray cast.
                    else
                    {
                        // If it's a normal fire weapon, we fire a bullet with a raycast
                        if (!_gazWeapon)
                        {
                            // We create a ray
                            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
                            RaycastHit hit;
                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _bulletLayerMask) && _bulletHole != null)
                            {
                                // If we hit an enemy, we are going to provoke damage
                                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BodyPart"))
                                {
                                    if (GoneWrong.AudioManager.instance != null && _hitSounds.Count > 0)
                                    {
                                        AudioClip clip = _hitSounds[Random.Range(0, _hitSounds.Count)];
                                        if (clip != null)
                                        {
                                            GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 1, hit.transform.position);
                                        }
                                    }

                                    // We emit blood particles at the hit point
                                    EmitBlood(hit);

                                    // Try getting the ai state machine
                                    AIStateMachine stateMachine = hit.transform.GetComponentInParent<AIStateMachine>();
                                    if (stateMachine != null)
                                    {
                                        float damage = _damage;
                                        if (_damageDependsOnDistance && _damages.Count > 0)
                                        {
                                            // By default, the damage should be equal to the least amount of damage
                                            damage = _damages[_damages.Count - 1].damage;

                                            float distance = (stateMachine.transform.position - GoneWrong.Player.instance.transform.position).magnitude;
                                            for (int i = 0; i < _damages.Count; i++)
                                            {
                                                if (distance < _damages[i].distance)
                                                {
                                                    damage = _damages[i].damage;
                                                    break;
                                                }
                                            }
                                        }

                                        // Check if we can decapitate the part
                                        AIDecapitation part = hit.transform.GetComponent<AIDecapitation>();

                                        if (_decapitator && part != null)
                                        {
                                            // Only decapitate the parts that don't cause for an instant kill
                                            if (!part.instantKill || part.CompareTag("Head"))
                                                part.DecapitatePart(hit);
                                        }

                                        stateMachine.TakeDamage(damage, Player.instance.transform.position, hit);
                                    }
                                }
                                // Else, we mot likely hit default geometry, so we instantiate the bullet hole
                                else
                                {
                                    GameObject tmp = Instantiate(_bulletHole, hit.point, Quaternion.identity);
                                    tmp.transform.forward = hit.normal;
                                    tmp.transform.parent = hit.transform;
                                }
                            }
                        } else {
                            // This is a gaz weapon
                            // Here, we cast a sphere for the gaz
                            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
                            RaycastHit hitInfo;
                            if (Physics.Raycast(ray, out hitInfo, 5f, LayerMask.GetMask("Barrier")))
                            {
                                Barrier barrier = hitInfo.transform.GetComponent<Barrier>();
                                if (barrier != null)
                                {
                                    barrier.TakeDamage(_damage);
                                }
                            }
                        }
                    }

                    // We reduce the number of rounds in our player inventory
                    weaponMount.rounds--;
                }
                else
                {
                    // We play out of ammo sound
                    if (_outOfAmmoSound != null && AudioManager.instance != null)
                    {
                        AudioManager.instance.PlayOneShotSound(_outOfAmmoSound, 1, 0, 0);
                    }

                    // We try to reload.
                    Reload();
                }
            }
        }

        public void EmitBlood(RaycastHit hit)
        {
            // We emit blood particles at the hit point
            if (EffectsManager.instance != null && _bloodParticles != null)
            {
                ParticleSystem bloodParticlesTmp = Instantiate(_bloodParticles, hit.point + (Player.instance.transform.position - transform.position).normalized / 3, Quaternion.identity);
                bloodParticlesTmp.transform.rotation = Quaternion.LookRotation(Player.instance.transform.position - transform.position);
                bloodParticlesTmp.Play();

                // We instantiate ground and wall blood splatters here:
                EffectsManager.instance.SplatterBlood(hit.transform);
            }
        }

        public void SetAttack()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(_attackHash);
            }
        }

        public void Attack(int fromRight = 1)
        {
            // We are gonna attack here by casting a ray in front of the player
            Player player = GoneWrong.Player.instance;
            if(player != null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                if (Physics.Raycast(ray, out hit, _hitRange, LayerMask.GetMask("BodyPart"))) {
                    if (GoneWrong.AudioManager.instance != null && _hitSounds.Count > 0)
                    {
                        AudioClip clip = _hitSounds[Random.Range(0, _hitSounds.Count)];
                        if (clip != null)
                        {
                            GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 1);
                        }
                    }

                    EmitBlood(hit);

                    AIStateMachine stateMachine = hit.transform.GetComponentInParent<AIStateMachine>();
                    if (stateMachine != null)
                    {
                        stateMachine.TakeDamage(_damage, player.transform.position, hit, false, fromRight);

                        // Check if we can decapitate the part
                        AIDecapitation part = hit.transform.GetComponent<AIDecapitation>();
                        if (_decapitator && part != null)
                        {
                            part.DecapitatePart(hit);
                        }
                    }
                }
            }
        }


        public float WeaponSelectOrDeselect(bool selected)
        {
            float clipLength = 0f;

            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (HasParameter("Selected", _animator))
            {
                _animator.SetBool(_selectedHash, selected);
                clipLength = 2f;
            }

            if (!selected && _putAwayTimer <= 0 && AudioManager.instance != null && _putAwaySound != null)
            {
                AudioManager.instance.PlayOneShotSound(_putAwaySound, 1, 0, 0);
                _putAwayTimer = _putAwayDelay;
            }
            if(selected && _putAwayTimer <= 0 && AudioManager.instance != null && _selectSound != null)
            {
                AudioManager.instance.PlayOneShotSound(_selectSound, 1, 0, 0);
                _selectTimer = _selectDelay;
            }

            return clipLength;
        }

        public bool Reload()
        {
            // Get the weapon mount
            InventoryWeaponMount weaponMount = null;
            if (_equippedWeapon.value == 0) weaponMount = _playerInventory.rifle1;
            else if (_equippedWeapon.value == 1) weaponMount = _playerInventory.rifle2;
            else if (_equippedWeapon.value == 2) weaponMount = _playerInventory.handgun;
            if (_equippedWeapon.value == 3) weaponMount = _playerInventory.melee;

            // Check if we have more ammo
            int bestFitAmmoIndex = -1;

            for (int i = 0; i < _playerInventory.ammo.Count; i++)
            {
                InventoryAmmoMount ammoMount = _playerInventory.ammo[i];
                if (ammoMount == null || ammoMount.item == null) continue;
                if (ammoMount.item.weapon == weaponMount.item)
                {
                    if (ammoMount.rounds > bestFitAmmoIndex)
                    {
                        bestFitAmmoIndex = i;
                    }
                }
            }

            // Return if we don't find ammo in our backpack
            if (bestFitAmmoIndex == -1) return false;

            _animator.SetBool(_isShootingHash, false);

            // We check if the number of rounds inside the weapon is inferior to the weapon capacity
            if (weaponMount.rounds < weaponMount.item.ammoCapacity && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
            {
                // If the weapon is of partial reload, we set realod to true because it's a loop animation
                if (weaponMount.item.partialReload)
                {
                    _animator.SetBool(_realoadWeaponHash, true);
                } // Else, we just trigger the loop animation
                else
                {
                    _animator.SetTrigger(_realoadWeaponHash);
                }
                
                return true;
            }

            return false;
        }

        public void StopReload()
        {
            if(_animator != null)
            {
                _animator.SetBool(_realoadWeaponHash, false);
            }
        }

        public void PlayReloadSound(int reloadSoundIndex)
        {
            if (_reloadSounds.Count > reloadSoundIndex && _reloadSounds[reloadSoundIndex] != null && AudioManager.instance != null)
            {
                AudioManager.instance.PlayOneShotSound(_reloadSounds[reloadSoundIndex], 1, 0, 0);
            }
        }

        public void DoReload()
        {
            // Get the weapon mount
            InventoryWeaponMount weaponMount = null;
            if (_equippedWeapon.value == 0) weaponMount = _playerInventory.rifle1;
            else if (_equippedWeapon.value == 1) weaponMount = _playerInventory.rifle2;
            else if (_equippedWeapon.value == 2) weaponMount = _playerInventory.handgun;
            if (_equippedWeapon.value == 3) weaponMount = _playerInventory.melee;

            // Rounds needed
            int roundsNeeded = weaponMount.item.ammoCapacity - weaponMount.rounds;

            for (int i = 0; i < _playerInventory.ammo.Count; i++)
            {
                // We get the ammo mount for each ammo we have
                InventoryAmmoMount ammoMount = _playerInventory.ammo[i];
                if (ammoMount == null || ammoMount.item == null) continue;

                // We check if the ammo applies to the equipped weapon
                if (ammoMount.item.weapon == weaponMount.item)
                {
                    // If the weapon reload type is partial, we just add one single bullet
                    if (weaponMount.item.partialReload)
                    {
                        weaponMount.rounds++;
                        ammoMount.rounds--;
                        // If the ammo mount no longer contains ammo, then we remove the ammo mount from our inventory
                        if (ammoMount.rounds == 0)
                        {
                            _playerInventory.ammo.RemoveAt(i);
                            // Repainte the playerInventory canvas
                            if (PlayerInventoryUI.instance != null)
                            {
                                PlayerInventoryUI.instance.Repaint(false);
                            }
                        }

                        // If we have attained the last round that can be loaded, then we force ourselves to stop reloading
                        if (weaponMount.rounds >= weaponMount.item.ammoCapacity)
                        {
                            _animator.SetBool(_realoadWeaponHash, false);
                        }

                        break;
                    }
                    // If the weapon reload type is non partial (a whole magazine at a time)
                    else
                    {
                        if (ammoMount.rounds > roundsNeeded)
                        {
                            weaponMount.rounds += roundsNeeded;

                            ammoMount.rounds -= roundsNeeded;

                            roundsNeeded = 0;

                            break;
                        }
                        else
                        {
                            roundsNeeded -= ammoMount.rounds;
                            weaponMount.rounds += ammoMount.rounds;
                            _playerInventory.ammo.RemoveAt(i);
                            if (PlayerInventoryUI.instance != null)
                            {
                                PlayerInventoryUI.instance.Repaint(false);
                            }
                        }
                    }
                }
            }
        }

        public static bool HasParameter(string paramName, Animator animator)
        {
            if (animator != null)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == paramName)
                        return true;
                }
                return false;
            }

            return false;
        }

        public void SetRun()
        {
            // Update animator parameters
            _animator.SetBool(_isRunningHash, Player.instance.isRunning);
        }


        public void SetShooting()
        {
            if (!_isAutomatic)
                _animator.SetBool(_isShootingHash, Player.instance.isShooting);
            
        }

        public void PlayFootstepSound()
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlayOneShotSound(
                                Player.instance.footSteps,
                                1, 0, 0);
        }

        public void ActivateFlashlight(int activate)
        {
            if (Flashlight.instance != null)
            {
                Flashlight.instance.ActivateDeactivateLight(activate == 1 ? true : false, 0);
            }
        }

        public void HandleSound(bool activate)
        {
            if(_audioSource != null)
            {
                if (activate) {
                    _audioSource.time = 0f;
                    _audioSource.Play();
                }
                else _audioSource.Stop();
            }
        }

        private IEnumerator Recoil()
        {
            _animator.enabled = false;

            float timer = 0f;
            float currentZValue = transform.localPosition.z;

            float differenceWithCurrentZValue = _maxZValue - currentZValue;

            float percentage = differenceWithCurrentZValue * 100 / _recoil;
            float delay = (_fireRate / 2) * percentage / 100;

            while (timer <= delay && differenceWithCurrentZValue > 0)
            {
                timer += Time.deltaTime;


                float nextValue = currentZValue + (differenceWithCurrentZValue * timer / delay);

                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                      nextValue);

                yield return null;
            }

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _maxZValue);

            timer = 0f;
            while (timer <= _fireRate / 2)
            {
                timer += Time.deltaTime;

                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y,
                      _maxZValue - (_recoil * timer / (_fireRate / 2)));

                yield return null;
            }

            //_animator.enabled = true;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _initialZValue);
        }
    }

}

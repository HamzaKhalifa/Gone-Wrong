using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoneWrong
{
    public class WeaponControl : MonoBehaviour
    {
        [SerializeField] InventoryWeapon _inventoryWeapon = null;
        [SerializeField][Range(1, 100)] private float _damage = 10f;
        [SerializeField] AudioClip _putAwaySound = null;
        [SerializeField] AudioClip _selectSound = null;
        [SerializeField] List<AudioClip> _reloadSounds = new List<AudioClip>();

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
        [SerializeField] GameObject _muzzleFlash = null;

        [Header("Shared variables")]
        [SerializeField] Inventory _playerInventory = null;
        [SerializeField] SharedInt _equippedWeapon = null;

        // Private
        private float _putAwayDelay = .1f;
        private float _putAwayTimer = 0f;
        private float _selectDelay = .1f;
        private float _selectTimer = 0f;
        private float _muzzleFlashActiveDelay = .05f;
        private float _muzzleFlashTimer = 0f;

        private LayerMask _bulletLayerMask = -1;

        // Hashes
        private int _isMovingHash = Animator.StringToHash("IsMoving");
        private int _isRunningHash = Animator.StringToHash("IsRunning");
        private int _selectedHash = Animator.StringToHash("Selected");
        private int _isShootingHash = Animator.StringToHash("IsShooting");
        private int _realoadWeaponHash = Animator.StringToHash("Reload");
        private int _attackHash = Animator.StringToHash("Attack");

        // Cache
        private Animator _animator = null;

        public Animator animator { get { return _animator; } }
        public Inventory playerInventory { get { return _playerInventory; } }
        public SharedInt equippedWeapon { get { return _equippedWeapon; } }
        public InventoryWeapon inventoryWeapon { get { return _inventoryWeapon; } }

        private void Start()
        {
            _bulletLayerMask = LayerMask.GetMask("Default", "BodyPart");

            _animator = GetComponent<Animator>();

            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Player.instance != null)
            {
                _animator.SetBool(_isMovingHash, GoneWrong.Player.instance.isMoving);
            }

            if (_muzzleFlashTimer <= 0 && _muzzleFlash != null)
            {
                _muzzleFlash.gameObject.SetActive(false);
            }

            _putAwayTimer = Mathf.Max(0, _putAwayTimer -= Time.deltaTime);
            _selectTimer = Mathf.Max(0, _selectTimer -= Time.deltaTime);
            _muzzleFlashTimer = Mathf.Max(0, _muzzleFlashTimer -= Time.deltaTime);
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
                    if (_bulletSound != null && AudioManager.instance != null)
                    {
                        AudioManager.instance.PlayOneShotSound(_bulletSound, 1, 0, 0);
                    }

                    // Emit Smoke
                    if (_bulletPosition != null)
                    {
                        if (_weaponSmoke != null)
                        {
                            _weaponSmoke.transform.position = _bulletPosition.position;
                            _weaponSmoke.Emit(5);
                        }
                        if (_impact != null)
                        {
                            _impact.transform.position = _bulletPosition.position;
                            _impact.Emit(10);
                        }

                        if (_multipleImpactObject != null)
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
                        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _bulletLayerMask) && _bulletHole != null)
                        {
                            // If we hit an enemy, we are going to provoke damage
                            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BodyPart"))
                            {
                                // Try getting the ai state machine
                                AIStateMachine stateMachine = hit.transform.GetComponentInParent<AIStateMachine>();
                                if (stateMachine != null)
                                {
                                    stateMachine.TakeDamage(_damage, Player.instance.transform.position, hit);
                                }
                            }
                            // Else, we mot likely hit default geometry, so we instantiate the bullet hole
                            else
                            {
                                GameObject tmp = Instantiate(_bulletHole, hit.point, Quaternion.identity);
                                tmp.transform.forward = hit.normal;
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
                if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, _hitRange, LayerMask.GetMask("BodyPart"))) { 
                    AIStateMachine stateMachine = hit.transform.GetComponentInParent<AIStateMachine>();
                    if (stateMachine != null)
                    {
                        //stateMachine.TakeDamage(_damage, player.transform.position, player.transform.position + player.transform.right);
                        stateMachine.TakeDamage(_damage, player.transform.position, hit, false, fromRight);
                        if (GoneWrong.AudioManager.instance != null && _hitSounds.Count > 0)
                        {
                            AudioClip clip = _hitSounds[Random.Range(0, _hitSounds.Count)];
                            if (clip != null)
                            {
                                GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 1);
                            }
                                
                        }
                    }
                }
            }
        }


        public float WeaponSelectOrDeselect(bool selected)
        {            float clipLength = 0f;
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
                            //_playerInventory.ammo[i].rounds = 0;
                            //_playerInventory.ammo[i].item = null;
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
                            //_playerInventory.ammo[i].rounds = 0;
                            //_playerInventory.ammo[i].item = null;
                        }
                    }
                }
            }
        }

        public static bool HasParameter(string paramName, Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
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
    }

}

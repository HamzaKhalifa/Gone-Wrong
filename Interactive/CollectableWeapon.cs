using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableWeapon : InteractiveObject
{
    [SerializeField] InventoryWeaponMount _weaponMount = null;
    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] AudioClip _pickSound = null;
    [SerializeField] GoneWrong.WeaponControl _weaponControl = null;

    [SerializeField] Vector3 _instantiateRotation = Vector3.zero;

    public GoneWrong.WeaponControl weaponControl { get { return _weaponControl; } }
    public Vector3 instantiateRotation{ get { return _instantiateRotation; } }

    protected override void Start()
    {
        base.Start();
    }

    public override bool Interact(Transform interactor)
    {
        base.Interact(interactor);

        if (PlayerInventoryUI.instance != null)
        {
            if (_weaponMount.item != null)
            {
                ReplaceWeapon(false, _weaponMount);

                return true;
            }

            return false;
        }

        return false;
    }

    public void ReplaceWeapon(bool atStart = false, InventoryWeaponMount theWeaponMount = null, int whichRifle = -1)
    {
        if (theWeaponMount.item == null) return;

        // We first get the weapon holder that we are gonna need
        GameObject weaponHolder = GoneWrong.Player.instance.weaponHolder;

        // Deciding which rifle we are gonna replace in case the weaponmount item is of type rifle
        if (whichRifle == -1)
            whichRifle = _playerInventory.rifle1.item == null ? 1 : 2;

        // Get the weapon to drop
        InventoryWeaponMount weaponToDrop = null;

        // We don't drop any weapon if we are the start of the game
        if (!atStart)
        {
            if (theWeaponMount.item.weaponType == WeaponType.Handgun)
            {
                weaponToDrop = _playerInventory.handgun;
            }
            else if (theWeaponMount.item.weaponType == WeaponType.Melee)
            {
                weaponToDrop = _playerInventory.melee;
            }
            else if (theWeaponMount.item.weaponType == WeaponType.Rifle)
            {
                weaponToDrop = whichRifle == 1 ? _playerInventory.rifle1 : _playerInventory.rifle2;
            }
        }

        // Then we drop the weapon here: 
        if (weaponToDrop != null && weaponToDrop.item != null && weaponToDrop.item.collectableWeapon != null
            /* We only replace when the weapon that's about to be equipped is different from the existing one*/
            && weaponToDrop.item != this._weaponMount.item
            // We don't have to drop anything if it's the start of the game
            && !atStart)
        {
            // We instantiate the collectable weapon
            CollectableWeapon collectableWeapon = Instantiate(weaponToDrop.item.collectableWeapon, transform.position, Quaternion.identity);
            // Then we assign the number of rounds to the collectable weapon
            collectableWeapon._weaponMount.rounds = weaponToDrop.rounds;

            // Then we unregister the weapon in player and remove it from the hierarchy
            
            // Then we find the weapon to dismiss from the hierarchy through its name
            string weaponName = collectableWeapon._weaponControl.name;
            if (weaponHolder != null)
            {
                Transform weaponControlTransform = weaponHolder.transform.Find(weaponName + "(Clone)");
                // Then once it is unregistered, we just delete it from the hierarchy
                if (weaponControlTransform != null)
                    Destroy(weaponControlTransform.gameObject);
            }
        }

        // We play the pick weapon audio sound
        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
        }

        // Then we replace the weapon mount with the new one
        if (theWeaponMount.item.weaponType == WeaponType.Handgun)
        {
            _playerInventory.handgun = theWeaponMount;
        }

        else if (theWeaponMount.item.weaponType == WeaponType.Melee)
        {
            _playerInventory.melee = theWeaponMount;
        }

        else if (theWeaponMount.item.weaponType == WeaponType.Rifle)
        {
            if (whichRifle == 1)
            {
                _playerInventory.rifle1 = theWeaponMount;
            } else
            {
                _playerInventory.rifle2 = theWeaponMount;
            }
        }

        // Then we add the weaponControl to the player weaponHolder and register it
        // We instantiate the weaponControl anywhere in the scene first
        if (_weaponControl != null)
        {
            // Get the temporary position and rotation of the weaponcontrol
            Vector3 position = _weaponControl.transform.position;
            Quaternion rotation = _weaponControl.transform.rotation;

            GoneWrong.WeaponControl weaponControl = Instantiate(_weaponControl);

            // We make of the weapon holder the parent of the weapon control that we just instantiated
            if (weaponHolder != null)
            {
                weaponControl.transform.parent = weaponHolder.transform;
                weaponControl.transform.localPosition = position;
                weaponControl.transform.localRotation = rotation;

                // If we are at the start of the game (loading the weapon from the saved game or having it in the inventory by default),
                // then we deactivate the weapon (to increase game performance)
                if (atStart)
                {
                    weaponControl.gameObject.SetActive(false);
                }
            }
        }

        // Then we switch to the raplacing weapon
        // We don't have to switch to anything if it's the start of the game
        if (!atStart)
        {
            SwitchWeapon(theWeaponMount, whichRifle);
            /*IEnumerator coroutine = SwitchWeapon(theWeaponMount, whichRifle);
            StartCoroutine(coroutine);*/
        }

        // Then we make the collectable weapon disappear from the scene
        if (!atStart)
            Destroy(gameObject);
    }

    public void SwitchWeapon(InventoryWeaponMount theWeaponMount, int whichRifle)
    {
        //yield return new WaitForSeconds(2f);

        // Then we force the player to switch to the picked weapon:
        // We get the weapon index first
        int weaponIndex = 1;
        if (theWeaponMount.item.weaponType == WeaponType.Handgun)
        {
            weaponIndex = 3;
        }

        else if (theWeaponMount.item.weaponType == WeaponType.Melee)
        {
            weaponIndex = 4;
        }

        else if (theWeaponMount.item.weaponType == WeaponType.Rifle)
        {
            weaponIndex = whichRifle;
        }

        // Then switch to the weapon
        GoneWrong.Player.instance.SwitchWeapon(weaponIndex);
    }

    public void PlayDropSound()
    {
        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
        }
    }
}

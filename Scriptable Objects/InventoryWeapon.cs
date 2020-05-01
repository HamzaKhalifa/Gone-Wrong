using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {
    Melee,
    Rifle,
    Handgun,
}

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/Weapon")]
public class InventoryWeapon : InventoryItem
{
    [SerializeField] int _ammoCapacity = 30;
    [SerializeField] WeaponType _weaponType = WeaponType.Rifle;
    [SerializeField] CollectableWeapon _collectableWeapon = null;
    [SerializeField] bool _partialReload = false;

    public int ammoCapacity { get { return _ammoCapacity; } }
    public WeaponType weaponType { get { return _weaponType; } }
    public CollectableWeapon collectableWeapon { get { return _collectableWeapon; } }
    public bool partialReload { get { return _partialReload; } }
}

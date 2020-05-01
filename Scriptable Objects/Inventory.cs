using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryWeaponMount
{
    public int rounds = 0;
    public InventoryWeapon item = null;
}

[System.Serializable]
public class InventoryAmmoMount
{
    public int rounds = 0;
    public InventoryAmmo item = null;
}

[System.Serializable]
public class InventoryConsumableMount
{
    public InventoryConsumable item = null;
}

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/Inventory")]
public class Inventory : ScriptableObject, ISerializationCallbackReceiver
{
    // Inspector Assigned
    [SerializeField] InventoryWeaponMount _rifle1Mount = new InventoryWeaponMount();
    [SerializeField] InventoryWeaponMount _rifle2Mount = new InventoryWeaponMount();
    [SerializeField] InventoryWeaponMount _handgunMount = new InventoryWeaponMount();
    [SerializeField] InventoryWeaponMount _meleeMount = new InventoryWeaponMount();

    [SerializeField] List<InventoryAmmoMount> _ammoMounts = new List<InventoryAmmoMount>();
    [SerializeField] List<InventoryConsumableMount> _consumableMounts = new List<InventoryConsumableMount>();
    [SerializeField] List<Messages> _messagesMounts = new List<Messages>();

    // Private
    private InventoryWeaponMount _rifle1 = new InventoryWeaponMount();
    private InventoryWeaponMount _rifle2 = new InventoryWeaponMount();
    private InventoryWeaponMount _handgun = new InventoryWeaponMount();
    private InventoryWeaponMount _melee = new InventoryWeaponMount();

    private List<InventoryAmmoMount> _ammo = new List<InventoryAmmoMount>();
    private List<InventoryConsumableMount> _consumables = new List<InventoryConsumableMount>();
    private List<Messages> _messages = new List<Messages>();

    // Properties
    public InventoryWeaponMount rifle1 { get { return _rifle1; } set { _rifle1 = value; } }
    public InventoryWeaponMount rifle2 { get { return _rifle2; } set { _rifle2 = value; } }
    public InventoryWeaponMount handgun { get { return _handgun; } set { _handgun = value; } }
    public InventoryWeaponMount melee { get { return _melee; } set { _melee = value; } }

    public List<InventoryAmmoMount> ammo { get { return _ammo; } set { _ammo = value; } }
    public List<InventoryConsumableMount> consumables { get { return _consumables; } set { _consumables = value; } }
    public List<Messages> messages { get { return _messages; } set { _messages = value; } }

    public void OnAfterDeserialize()
    {
        _rifle1 = new InventoryWeaponMount();
        _rifle2 = new InventoryWeaponMount();
        _handgun = new InventoryWeaponMount();
        _melee = new InventoryWeaponMount();

        _ammo.Clear();
        _consumables.Clear();
        _messages.Clear();

        if (_rifle1Mount.item != null)
        {
            _rifle1.rounds = _rifle1Mount.rounds;
            _rifle1.item = _rifle1Mount.item;
        }
        if (_rifle2Mount.item != null)
        {
            _rifle2.rounds = _rifle2Mount.rounds;
            _rifle2.item = _rifle2Mount.item;
        }
        if (_handgunMount.item != null)
        {
            _handgun.rounds = _handgunMount.rounds;
            _handgun.item = _handgunMount.item;
        }
        if (_meleeMount.item != null)
        {
            _melee.item = _meleeMount.item;
        }

        for (int i = 0; i < _ammoMounts.Count; i++)
        {
            if (_ammoMounts[i] == null) continue;

            InventoryAmmoMount ammoMount = new InventoryAmmoMount();
            ammoMount.rounds = _ammoMounts[i].rounds;
            ammoMount.item = _ammoMounts[i].item;

            if (!_ammo.Contains(_ammoMounts[i]))
                _ammo.Add(ammoMount);
        }

        for (int i = 0; i < _consumableMounts.Count; i++)
        {
            if (_consumableMounts[i] == null) continue;

            InventoryConsumableMount consumableMount = new InventoryConsumableMount();
            consumableMount.item = _consumableMounts[i].item;

            if (!_consumables.Contains(_consumableMounts[i]))
                _consumables.Add(consumableMount);
        }

        for (int i = 0; i < _messagesMounts.Count; i++)
        {
            if (_messagesMounts[i] == null) continue;

            if (!_messages.Contains(_messagesMounts[i]))
                _messages.Add(_messagesMounts[i]);
        }
    }

    public void OnBeforeSerialize()
    {
    }
}


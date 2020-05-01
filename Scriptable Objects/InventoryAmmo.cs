using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/Ammo")]
public class InventoryAmmo : InventoryItem
{
    [SerializeField] int _capacity = 30;
    [SerializeField] InventoryWeapon _weapon = null;
    [SerializeField] CollectableAmmo _collectableAmmo = null;

    public int capacity { get { return _capacity; } }
    public InventoryWeapon weapon { get { return _weapon; } }
    public CollectableAmmo collectableAmmo { get { return _collectableAmmo; } }
}

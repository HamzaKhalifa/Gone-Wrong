using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/Consumable")]
public class InventoryConsumable : InventoryItem
{
    [SerializeField] [Range(-100f, 100f)] float _healthAlteration = 0f;
    [SerializeField] [Range(-100f, 100f)] float _staminaAlteration = 0f;
    [SerializeField] [Range(-100f, 100f)] float _infectionAlteration = 0f;
    [SerializeField] CollectableConsumable _collectableConsumable = null;
    [SerializeField] AudioClip _consumeSound = null;


    public float healthAterlation { get { return _healthAlteration; } }
    public float staminaAlteration { get { return _staminaAlteration; } }
    public float infectionAlteration { get { return _infectionAlteration; } }
    public CollectableConsumable collectableConsumable { get { return _collectableConsumable; } }
    public AudioClip consumeSound { get { return _consumeSound; } }
}

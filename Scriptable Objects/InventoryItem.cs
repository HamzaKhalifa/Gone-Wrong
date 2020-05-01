using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [SerializeField] string _name = null;
    [SerializeField] [TextArea(5, 10)] string _description = null;
    [SerializeField] Sprite _image = null;
    [SerializeField] string _actionButton1Text = "Drop";
    [SerializeField] string _actionButton2Text = "Consume";
    [SerializeField] bool _showActionButton2 = true;
    [SerializeField] bool _canBeDropped = true;

    public string itemName { get { return _name; } }
    public string itemDescription { get { return _description; } }
    public Sprite image { get { return _image; } }
    public string actionButton1Text { get { return _actionButton1Text; } }
    public string actionButton2Text { get { return _actionButton2Text; } }
    public bool showActionButton2 { get { return _showActionButton2; } }
    public bool canBeDropped { get { return _canBeDropped; } }
}

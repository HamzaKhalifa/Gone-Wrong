using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Shared Variables/Int")]
public class SharedInt : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private int _defaultValue = 0;

    private int _value = 0;

    public int value { get { return _value; } set { _value = value; } }

    public void OnAfterDeserialize()
    {
        _value = _defaultValue;
    }

    public void OnBeforeSerialize() { }
}

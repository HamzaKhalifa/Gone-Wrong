using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Shared Variables/Float")]
public class SharedFloat : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private float _defaultValue = 100f;

    private float _value = 0;

    public float value { get { return _value; } set { _value = value; } }

    public void OnAfterDeserialize()
    {
        _value = _defaultValue;
    }

    public void OnBeforeSerialize() { }
}

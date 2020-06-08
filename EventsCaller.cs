using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsCaller : MonoBehaviour
{
    [SerializeField] private UnityEvent _events = null;

    private bool _calledEvents = false;

    public void InvokeEvents()
    {
        if (!_calledEvents)
        {
            _events.Invoke();
            _calledEvents = true;
        }
    }
}

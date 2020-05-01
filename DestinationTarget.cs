using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestinationTarget : MonoBehaviour
{
    [SerializeField] private UnityEvent _destinationReachedEvents = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_destinationReachedEvents != null)
            {
                // We set the last destination target with which we collided so to make the progress manager destroy this object in case we are loading another scene and still need to save.
                if (ProgressManager.instance != null)
                {
                    ProgressManager.instance.lastCollidedDestinationTarget = gameObject;
                }

                _destinationReachedEvents.Invoke();
                Destroy(gameObject);
            }
        }
    }
}

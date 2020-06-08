using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] private float _destroyTime = 3f;

    void Start()
    {
        Invoke("DestroySelf", _destroyTime);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}

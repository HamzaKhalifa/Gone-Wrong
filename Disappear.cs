using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappear : MonoBehaviour
{
    [SerializeField] private float _disappearDelay = 1f;


    public void DoDisappear()
    {
        Invoke("ActualDisappear", _disappearDelay);
    }

    private void ActualDisappear()
    {
        Destroy(gameObject);
    }
}

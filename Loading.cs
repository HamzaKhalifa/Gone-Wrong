using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    public static Loading instance = null;

    private void Awake()
    {
        instance = this;
        SetLoading(false);
    }

    public void SetLoading(bool loading)
    {
        gameObject.SetActive(loading);
    }
}

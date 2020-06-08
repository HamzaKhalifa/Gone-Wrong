using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightLight : MonoBehaviour
{
    [SerializeField] private float _followSpeed = 10f;

    // Private
    private Camera _mainCamera;
    private Vector3 _difference = Vector3.zero;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _difference = _mainCamera.transform.position - transform.position;
        }
    }

    private void Update()
    {
        if (_mainCamera == null) return;

        transform.position = _mainCamera.transform.position - _difference;
        transform.rotation = Quaternion.Lerp(transform.rotation, _mainCamera.transform.rotation, _followSpeed * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalCamera : MonoBehaviour
{
    public static ExternalCamera instance = null;

    [SerializeField] private Transform _target = null;
    [SerializeField] private float _positionSmoothness = .4f;
    [SerializeField] private float _rotationSmoothness = .2f;

    [Header("Mouse Vertical mouse movement")]
    [SerializeField] private float _mouseSensitivity = 4f;
    [SerializeField] private float _max = 40f;
    [SerializeField] private float _min = -40f;

    public Transform target { set { _target = value; } }

    // Private
    private Camera _camera = null;

    private void Start()
    {
        instance = this;
        gameObject.SetActive(false);

        _camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {

    }

    void FixedUpdate()
    {
        if (_target != null)
        {
            Vector3 positionSmoothness = Vector3.zero;
            transform.position = Vector3.SmoothDamp(transform.position, _target.transform.position, ref positionSmoothness, _positionSmoothness);

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, _target.transform.rotation.eulerAngles.y, 0), _rotationSmoothness);

            // For mouse movement
            if (_camera != null) {
                float mouseY = Input.GetAxis("Mouse Y");
                float rotAmountY = mouseY * _mouseSensitivity;

                Vector3 mouseRotation = _camera.transform.rotation.eulerAngles;
                mouseRotation.x -= rotAmountY;

                mouseRotation.x = Mathf.Min(Mathf.Max(mouseRotation.x, _min), _max);

                _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, Quaternion.Euler(mouseRotation), .4f);
            }
        }
    }
}

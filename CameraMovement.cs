using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 4f;
    [SerializeField] private GameObject _body = null;
    [SerializeField] private float _shakeIntensity = .1f;
    [SerializeField] private float _runShakeIntensity = .05f;
    [SerializeField] private float _shakeSpeed = .25f;
    [SerializeField] private float _runShakeSpeed = .5f;

    // Private
    private Vector3 _initialLocalPosition = Vector3.zero;
    private Vector3 _maxLocalPosition = Vector3.zero;
    private Vector3 _minLocalPosition = Vector3.zero;
    private bool _shakingUp = true;

    private float _xAxisClamp = 0f;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _initialLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // We don't move the camera if we have the inventory on
        if (PlayerInventoryUI.instance != null && PlayerInventoryUI.instance.gameObject.activeSelf)
        {
            return;
        }

        float horizontal = Input.GetAxis("Mouse X");
        horizontal = Mathf.Max(Mathf.Min(horizontal, .5f), -.5f);
        float vertical = Input.GetAxis("Mouse Y");
        vertical = Mathf.Max(Mathf.Min(vertical, .5f), -.5f);

        float rotAmountX = horizontal * _sensitivity;
        float rotAmountY = vertical * _sensitivity;

        _xAxisClamp -= rotAmountY;

        Vector3 targetRotCam = transform.rotation.eulerAngles;
        Vector3 targetRotBody = _body.transform.rotation.eulerAngles;

        targetRotCam.x -= rotAmountY;
        targetRotCam.z = 0;
        targetRotBody.y += rotAmountX;

        if (_xAxisClamp > 80)
        {
            _xAxisClamp = 80;
            targetRotCam.x = 80;
        }
        else if (_xAxisClamp < -90)
        {
            _xAxisClamp = -90;
            targetRotCam.x = 270;
        }


        transform.rotation = Quaternion.Euler(targetRotCam);
        _body.transform.rotation = Quaternion.Euler(targetRotBody);

        // Shaking
        if(GoneWrong.Player.instance != null && !GoneWrong.Player.instance.insideACar && GoneWrong.Player.instance.canMove)
        {
            float shakeIntensity = GoneWrong.Player.instance.isRunning ? _runShakeIntensity : _shakeIntensity;
            float shakeSpeed = GoneWrong.Player.instance.isRunning ? _runShakeSpeed : _shakeSpeed;

            _maxLocalPosition = new Vector3(_initialLocalPosition.x, _initialLocalPosition.y + shakeIntensity / 2, _initialLocalPosition.z);
            _minLocalPosition = new Vector3(_initialLocalPosition.x, _initialLocalPosition.y - shakeIntensity / 2, _initialLocalPosition.z);

            Vector3 playerDesiredMove = GoneWrong.Player.instance.desiredMovement;
            if (Mathf.Abs(playerDesiredMove.x) > 0.1f || Mathf.Abs(playerDesiredMove.z) > 0.1f) {
                if (_shakingUp)
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (shakeSpeed * Time.deltaTime), transform.localPosition.z);
                    if (transform.localPosition.y >= _maxLocalPosition.y)
                    {
                        _shakingUp = false;
                    }
                } else
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (shakeSpeed * Time.deltaTime), transform.localPosition.z);
                    if (transform.localPosition.y <= _minLocalPosition.y)
                    {
                        _shakingUp = true;
                        // If we don't have any weapon equipped
                        // or if we have a weapon equipped and don't have nor the move nor the run animation playing,
                        // We play our own footsteps sound
                        if (GoneWrong.Player.instance.equippedWeapon == 0
                            || (GoneWrong.Player.instance.equippedWeapon != 0)
                                && !GoneWrong.Player.instance.equippedWeaponControl.animator.GetCurrentAnimatorStateInfo(0).IsName("Move")
                                && !GoneWrong.Player.instance.equippedWeaponControl.animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                        {
                            if (GoneWrong.AudioManager.instance)
                            {
                                GoneWrong.AudioManager.instance.PlayOneShotSound(
                                    GoneWrong.Player.instance.footSteps,
                                    1, 0, 0);
                            }
                        }
                    }
                }
            } else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _initialLocalPosition, shakeSpeed * Time.deltaTime);
            }
        }
    }
}

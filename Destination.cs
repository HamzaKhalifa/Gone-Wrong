using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destination : MonoBehaviour
{
    public static Destination instance = null;

    [SerializeField] SharedFloat _distanceSharedFloat = null;
    [SerializeField] SharedFloat _angleWithHorizontalShredFloat = null;
    [SerializeField] SharedFloat _angleWithCameraUpSharedFloat = null;

    private Camera _camera = null;

    public Camera theCamera { get { return _camera; } set { _camera = value; } }

    private void Start()
    {
        instance = this;
        _camera = Camera.main;
    }

    private void Update()
    {
        if (GoneWrong.Player.instance != null && _distanceSharedFloat != null)
        {
            GoneWrong.Player player = GoneWrong.Player.instance;
            _distanceSharedFloat.value = (transform.position - player.transform.position).magnitude;

            float angleWithForward = Vector3.Angle(player.transform.forward, transform.position - player.transform.position);
            float angleWithRight = Vector3.Angle(player.transform.right, transform.position - player.transform.position);

            //Debug.Log("Angle with forward: " + angleWithForward);
            //Debug.Log("Angle with right: " + angleWithRight);

            if (_angleWithHorizontalShredFloat != null)
            {
                Vector3 horizontallProjectedVector = Vector3.ProjectOnPlane(transform.position - player.transform.position, player.transform.up);
                float sign = Mathf.Sign(Vector3.Cross(player.transform.forward, horizontallProjectedVector).y);
                float angleWithHorizontal = Vector3.Angle(horizontallProjectedVector, player.transform.forward) * sign;
                _angleWithHorizontalShredFloat.value = angleWithHorizontal;
            }

            if (_angleWithCameraUpSharedFloat != null)
            {
                if (_camera == null)
                {
                    _camera = FindObjectOfType<Camera>();
                }

                float angleWithCameraUp = Vector3.Angle(_camera.transform.up, transform.position - _camera.transform.position);
                _angleWithCameraUpSharedFloat.value = angleWithCameraUp;
            }
        }
    }

    public void SetNewDestination(Transform newDestination)
    {
        if (newDestination != null) 
            transform.position = newDestination.position;
    }

    public void ChangeLevelObjectiveText(string text)
    {
        if (PlayerHUD.instance != null)
        {
            PlayerHUD.instance.ChangeLevelObjectiveText(text);
            CarHUD.instance.ChangeLevelObjectiveText(text);
        }
    }

    public void ChangeMusic(AudioClip audioClip)
    {
        if (audioClip == null || GoneWrong.AudioManager.instance == null) return;

        GoneWrong.AudioManager.instance.ChangeMusic(audioClip);
    }

    public void SetFog(bool fogEnabled)
    {
        IEnumerator coroutine = ChangeFogDensity(fogEnabled ? .35f : 0f);
        StartCoroutine(coroutine);
    }

    private IEnumerator ChangeFogDensity(float fogDensity)
    {
        float currentFogDensity = RenderSettings.fogDensity;
        float delay = 10f;
        float timer = 0f;

        while (timer <= delay)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / delay;

            float nextFogDensity = 0f;
            if (fogDensity > currentFogDensity)
            {
                nextFogDensity = currentFogDensity + ((fogDensity - currentFogDensity) * normalizedTime);
            } else
            {
                nextFogDensity = currentFogDensity - ((currentFogDensity - fogDensity) * normalizedTime);
            }

            RenderSettings.fogDensity = nextFogDensity;

            yield return null;
        }

        RenderSettings.fogDensity = fogDensity;
    }

    public void SetSkyBox(Material skyBoxMaterial)
    {
        RenderSettings.skybox = skyBoxMaterial;
    }
}

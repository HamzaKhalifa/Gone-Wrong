using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovingCamera : MonoBehaviour
{
    [SerializeField] private List<Transform> _transforms = new List<Transform>();
    [SerializeField] private float _speed = 2f;
    [SerializeField] private bool _canEscape = true;
    [SerializeField] private bool _deactivatePlayerOnStart = true;

    [SerializeField] private GameObject _nextCinematicObject = null;
    [SerializeField] private UnityEvent _onFinishEvents = null;
    [SerializeField] private bool _stopAfterEnd = false;
    [SerializeField] private float _finalSpotWaitTime = 0f;

    private int _currentTransformIndex = -1;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_currentTransformIndex == -1 && _transforms.Count > 0)
        {
            _currentTransformIndex = 0;
        }

        if (GoneWrong.Player.instance != null && _deactivatePlayerOnStart)
        {
            GoneWrong.Player.instance.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_currentTransformIndex != -1 && _transforms.Count > _currentTransformIndex)
        {
            transform.position = Vector3.MoveTowards(transform.position, _transforms[_currentTransformIndex].position, _speed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _transforms[_currentTransformIndex].rotation, _speed * Time.deltaTime);

            if ((transform.position - _transforms[_currentTransformIndex].position).magnitude <= 1f)
            {
                _currentTransformIndex++;

                if (_currentTransformIndex >= _transforms.Count)
                {
                    if (!_stopAfterEnd)
                        _currentTransformIndex = 0;
                    else
                    {
                        Invoke("PassCinematic", _finalSpotWaitTime);
                    }
                }
            }
        }

        // To pass the cinematic:
        if(Input.GetKeyDown(KeyCode.Escape) && _canEscape)
        {
            PassCinematic();
        }
    }

    public void PassCinematic()
    {

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Here, we have finished the cinematic, so we activate the next object and we dectivate this one
        // We also call the on finish events
        if (_onFinishEvents != null)
        {
            _onFinishEvents.Invoke();
        }

        if (_nextCinematicObject != null)
        {
            _nextCinematicObject.SetActive(true);
        }
        else
        {
            _currentTransformIndex = 0;
        }

        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractiveDoor : InteractiveObject
{
    [SerializeField] List<GameProgress> _openProgressConditions = new List<GameProgress>();
    [SerializeField] Collider _closedBoxCollider = null;
    [SerializeField] Collider _openBoxCollider = null;
    [SerializeField] bool _twoWays = true;
    [SerializeField] bool _open = false;
    [SerializeField] bool _canOpen = true;
    [SerializeField] string _lockedText = "Its locked";
    [SerializeField] Vector3 _closedRotation = Vector3.zero;
    [SerializeField] Vector3 _openRotation = new Vector3(0, -90, 0);
    [SerializeField] bool _translate = false;
    [SerializeField] Vector3 _closedPosition = Vector3.zero;
    [SerializeField] Vector3 _openPosition = Vector3.zero;
    [SerializeField] AudioClip _openSound = null;
    [SerializeField] AudioClip _closeSound = null;
    [SerializeField] AudioClip _cantOpenSound = null;
    [SerializeField] List<GameObject> _childObjects = new List<GameObject>();
    [SerializeField] List<GameObject> _parentObjects = new List<GameObject>();

    private Quaternion destinationRotation = Quaternion.identity;
    private Vector3 destinationPosition = Vector3.zero;
    private IEnumerator _changeTextCouroutine = null;

    public bool open { get { return _open; } }
    public bool canOpen { get { return _canOpen; } }

    protected override void Start()
    {
        base.Start();

        if (_open && _canOpen)
        {
            transform.localRotation = Quaternion.Euler(_openRotation);
            if (_translate)
            {
                transform.localPosition = _openPosition;
            }
            if(_openBoxCollider != null) _openBoxCollider.enabled = true;
            if(_closedBoxCollider != null) _closedBoxCollider.enabled = false;
        } else
        {
            transform.localRotation = Quaternion.Euler(_closedRotation);
            if (_translate)
            {
                transform.localPosition = _closedPosition;
            }
            if (_openBoxCollider != null) _openBoxCollider.enabled = false;
            if (_closedBoxCollider != null) _closedBoxCollider.enabled = true;
        }

        destinationRotation = transform.localRotation;
        destinationPosition = transform.localPosition;

        ActivateDeactivateObjects(_childObjects, _open);
        ActivateDeactivateObjects(_parentObjects, !_open);
    }

    private void Update()
    {
        if (transform.localRotation != destinationRotation)
        {
            if (Quaternion.Angle(transform.localRotation, destinationRotation) > 1.5f)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, destinationRotation, 2f * Time.deltaTime);
            }
            else
            {
                if (_closeSound != null && GoneWrong.AudioManager.instance != null && !_open)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(_closeSound, 1, 1, 1, transform.position);
                }
                transform.localRotation = destinationRotation;
            }
        }

        if (transform.localPosition != destinationPosition && _translate)
        {
            if ((transform.localPosition - destinationPosition).magnitude > .01f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, destinationPosition, 2f * Time.deltaTime);
            }
            else
            {
                if (_closeSound != null && GoneWrong.AudioManager.instance != null && !_open)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(_closeSound, 1, 1, 1, transform.position);
                }
                transform.localPosition = destinationPosition;
            }
        }
    }

    public bool ProgressionConditionsMet()
    {
        bool validProgress = true;

        if (_openProgressConditions.Count > 0 && ProgressManager.instance != null)
        {
            foreach (GameProgress entry in _openProgressConditions)
            {
                if (!ProgressManager.instance.VerifyProgress(entry.key, entry.value))
                {
                    validProgress = false;
                }
            }
        }

        return validProgress;
    }

    public override bool Interact(Transform interactor)
    {
        bool validProgress = ProgressionConditionsMet();

        if (!_canOpen || !validProgress)
        {
            if (_cantOpenSound != null && GoneWrong.AudioManager.instance != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(_cantOpenSound, 1, 1, 1, transform.position);
            }

            _text = _lockedText;
            if (_changeTextCouroutine != null)
                StopCoroutine(_changeTextCouroutine);
            _changeTextCouroutine = ChangeText();
            StartCoroutine(_changeTextCouroutine);

            return false;
        }

        // Here we call the base.Interact() to invoke the events
        base.Interact(interactor);

        // If we are about to end the level and load the next scene
        if (_endLevel)
        {
            if (ProgressManager.instance != null)
            {
                ProgressManager.instance.LoadScene(_nextScene);
            }
            
            return false;
        }

        if (_closedBoxCollider != null) _closedBoxCollider.enabled = _open;
        if (_openBoxCollider != null) _openBoxCollider.enabled = !_open;

        _open = !_open;

        Vector3 destinationRotationVector = _open ? _openRotation : _closedRotation;

        if (_twoWays)
        {
            float angleBetweenDoorAndInteractor = Vector3.Angle(transform.forward, interactor.forward);

            // If the forward of the door is pointing upwards, we use the right
            if (transform.forward.y > 0.5f)
            {
                angleBetweenDoorAndInteractor = Vector3.Angle(transform.right, interactor.forward);
            }

            if (angleBetweenDoorAndInteractor < 40 && destinationRotationVector == _openRotation)
            {
                destinationRotationVector = -destinationRotationVector;
            }
        }

        destinationRotation = Quaternion.Euler(destinationRotationVector);

        if (_translate)
        {
            destinationPosition = _open ? _openPosition : _closedPosition;
        }

        if (_openSound != null && GoneWrong.AudioManager.instance != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_openSound, 1, 1, 1, transform.position);
        }

        ActivateDeactivateObjects(_childObjects, _open);
        ActivateDeactivateObjects(_parentObjects, !_open);

        return true;
    }

    private void ActivateDeactivateObjects(List<GameObject> objects, bool activate)
    {
        if (objects.Count > 0)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                {
                    Collider[] colliders = objects[i].GetComponents<Collider>();
                    foreach(Collider collider in colliders)
                    {
                        // We only deactivate the trigger colliders
                        collider.enabled = activate;
                    }
                }
            }
        }
    }

    private IEnumerator ChangeText()
    {
        yield return new WaitForSeconds(3f);

        _text = _interactiveText;
        _changeTextCouroutine = null;
    }
}

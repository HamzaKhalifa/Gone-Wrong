using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControlZoom : MonoBehaviour
{
    // Inspect Assigned Fields
    [SerializeField] private Sprite _scopeSprite = null;
    [SerializeField] private float _scopeDelay = 1f;
    [SerializeField] private List<GameObject> _toHideElements = new List<GameObject>();
    [SerializeField] private float _zoomFvwAddition = 30f;

    // Cache Fields
    private Animator _animator = null;

    // Animator Hashes
    int _isZoomingHash = Animator.StringToHash("IsZooming");

    // Private
    private bool _isZooming = false;
    private IEnumerator _coroutine = null;
    private float _fvw = 0f;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _fvw = Camera.main.fieldOfView;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            _isZooming = !_isZooming;
            if (ScopeUI.instance != null)
            {
                _animator.SetLayerWeight(1, _isZooming ? 1 : 0);
                _animator.SetBool(_isZoomingHash, _isZooming);

                if (_coroutine != null) StopCoroutine(_coroutine);
                _coroutine = SetScope();
                StartCoroutine(_coroutine);
            }
        }
    }

    private IEnumerator SetScope()
    {
        if (_isZooming)
            yield return new WaitForSeconds(_scopeDelay);

        foreach(GameObject element in _toHideElements)
        {
            if (element != null)
                element.SetActive(!_isZooming);
        }

        yield return null;

        ScopeUI.instance.SetScopeImage(_scopeSprite, _isZooming);

        Camera.main.fieldOfView = _isZooming ? _fvw - _zoomFvwAddition : _fvw;

        _coroutine = null;
    }
}

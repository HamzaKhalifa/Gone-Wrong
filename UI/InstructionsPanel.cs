using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsPanel : MonoBehaviour
{
    [SerializeField] private AudioClip _togglePanelSound = null;
    [SerializeField] private float _animationSpeed = .5f;

    // Cache Fields
    private RectTransform _rectTransform = null;
    private IEnumerator _coroutine = null;
    private float _showingHeight = 0f;

    // Private Fields
    private bool _isShowing = true;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _showingHeight = _rectTransform.rect.height;
    }

    private void Update()
    {
        if (_rectTransform == null) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = AnimateCoroutine();
            StartCoroutine(_coroutine);
        }
    }

    private IEnumerator AnimateCoroutine()
    {
        if (GoneWrong.AudioManager.instance != null && _togglePanelSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_togglePanelSound, 1, 0, 1);
        }

        _isShowing = !_isShowing;
        float delay = _animationSpeed;
        float time = 0f;

        if (_isShowing)
            time = _rectTransform.rect.height / _showingHeight;
        else
            time = 1 - (_rectTransform.rect.height / _showingHeight);

        while (time <= delay)
        {
            time += Time.deltaTime;
            float normalizedTime = time / delay;

            float newHeight = 0f;

            if (_isShowing)
                newHeight = _showingHeight * normalizedTime;
            else
                newHeight = _showingHeight - (_showingHeight * normalizedTime);

            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, newHeight);

            yield return null;
        }

        if (_isShowing)
        {
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _showingHeight);
        } else
        {
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, 0);
        }

        _coroutine = null;

    }
}

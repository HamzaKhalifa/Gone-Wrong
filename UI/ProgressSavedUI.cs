using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressSavedUI : MonoBehaviour
{
    public static ProgressSavedUI instance = null;

    [SerializeField] float _showingTime = 4f;

    private Text _savedText = null;
    private IEnumerator _coroutine = null;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _savedText = GetComponentInChildren<Text>();
        if (_savedText != null)
            _savedText.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_savedText == null) return;

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        _coroutine = ShowingCoroutine();
        StartCoroutine(_coroutine);
    }

    private IEnumerator ShowingCoroutine()
    {
        _savedText.gameObject.SetActive(true);

        yield return new WaitForSeconds(_showingTime);

        _savedText.gameObject.SetActive(false);

        _coroutine = null;

    }
}

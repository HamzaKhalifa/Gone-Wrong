using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartingScreen : MonoBehaviour
{
    public static StartingScreen instance = null;

    [SerializeField] float _fadeTime = 5f;

    private Image _blackScreen = null;

    public float fadeTime { get { return _fadeTime; } }

    void Start()
    {
        instance = this;

        _blackScreen = GetComponent<Image>();
        if (_blackScreen != null)
        {
            _blackScreen.color = new Color(_blackScreen.color.r, _blackScreen.color.g, _blackScreen.color.b, 1);
            IEnumerator coroutine = Fade(true);
            StartCoroutine(coroutine);
        }
    }

    public void MakeScreenBlack()
    {
        IEnumerator coroutine = Fade(false);
        StartCoroutine(coroutine);
    }

    IEnumerator Fade(bool fade)
	{
        float time = 0f;

        while (time < _fadeTime)
        {
            float _normalizedTime = time / _fadeTime;
            float nextValue = fade ? 1 - _normalizedTime : _normalizedTime;

            time += Time.deltaTime;
            _blackScreen.color = new Color(_blackScreen.color.r, _blackScreen.color.g,
                    _blackScreen.color.b, nextValue);

            yield return null;
        }
	}
}

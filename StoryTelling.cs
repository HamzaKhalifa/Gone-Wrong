using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryTelling : MonoBehaviour
{
    [SerializeField] private List<Text> _textsTexts = new List<Text>();
    [SerializeField] private float _typingSpeed = .1f;
    [SerializeField] private Button _nextButton = null;
    [SerializeField] private int _nextSceneIndex = 2;

    [Header("Sounds")]
    [SerializeField] private AudioClip _nextTextSound = null;
    [SerializeField] private List<AudioClip> _letterSounds = new List<AudioClip>();

    private List<string> _texts = new List<string>();
    private int _index = 0;
    private bool _canPass = true;

    private void Start()
    {
        for (int i = 0; i < _textsTexts.Count; i++)
        {
            _texts.Add(_textsTexts[i].text);
            _textsTexts[i].text = "";
        }
    }

    public void NextText()
    {
        if (_index < _texts.Count && _canPass)
        {
            _nextButton.gameObject.SetActive(false);

            // _textsTexts[_index].text = _texts[_index];
            IEnumerator coroutine = WriteText(_textsTexts[_index], _texts[_index]);
            StartCoroutine(coroutine);

            if (GoneWrong.AudioManager.instance != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(_nextTextSound, 1, 0, 0);
            }
        }

        _index++;

        if (_index > _texts.Count)
        {
            // This is where we load the next scene
            if (ProgressManager.instance != null)
            {
                ProgressManager.instance.LoadScene(_nextSceneIndex);
            } else 
                SceneManager.LoadScene(_nextSceneIndex);
        }
    }

    private IEnumerator WriteText(Text text, string theText)
    {
        _canPass = false;
        for (int i = 0; i < theText.Length; i++)
        {
            text.text = text.text + theText[i];

            if (GoneWrong.AudioManager.instance != null)
            {
                AudioClip clip = _letterSounds[Random.Range(0, _letterSounds.Count)];
                if (clip != null)
                    GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 0);
            }

            yield return new WaitForSeconds(_typingSpeed);
        }

        text.text = theText;

        yield return new WaitForSeconds(1f);

        _nextButton.gameObject.SetActive(true);

        _canPass = true;
    }
}

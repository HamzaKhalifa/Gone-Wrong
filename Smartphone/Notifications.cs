using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifications : MonoBehaviour
{
    public static Notifications instance = null;

    [SerializeField] private List<Text> _notificationTexts = new List<Text>();
    [SerializeField] private float _showNotificationDelay = 5f;
    [SerializeField] private AudioClip _notificationSound = null;

    private Queue<string> _queue = new Queue<string>();
    private IEnumerator _coroutine = null;

    private void Awake()
    {
        instance = this;

        foreach(Text notificationText in _notificationTexts)
        {
            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (_queue.Count > 0 && _coroutine == null)
        {
            _coroutine = ShowNotification();
            StartCoroutine(_coroutine);
        }
    }

    public void EnqueNotification(string notification)
    {
        _queue.Enqueue(notification);
    }

    private IEnumerator ShowNotification()
    {

        if (GoneWrong.AudioManager.instance != null && _notificationSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_notificationSound, 1, 0, 0);
        }

        string theNotification = "";
        if (_queue.Count > 0)
            theNotification = _queue.Dequeue();

        foreach (Text notificationText in _notificationTexts)
        {
            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(true);
                notificationText.text = theNotification;
            }
        }

        yield return new WaitForSeconds(_showNotificationDelay);

        foreach (Text notificationText in _notificationTexts)
        {
            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }

        _coroutine = null;
    }

}

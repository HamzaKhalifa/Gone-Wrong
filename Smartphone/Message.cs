using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    [SerializeField] Text _callerNameText = null;
    [SerializeField] Image _callerImage = null;
    [SerializeField] Text _callerMessageTitle = null;

    private Messages _message = null;

    public Messages message { get { return _message; } }

    public void Initialize(Messages message)
    {
        _message = message;
        _callerNameText.text = _message.callerName;
        _callerImage.sprite = _message.callerImage;
        _callerMessageTitle.text = _message.title;
    }
}

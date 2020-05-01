using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Smartphone/Message")]
public class Messages : ScriptableObject
{
    [SerializeField] private AudioClip _messageAudioClip = null;
    [SerializeField] private string _title = "";
    [SerializeField] private Sprite _callerImage = null;
    [SerializeField] private string _callerName = "";
    [SerializeField] private int _age = 0;
    [SerializeField][TextArea(10, 15)] private string _messageText = "";

    public AudioClip messageAudioClip { get { return _messageAudioClip; } }
    public string title { get { return _title; } }
    public Sprite callerImage { get { return _callerImage; } }
    public string callerName { get { return _callerName; } }
    public int age { get { return _age; } }
    public string messageText { get { return _messageText; } }
}

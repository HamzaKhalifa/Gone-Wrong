using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartphoneUI : MonoBehaviour
{
    public static SmartphoneUI instance = null;

    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] Message _messagePrefab = null;
    [SerializeField] AudioClip _hoverMessageSound = null;
    [SerializeField] AudioClip _selectMessageSound = null;

    [SerializeField] private GameObject _messagesListContainer = null;
    [SerializeField] private Text _nameAndTimeText = null;
    [SerializeField] private GameObject _messagesContainer = null;

    [Header("Single Message parts")]
    [SerializeField] private GameObject _singleMessageContainer = null;
    [SerializeField] private Image _callerImage = null;
    [SerializeField] private Text _callerNameText = null;
    [SerializeField] private Text _callerAgeText = null;
    [SerializeField] private Text _messageTitleText = null;
    [SerializeField] private Slider _messageSlider = null;
    [SerializeField] private Text _messageContentText = null;

    private int _hoveredMessageIndex = -1;
    private List<Message> _messagePrefabs = new List<Message>();
    private int _selectedMessageIndex = -1;
    private ScrollRect _scrollRect = null;

    // Cache variables
    private AudioSource _audioSource = null;

    private void Awake()
    {
        instance = this;

        _audioSource = GetComponent<AudioSource>();

        // Instantiate messages in our smartphone based off what is entered in the player inventory
        if (_playerInventory != null && _playerInventory.messages.Count > 0)
        {
            foreach(Messages message in _playerInventory.messages)
            {
                Message tmp = Instantiate(_messagePrefab, _messagesContainer.transform);
                _messagePrefabs.Add(tmp);
                tmp.Initialize(message);
            }

            _hoveredMessageIndex = 0;
        }

        _scrollRect = _messagesListContainer.GetComponent<ScrollRect>();
    }

    public void Repaint ()
    {
        _selectedMessageIndex = -1;
        _hoveredMessageIndex = -1;
        _messagePrefabs.Clear();

        if (_scrollRect != null)
        {
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        // Destroy all the already existing messages
        foreach (Transform child in _messagesContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Instantiate messages in our smartphone based off what is entered in the player inventory
        if (_playerInventory != null && _playerInventory.messages.Count > 0)
        {
            foreach (Messages message in _playerInventory.messages)
            {
                Message tmp = Instantiate(_messagePrefab, _messagesContainer.transform);
                _messagePrefabs.Add(tmp);
                tmp.Initialize(message);
            }

            _hoveredMessageIndex = 0;
        }
    }

    public void Update()
    {
        if (_nameAndTimeText != null)
        {
            _nameAndTimeText.text = GoneWrong.Player.instance.playerName;
        }

        // If we are not looking at the smartphone, then we don't need to do anything
        if (Smartphone.instance == null || !Smartphone.instance.looking)
            return;

        // Now we handle the hovered message
        if (Input.GetKeyDown(KeyCode.W) && _messagePrefabs.Count > 0)
        {
            // We play the hover message sound
            if (GoneWrong.AudioManager.instance != null && _hoverMessageSound != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(_hoverMessageSound, 1, 0, 0);
            }

            _hoveredMessageIndex++;
            if (_hoveredMessageIndex >= _messagePrefabs.Count)
            {
                _hoveredMessageIndex = _messagePrefabs.Count - 1;
            }

            HandleScroll();
        }

        if (Input.GetKeyDown(KeyCode.Q) && _messagePrefabs.Count > 0)
        {
            // We play the hover message sound
            if (GoneWrong.AudioManager.instance != null && _hoverMessageSound != null)
            {
                GoneWrong.AudioManager.instance.PlayOneShotSound(_hoverMessageSound, 1, 0, 0);
            }

            _hoveredMessageIndex--;
            if (_hoveredMessageIndex <= 0)
            {
                _hoveredMessageIndex = 0;
            }

            HandleScroll();
        }


        // Here we deselect all messages but the selected message
        for (int i = 0; i < _messagePrefabs.Count; i++)
        {
            if (i != _hoveredMessageIndex)
            {
                _messagePrefabs[i].GetComponent<Image>().fillCenter = false;
            }
        }

        //Here we try to change the UI of the selected message
        if (_hoveredMessageIndex != -1)
        {
            _messagePrefabs[_hoveredMessageIndex].GetComponent<Image>().fillCenter = true;
        }

        // We select the message once we click the enter button
        if (Input.GetKeyDown(KeyCode.Return) && _hoveredMessageIndex != -1)
        {
            SelectMessage(_messagePrefabs[_hoveredMessageIndex].message, _hoveredMessageIndex);
        }

        // We update the selected message clip length slider
        if (_messageSlider != null && _selectedMessageIndex != -1 && _audioSource != null)
        {
            _messageSlider.value = _audioSource.time / _audioSource.clip.length;
        }

        // Handle click selected message button
        if (Input.GetKeyDown(KeyCode.A) && _selectedMessageIndex != -1)
        {
            DeselectMessage();
        }
    }

    public void HandleScroll()
    {
        if (_scrollRect != null)
        {
            float scrollAmount;
            //scrollAmount = ((Mathf.Floor(_hoveredMessageIndex / 3) * 3) - 1 ) * _scrollValue;
            scrollAmount = (1 - (_hoveredMessageIndex / (float)(_messagePrefabs.Count - 1)));
            Debug.Log(scrollAmount);
            _scrollRect.verticalNormalizedPosition = scrollAmount;
        }
    }

    public void SelectMessage(Messages message, int messageIndex)
    {
        // We play the select message sound
        if (GoneWrong.AudioManager.instance != null && _selectMessageSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_selectMessageSound, 1, 0, 0);
        }

        if (_messagesListContainer != null)
            _messagesListContainer.SetActive(false);
        if (_singleMessageContainer != null)
            _singleMessageContainer.SetActive(true);

        _callerImage.sprite = message.callerImage;
        _callerNameText.text = message.callerName;
        _callerAgeText.text = message.age + "";
        _messageTitleText.text = message.title;
        _messageContentText.text = message.messageText;

        // We set the selected message index to keep track of the clip lenght slider
        _selectedMessageIndex = messageIndex;
        if (_audioSource != null)
        {
            _audioSource.clip = message.messageAudioClip;
            _audioSource.Play();
        }
    }

    public void DeselectMessage()
    {
        _selectedMessageIndex = -1;
        // And we stop the audio source
        _audioSource.clip = null;
        _audioSource.Stop();

        if (_messagesListContainer != null)
            _messagesListContainer.SetActive(true);
        if (_singleMessageContainer != null)
            _singleMessageContainer.SetActive(false);
    }
}

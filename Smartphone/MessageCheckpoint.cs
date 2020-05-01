using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MessageCheckpoint : MonoBehaviour
{
    [SerializeField] private List<Messages> _messages = new List<Messages>();
    [SerializeField] private Inventory _playerInventory = null;
    [SerializeField] private UnityEvent _onReceiveMessageEvent = null;

    private bool _sentMessage = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_sentMessage)
        {
            if (_messages.Count >  0 && _playerInventory != null && SmartphoneUI.instance != null)
            {
                foreach(Messages message in _messages)
                {
                    // We check if the player inventory contains the message or not
                    if (!_playerInventory.messages.Contains(message))
                        _playerInventory.messages.Add(message);
                    else
                        // If the player inventory already contains the messages because it was saved, we destroy it
                        Destroy(gameObject);
                }

                SmartphoneUI.instance.Repaint();
                if (Notifications.instance != null)
                {
                    Notifications.instance.EnqueNotification("You just received a new message in your mailbox. Press \"S\" To check.");
                    Notifications.instance.EnqueNotification("You now have " + _playerInventory.messages.Count + " message(s) in your mailbox. Press \"S\" to check.");
                }

                _sentMessage = true;

                // Now we invoke the events
                _onReceiveMessageEvent.Invoke();
            }

            Destroy(gameObject);
        }
    }
}

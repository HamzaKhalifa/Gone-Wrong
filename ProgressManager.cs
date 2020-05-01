using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

[System.Serializable]
public class GameProgress {
    public string key = "";
    public string value = "";
}

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager instance = null;

    [SerializeField] List<GameProgress> _progressStates = new List<GameProgress>();
    [SerializeField] private Inventory _playerInventory = null;
    [SerializeField] private Transform _playerFinalPosition = null;
    [SerializeField] private UnityEvent _startEvents = null;

    private int sceneToLoadIndex = 0;
    private GameObject _lastCollidedDestinationTarget = null;

    public GameObject lastCollidedDestinationTarget { set { _lastCollidedDestinationTarget = value; } }

    void Awake()
    {
        instance = this;

        // If it's the hospital, we try to check if we were inside the hospital or not:
        if(SaveGame.instance != null)
        {
            SavedData savedData = SaveGame.instance.GetSavedData();
            if (savedData != null)
            {
                // If the currently saved scene is not the hospital, it means that we just came back to the hospital
                // From a different area.
                if (savedData.currentScene != 1 && GoneWrong.Player.instance != null && _playerFinalPosition)
                {
                    GoneWrong.Player.instance.transform.position = _playerFinalPosition.position;
                    GoneWrong.Player.instance.transform.rotation = _playerFinalPosition.rotation;
                }
            }
        }
    }

    private void Start()
    {
        switch(SceneManager.GetActiveScene().name)
        {
            case "Hospital":
                if (Notifications.instance != null && _playerInventory != null && _playerInventory.messages.Count > 0)
                {
                    Notifications.instance.EnqueNotification("You have " + _playerInventory.messages.Count + " message(s) in your mailbox. Press \"S\" to check.");
                }
                break;
        }

        if (_startEvents != null)
        {
            _startEvents.Invoke();
        }
    }

    public bool VerifyProgress(string key, string value)
    {
        foreach(GameProgress progress in _progressStates)
        {
            if (progress.key == key && progress.value == value)
            {
                return true;
            }
        }

        return false;
    }

    public void SetProgress(string key, string value)
    {
        foreach (GameProgress progress in _progressStates)
        {
            if (progress.key == key)
            {
                progress.value = value;
                return;
            }
        }

        // We get here when we didn't find the key
        GameProgress gameProgress = new GameProgress();
        gameProgress.key = key;
        gameProgress.value = value;

        _progressStates.Add(gameProgress);
    }

    public void LoadScene(int sceneIndex)
    {
        // We disable the characterController to prevent him from moving
        if (GoneWrong.Player.instance != null)
        {
            GoneWrong.Player.instance.canMove = false;
        }

        // We also disable PlayerHUD to hide the next objectives texts from showing
        if (PlayerHUD.instance != null) PlayerHUD.instance.gameObject.SetActive(false);

        float fadeTime = 0f;
        // We make the screen black first
        if (StartingScreen.instance != null)
        {
            StartingScreen.instance.MakeScreenBlack();
            fadeTime = StartingScreen.instance.fadeTime;
        }

        // Before saving the game, we need to destroy the last destination target we collided with
        if (_lastCollidedDestinationTarget != null) Destroy(_lastCollidedDestinationTarget);

        sceneToLoadIndex = sceneIndex;

        Invoke("ActualLoadScene", fadeTime);
    }

    private void ActualLoadScene()
    {
        // We save the game before loading anything
        if (SaveGame.instance != null) SaveGame.instance.Save();

        if (Loading.instance != null) Loading.instance.SetLoading(true);

        SceneManager.LoadScene(sceneToLoadIndex);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

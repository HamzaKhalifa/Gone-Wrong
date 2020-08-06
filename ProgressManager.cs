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
    [SerializeField] private Animator _blinkAnimator = null;

    private int _sceneToLoadIndex = 0;
    private GameObject _lastCollidedDestinationTarget = null;
    // For end level doors
    private int _nextScene = -1;

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
        _nextScene = sceneIndex;

        // We disable the characterController to prevent him from moving
        if (GoneWrong.Player.instance != null)
        {
            GoneWrong.Player.instance.canMove = false;
        }

        // We also disable PlayerHUD to hide the next objectives texts from showing
        if (PlayerHUD.instance != null) PlayerHUD.instance.gameObject.SetActive(false);

        // And we deactivate the Playerinventory UI
        if (PlayerInventoryUI.instance != null) PlayerInventoryUI.instance.gameObject.SetActive(false);

        // Destroy all zombies so that they don't attack the player when loading the new scene
        AIStateMachine[] stateMachines = FindObjectsOfType<AIStateMachine>();
        foreach (AIStateMachine stateMachine in stateMachines)
        {
            stateMachine.gameObject.SetActive(false);
        }

        float fadeTime = 0f;
        // We make the screen black first
        if (StartingScreen.instance != null)
        {
            StartingScreen.instance.MakeScreenBlack();
            fadeTime = StartingScreen.instance.fadeTime;
        }

        // Before saving the game, we need to destroy the last destination target we collided with
        if (_lastCollidedDestinationTarget != null) Destroy(_lastCollidedDestinationTarget);

        _sceneToLoadIndex = sceneIndex;

        Invoke("ActualLoadScene", fadeTime);
    }

    private void ActualLoadScene()
    {
        StartCoroutine(LoadSceneCoroutine());
    }

    private IEnumerator LoadSceneCoroutine()
    {
        // There is an automatic save data, before going from a level to another
        // We only save the game when we aren't in the main men
        if (SaveGame.instance != null && SceneManager.GetActiveScene().buildIndex != 0) SaveGame.instance.Save(_nextScene);

        // Remove the sound only when it's not the main menu
        if (SceneManager.GetActiveScene().buildIndex != 0)
            Camera.main.GetComponent<AudioListener>().enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Loading loadingInstance = Loading.instance;
        if (loadingInstance != null)
        {
            Loading.instance.SetLoading(true);
            DontDestroyOnLoad(loadingInstance);
        }

        yield return SceneManager.LoadSceneAsync(_sceneToLoadIndex);

        DontDestroyOnLoad(gameObject);
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        Destroy(loadingInstance);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Destroy(gameObject);
    }

    public void ChangePlayerPosition(Transform newPlayerPosition)
    {
        GoneWrong.Player player = GoneWrong.Player.instance;
        if (player != null && newPlayerPosition != null)
        {
            // We deactivate the player character controller first
            player.characterController.enabled = false;
            player.transform.position = newPlayerPosition.position;
            player.transform.rotation = newPlayerPosition.rotation;
            player.characterController.enabled = true;
        }
    }

    public void Blink()
    {
        if (_blinkAnimator != null)
        {
            _blinkAnimator.SetTrigger("Blink");
        }
    }

    public void IncreaseThisAudioSourcePitch(float pitch)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.pitch = pitch;
        }
    }

    public void DropConsumableByName(string name)
    {
        if(PlayerInventoryUI.instance != null)
        {
            PlayerInventoryUI.instance.DropConsumableByName(name);
        }
    }

    public void PlayLastMessage()
    {
        if (SmartphoneUI.instance != null)
        {
            SmartphoneUI.instance.EnqueueLastMessage();
        }
    }

    public void TheEnd()
    {
        Invoke("TheEndMainMenu", 10);
    }

    public void TheEndMainMenu()
    {
        LoadScene(0);
    }

    public void PlaySound(AudioClip clip)
    {
        GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 1, transform.position);
    }
}


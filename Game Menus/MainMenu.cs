using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button _continueButton = null;
    [SerializeField] GameObject _loadingText = null;
    [SerializeField] GameObject _areYouSurePanel = null;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Check if we have got some save data
        if (_continueButton != null)
        {
            bool showContinueButton = SaveGame.instance != null && SaveGame.instance.savedData != null;
            _continueButton.gameObject.SetActive(showContinueButton);
        }

        HandlePanel(false);

        SetLoading(false);
    }

    public void SetLoading(bool loading)
    {
        if (_loadingText != null)
            _loadingText.gameObject.SetActive(loading);
    }

    public void NewGameButton()
    {
        bool showPanel = SaveGame.instance != null && SaveGame.instance.savedData != null;
        if (!showPanel) NewGame();
        else
        {
            HandlePanel(true);
        }
    }

    public void HandlePanel(bool show)
    {
        if (_areYouSurePanel != null)
        {
            _areYouSurePanel.gameObject.SetActive(show);
        }
    }

    public void NewGame()
    {
        HandlePanel(false);

        SetLoading(true);

        if (SaveGame.instance != null) SaveGame.instance.DeleteSaveGame();
        SceneManager.LoadScene(1);
    }

    public void LoadScene(int index)
    {
        SetLoading(true);
        SceneManager.LoadScene(index);
    }

    public void QuitGame()
    {
        // save any game data here
        SetLoading(true);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
         Application.Quit();
        #endif
    }

    public void Continue()
    {
        if (SaveGame.instance != null && SaveGame.instance.savedData != null)
        {
            SetLoading(true);
            SceneManager.LoadScene(SaveGame.instance.savedData.currentScene);
        }
    }
}

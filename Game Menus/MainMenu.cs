using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button _continueButton = null;
    [SerializeField] GameObject _loadingPanel = null;
    [SerializeField] GameObject _areYouSurePanel = null;
    [SerializeField] Inventory _playerInventory = null;

    [Header("Sounds")]
    [SerializeField] private AudioClip _clickSound = null;
    [SerializeField] private AudioClip _cancelSound = null;
    [SerializeField] private AudioClip _hoverSound = null;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Check if we have got some save data
        if (_continueButton != null)
        {
            bool showContinueButton = SaveGame.instance != null && SaveGame.instance.GetSavedData() != null;
            _continueButton.gameObject.SetActive(showContinueButton);
        }

        HandlePanel(false, false);

        SetLoading(false);
    }

    public void SetLoading(bool loading)
    {
        if (_loadingPanel != null)
            _loadingPanel.gameObject.SetActive(loading);
    }

    public void NewGameButton()
    {
        if(GoneWrong.AudioManager.instance != null && _clickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_clickSound, 1, 0, 1);
        }

        bool showPanel = SaveGame.instance != null && SaveGame.instance.GetSavedData() != null;
        if (!showPanel) {
            NewGame();
        }
        else
        {
            HandlePanel(true);
        }
    }

    public void CancelPanel()
    {
        HandlePanel(false, true);
    }

    public void HandlePanel(bool show, bool PlaySound = true)
    {
        if (GoneWrong.AudioManager.instance != null && PlaySound)
        {
            AudioClip clip = show ? _clickSound : _cancelSound;
            if (clip != null)
                GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 1);
        }

        if (_areYouSurePanel != null)
        {
            _areYouSurePanel.gameObject.SetActive(show);
        }
    }

    public void NewGame()
    {
        if (GoneWrong.AudioManager.instance != null && _clickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_clickSound, 1, 0, 1);
        }

        HandlePanel(false);

        SetLoading(true);

        if (SaveGame.instance != null) SaveGame.instance.DeleteSaveGame();
        if (_playerInventory != null) _playerInventory.InitializeInventory();
        ProgressManager.instance.LoadScene(1);
    }

    public void LoadScene(int index)
    {
        SetLoading(true);
        SceneManager.LoadScene(index);
    }

    public void QuitGame()
    {
        if (GoneWrong.AudioManager.instance != null && _clickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_clickSound, 1, 0, 1);
        }

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
        if (GoneWrong.AudioManager.instance != null && _clickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_clickSound, 1, 0, 1);
        }

        if (SaveGame.instance != null && SaveGame.instance.savedData != null)
        {
            SetLoading(true);
            ProgressManager.instance.LoadScene(SaveGame.instance.savedData.currentScene);
        }
    }

    public void PlayHoverSound()
    {
        if (_hoverSound != null && GoneWrong.AudioManager.instance != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_hoverSound, 1, 0, 1);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    [Header("Inspector Assigned Objects")]
    [SerializeField] private GameObject _quitGamePanel = null;

    [Header("Configuration")]
    [SerializeField] private float _escapeButtonDelay = 3f;

    [Header("Sounds")]
    [SerializeField] private AudioClip _showQuitPanelSound = null;
    [SerializeField] private AudioClip _clickSound = null;
    [SerializeField] private AudioClip _hoverSound = null;
    [SerializeField] private AudioClip _cancelSound = null;

    private float _escapeButtonTimer = 0f;
    private bool _escapeButtonDown = false;

    private void Start()
    {
        if (_quitGamePanel != null)
        {
            _quitGamePanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _escapeButtonDown = true;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _escapeButtonDown = false;
            _escapeButtonTimer = 0f;
        }

        if (_escapeButtonDown && !_quitGamePanel.gameObject.activeSelf)
        {
            _escapeButtonTimer += Time.deltaTime;
            if (_escapeButtonTimer >= _escapeButtonDelay)
            {
                if (GoneWrong.AudioManager.instance != null && _showQuitPanelSound != null)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(_showQuitPanelSound, 1, 0, 1);
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                _quitGamePanel.gameObject.SetActive(true);
                _escapeButtonTimer = 0f;

                // Also make sure to deactivate the inventory
                if (PlayerInventoryUI.instance != null)
                    PlayerInventoryUI.instance.gameObject.SetActive(false);

                // We also stop the player from moving
                if (GoneWrong.Player.instance != null)
                {
                    GoneWrong.Player.instance.canMove = false;
                }

                // We also stop the camera from moving
                if (CameraMovement.instance != null)
                {
                    CameraMovement.instance.canControl = false;
                }
            }
        }
    }

    public void Quit()
    {
        // We play the quit sound
        if (GoneWrong.AudioManager.instance != null && _clickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_clickSound, 1, 0, 1);
        }

        // We deactivate the quit panel
        _quitGamePanel.gameObject.SetActive(false);

        if (ProgressManager.instance != null)
        {
            ProgressManager.instance.LoadScene(0);
        }
    }

    public void Cancel()
    {
        if (GoneWrong.AudioManager.instance != null && _cancelSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_cancelSound, 1, 0, 1);
        }

        if (_quitGamePanel != null)
        {
            _quitGamePanel.gameObject.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // The player can move again
        if (GoneWrong.Player.instance != null)
        {
            GoneWrong.Player.instance.canMove = true;
        }

        // We go back to controlling the camera
        if (CameraMovement.instance != null)
        {
            CameraMovement.instance.canControl = true;
        }
    }

    public void PlayHoverSound()
    {
        if (GoneWrong.AudioManager.instance != null && _hoverSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_hoverSound, 1, 0, 1);
        }
    }
}

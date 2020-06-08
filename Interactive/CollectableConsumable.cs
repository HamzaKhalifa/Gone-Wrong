using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectableConsumable : InteractiveObject
{
    [SerializeField] InventoryConsumableMount _consumableMount = null;
    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] AudioClip _pickSound = null;
    [SerializeField] Vector3 _instantiateRotation = Vector3.zero;
    [SerializeField] List<GameProgress> _progressStates = new List<GameProgress>();
    [SerializeField] private bool _destroyOnPick = true;
    [SerializeField] private bool _canSave = true;

    private IEnumerator _changeTextCouroutine = null;

    public Vector3 instantiateRotation { get { return _instantiateRotation; } }
    public InventoryConsumableMount consumableMount { get { return _consumableMount; } }
    public List<GameProgress> progressStates { get { return _progressStates; } }
    public bool canSave { get { return _canSave; } }

    protected override void Start()
    {
        base.Start();
    }

    public override bool Interact(Transform interactor)
    {
        // If this is an object that doesn't get destroyed after interacting with it, then we don't do anything the...
        // second time we interact with it, so to avoid overpicking the same collectable item.
        if (!_destroyOnPick && _didInteract) return false;

        if (PlayerInventoryUI.instance != null)
        {
            if (_consumableMount.item != null)
            {
                // We first try to find whether there is an empty slot in the backpack
                for (int i = 0; i < PlayerInventoryUI.instance.itemInfos.Count; i++)
                {
                    ItemInfo itemInfo = PlayerInventoryUI.instance.itemInfos[i];
                    if (itemInfo.isEmpty)
                    {
                        _playerInventory.consumables.Add(_consumableMount);
                        // We now repaint the playerinventoryUI
                        PlayerInventoryUI.instance.Repaint(false);

                        // Now we play the pickup sound
                        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
                        {
                            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
                        }

                        // Now we alter the game progress
                        if (_progressStates.Count > 0 && ProgressManager.instance != null)
                        {
                            foreach(GameProgress gameProgress in _progressStates)
                            {
                                ProgressManager.instance.SetProgress(gameProgress.key, gameProgress.value);
                            }
                        }

                        // And destroy this gameobject
                        if (_destroyOnPick)
                            Destroy(gameObject);

                        // We are calling base.interact at the end of the function so to avoid some objects destroying themselves before altering progress manager (Like the container in the industrial zone)
                        base.Interact(interactor);

                        return true;
                    }
                }

                // If we are here, it means we haven't found any empty spot in our backpack
                // So we show a "full backpack" message
                _text = "Backpack is Full";
                _changeTextCouroutine = ChangeText();
                StartCoroutine(_changeTextCouroutine);

                return false;
            }
        }

        return false;
    }

    private IEnumerator ChangeText()
    {
        yield return new WaitForSeconds(3f);

        _text = _interactiveText;
        _changeTextCouroutine = null;
    }

    public void PlayDropSound()
    {
        // When we drop, we also alter the game states
        if (_progressStates.Count > 0 && ProgressManager.instance != null)
        {
            foreach (GameProgress gameProgress in _progressStates)
            {
                // We set the progress state to nothing
                ProgressManager.instance.SetProgress(gameProgress.key, "OFF");
            }
        }

        // We play the drop sound
        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
        }
    }
}

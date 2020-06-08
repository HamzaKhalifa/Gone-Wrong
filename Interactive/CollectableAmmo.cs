using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableAmmo : InteractiveObject
{
    [SerializeField] InventoryAmmoMount _ammoMount = null;
    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] AudioClip _pickSound = null;
    [SerializeField] private bool _canSave = true;

    private IEnumerator _changeTextCouroutine = null;

    public InventoryAmmoMount ammoMount { get { return _ammoMount; } }
    public bool canSave { get { return _canSave; } }

    protected override void Start()
    {
        base.Start();
    }

    public override bool Interact(Transform interactor)
    {
        base.Interact(interactor);

        if (PlayerInventoryUI.instance != null)
        {
            if (_ammoMount.item != null)
            {
                // We first try to find whether there is an empty slot in the backpack
                for (int i = 0; i < PlayerInventoryUI.instance.itemInfos.Count; i++)
                {
                    ItemInfo itemInfo = PlayerInventoryUI.instance.itemInfos[i];
                    if (itemInfo.isEmpty)
                    {
                        _playerInventory.ammo.Add(_ammoMount);
                        // Then we simply apply the new ammo to the newly added ammomount, or the already existing and found ammo mount
                        _playerInventory.ammo[_playerInventory.ammo.Count - 1] = _ammoMount;
                        // We now repaint the playerinventoryUI
                        PlayerInventoryUI.instance.Repaint(false);

                        // Now we play the pickup sound
                        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
                        {
                            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
                        }

                        // And destroy this gameobject
                        Destroy(gameObject);
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
        if (GoneWrong.AudioManager.instance != null && _pickSound != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(_pickSound, 1, 0, 0);
        }
    }
}

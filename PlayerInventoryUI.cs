using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfo
{
    public Transform transform;
    public bool isEmpty = true;
    public Text nameText = null;
    public Text roundsText = null;
    public string description = null;
    public Image image = null;
    public InventoryWeaponMount weaponMount = null;
}

public class ItemInfo
{
    public Transform transform = null;
    public bool isEmpty = true;
    public ItemType itemType = ItemType.None;
    public Image image = null;
    public InventoryAmmoMount ammoMount;
    public InventoryConsumableMount consumableMount;
}

[System.Serializable]
public struct GeneralDescriptionPanel
{
    public Image descriptionImage;
    public Text descriptionNameText;
    public Text decriptionText;
    public Button actionButton1;
    public Button actionButton2;

    public GeneralDescriptionPanel(Image image, Text desName, Text des, Button b1, Button b2)
    {
        descriptionImage = image;
        descriptionNameText = desName;
        decriptionText = des;
        actionButton1 = b1;
        actionButton2 = b2;
    }
}

public enum ItemType
{
    None,
    Weapon,
    Consumable,
    Ammo
}

public class PlayerInventoryUI : MonoBehaviour
{
    public static PlayerInventoryUI instance = null;

    [SerializeField] Inventory _inventory = null;
    [SerializeField] List<GameObject> _itemSlots = new List<GameObject>();
    [SerializeField] List<GameObject> _weaponSlots = new List<GameObject>();

    [SerializeField] Color _weaponContainerHoverColor = Color.red;
    [SerializeField] Color _weaponContainerClickedColor = Color.green;
    [SerializeField] Color _itemContainerHoverColor = Color.red;
    [SerializeField] Color _itemContainerClickedColor = Color.green;
    [SerializeField] GeneralDescriptionPanel _generalDescriptionPanel = new GeneralDescriptionPanel();

    [Header("Stats dependencies")]
    [SerializeField] SharedFloat _healthSharedFloat = null;
    [SerializeField] SharedFloat _staminaSharedFloat = null;
    [SerializeField] SharedFloat _infectionSharedFloat = null;
    [SerializeField] Slider _healthSlider = null;
    [SerializeField] Slider _staminaSlider = null;
    [SerializeField] Slider _infectionSlider = null;

    private List<WeaponInfo> _weaponInfos = new List<WeaponInfo>();
    private List<ItemInfo> _itemInfos = new List<ItemInfo>();
    private Color _weaponContainerDefaultColor = Color.white;
    private Color _itemContainerDefaulColor = Color.white;
    private int _clickedItemIndex = -1;
    private ItemType _clickedItemType = ItemType.None;
    private ItemType _hoverdItemType = ItemType.None;

    public List<ItemInfo> itemInfos { get { return _itemInfos; } }

    private void Start()
    {
        instance = this;

        Repaint(true);
        gameObject.SetActive(false);

        if (_weaponSlots.Count > 0)
        {
            _weaponContainerDefaultColor = _weaponSlots[0].GetComponent<Image>().color;
        }

        if (_itemSlots.Count > 0)
        {
            _itemContainerDefaulColor = _itemSlots[0].GetComponent<Image>().color;
        }
    }

    private void OnEnable()
    {
        Repaint(false);
    }

    private void Update()
    {
        if (_healthSharedFloat != null && _healthSlider != null)
        {
            _healthSlider.value = _healthSharedFloat.value / 100;
        }

        if (_staminaSharedFloat != null && _staminaSlider != null)
        {
            _staminaSlider.value = _staminaSharedFloat.value / 100;
        }

        if (_infectionSharedFloat != null && _infectionSlider != null)
        {
            _infectionSlider.value = (int) _infectionSharedFloat.value / 100;
        }
    }

    public void Repaint(bool firstPaint)
    {
        _itemInfos.Clear();
        _weaponInfos.Clear();

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            ItemInfo itemInfo = new ItemInfo();
            itemInfo.transform = _itemSlots[i].transform;
            itemInfo.image = _itemSlots[i].transform.Find("Frame").Find("Slot Image").GetComponent<Image>();
            itemInfo.image.gameObject.SetActive(false);
            itemInfo.isEmpty = true;

            _itemInfos.Add(itemInfo);
        }

        for(int i = 0; i < _weaponSlots.Count; i++)
        {
            Transform weaponInfoContainer = _weaponSlots[i].transform.Find("Weapon Info Container");

            WeaponInfo weaponInfo = new WeaponInfo();
            weaponInfo.transform = weaponInfoContainer.transform;
            weaponInfo.isEmpty = true;
            weaponInfo.nameText = weaponInfoContainer.Find("Weapon Name").GetComponent<Text>();
            weaponInfo.roundsText = weaponInfoContainer.Find("Weapon Rounds").GetComponent<Text>();
            weaponInfo.image = weaponInfoContainer.Find("Weapon Image").Find("Frame").Find("Slot Image").GetComponent<Image>();

            _weaponInfos.Add(weaponInfo);

            weaponInfoContainer.gameObject.SetActive(false);
        }

        if (_inventory != null)
        {
            // Setting inventory weapons
            List<InventoryWeaponMount> weaponMounts = new List<InventoryWeaponMount>();
            weaponMounts.Add(_inventory.rifle1);
            weaponMounts.Add(_inventory.rifle2);
            weaponMounts.Add(_inventory.handgun);
            weaponMounts.Add(_inventory.melee);

            for (int i = 0; i < weaponMounts.Count; i++) 
            {
                if (weaponMounts[i].item != null)
                {
                    _weaponInfos[i].isEmpty = false;
                    _weaponInfos[i].nameText.text = weaponMounts[i].item.itemName;
                    _weaponInfos[i].roundsText.text = weaponMounts[i].rounds + "/" + weaponMounts[i].item.ammoCapacity;
                    _weaponInfos[i].description = weaponMounts[i].item.itemDescription;
                    _weaponInfos[i].image.sprite = weaponMounts[i].item.image;
                    _weaponInfos[i].weaponMount = weaponMounts[i];

                    _weaponInfos[i].transform.gameObject.SetActive(true);

                    // After adding the weapon to the inventory, we are gonna add the weaponControl
                    // We only do this at the beginning
                    if (firstPaint)
                        _weaponInfos[i].weaponMount.item.collectableWeapon.ReplaceWeapon(true, _weaponInfos[i].weaponMount);
                } else
                {
                    _weaponInfos[i].image.sprite = null;
                    _weaponInfos[i].nameText.text = "";
                    _weaponInfos[i].roundsText.text = "";
                }
            }

            // Setting inventory ammo
            for (int i = 0; i < _inventory.ammo.Count; i++)
            {
                if (_inventory.ammo[i].item != null)
                {
                    InventoryAmmoMount ammoMount = _inventory.ammo[i];
                    _itemInfos[i].isEmpty = false;
                    _itemInfos[i].image.gameObject.SetActive(true);
                    _itemInfos[i].image.sprite = _inventory.ammo[i].item.image;
                    _itemInfos[i].itemType = ItemType.Ammo;
                    _itemInfos[i].ammoMount = ammoMount;
                }
            }

            // Setting inventory consumables
            for (int i = 0; i < _inventory.consumables.Count; i++)
            {
                int consumableIndex = _inventory.ammo.Count + i;
                if (_inventory.consumables[i].item != null)
                {
                    InventoryConsumableMount consumableMount = _inventory.consumables[i];
                    _itemInfos[consumableIndex].isEmpty = false;
                    _itemInfos[consumableIndex].image.gameObject.SetActive(true);
                    _itemInfos[consumableIndex].image.sprite = _inventory.consumables[i].item.image;
                    _itemInfos[consumableIndex].itemType = ItemType.Consumable;
                    _itemInfos[consumableIndex].consumableMount = consumableMount;
                }
            }
        }

        if (_clickedItemIndex == -1)
        {
            ResetGeneralDescription();
        } else
        {
            ChangeGeneralDescription(_clickedItemIndex);
        }
    }

    public void OnWeaponPointerEnter(int index)
    {
        if (_weaponSlots.Count > index && _clickedItemIndex == -1 && _weaponInfos[index].weaponMount != null)
        {
            if (_weaponSlots[index] != null)
            {
                Image image = _weaponSlots[index].GetComponent<Image>();
                image.color = _weaponContainerHoverColor;
                image.fillCenter = true;
                _hoverdItemType = ItemType.Weapon;

                ChangeGeneralDescription(index);
            }
        }
    }

    public void OnWeaponPointerExit(int index)
    {
        if (_clickedItemIndex == index && _hoverdItemType == ItemType.Weapon) return;

        if (_weaponSlots.Count > index)
        {
            if (_weaponSlots[index] != null)
            {
                Image image = _weaponSlots[index].GetComponent<Image>();
                image.color = _weaponContainerDefaultColor;
                image.fillCenter = false;
            }
        }
        if (_clickedItemIndex == -1)
            ResetGeneralDescription();
    }

    public void OnWeaponPointerClick(int index)
    {
        if (_weaponInfos[index].weaponMount == null) return;

        if (_weaponSlots.Count > index)
        {
            if (_weaponSlots[index] != null)
            {
                Image image = _weaponSlots[index].GetComponent<Image>();
                // If we already got the weapon selected, we unselect it
                if (_clickedItemIndex == index && _hoverdItemType == ItemType.Weapon)
                {
                    image.color = _weaponContainerHoverColor;
                    image.fillCenter = true;

                    _clickedItemIndex = -1;
                } else
                {
                    image.color = _weaponContainerClickedColor;
                    image.fillCenter = true;
                    _clickedItemIndex = index;
                    _hoverdItemType = ItemType.Weapon;

                    ChangeGeneralDescription(index);

                    // Deselect Other weapons
                    for (int i = 0; i < _weaponSlots.Count; i++)
                    {
                        if (index != i)
                        {
                            OnWeaponPointerExit(i);
                        }
                    }

                    // Deselect all items
                    for (int i = 0; i < _itemInfos.Count; i++)
                    {
                        OnItemPointerExit(i);
                    }
                }
            }
        }
    }

    public void OnItemPointerEnter(int index)
    {
        if (_itemInfos[index].ammoMount == null && _itemInfos[index].consumableMount == null) return;

        if (_itemInfos[index] != null && _clickedItemIndex == -1)
        {
            _itemInfos[index].transform.GetComponent<Image>().color = _itemContainerHoverColor;
            _hoverdItemType = _itemInfos[index].itemType;

            ChangeGeneralDescription(index);
        }
    }

    public void OnItemPointerExit(int index)
    {
        if (_clickedItemIndex == index && _hoverdItemType != ItemType.Weapon) return;

        if (_itemInfos.Count > index)
        {
            if (_itemInfos[index] != null)
            {
                _itemInfos[index].transform.GetComponent<Image>().color = _itemContainerDefaulColor;
            }
        }
        if (_clickedItemIndex == -1)
            ResetGeneralDescription();
    }

    public void OnItemPointerClick(int index)
    {
        if (_itemInfos[index].ammoMount == null && _itemInfos[index].consumableMount == null) return;

        if (_itemInfos.Count > index)
        {
            if (_itemInfos[index] != null)
            {
                // If we already got the weapon selected, we unselect it
                if (_clickedItemIndex == index && _hoverdItemType != ItemType.Weapon)
                {
                    _itemInfos[index].transform.GetComponent<Image>().color = _itemContainerDefaulColor;

                    _clickedItemIndex = -1;
                }
                else
                {
                    _itemInfos[index].transform.GetComponent<Image>().color = _itemContainerClickedColor;
                    
                    _clickedItemIndex = index;
                    _hoverdItemType = _itemInfos[index].itemType;

                    ChangeGeneralDescription(index);

                    // Deselect All weapons
                    for (int i = 0; i < _weaponSlots.Count; i++)
                    {
                        OnWeaponPointerExit(i);
                    }

                    // Deselect Other Items
                    for (int i = 0; i < _itemInfos.Count; i++)
                    {
                        if (index != i)
                        {
                            OnItemPointerExit(i);
                        }
                    }
                }
            }
        }
    }

    public void ChangeGeneralDescription(int index)
    {
        if (_weaponInfos.Count > index && _hoverdItemType == ItemType.Weapon)
        {
            if (_weaponInfos[index].weaponMount != null && _weaponInfos[index].weaponMount.item != null)
            {
                _clickedItemType = ItemType.Weapon;

                InventoryWeapon weapon = _weaponInfos[index].weaponMount.item;
                if (_generalDescriptionPanel.descriptionImage != null)
                {
                    _generalDescriptionPanel.descriptionImage.gameObject.SetActive(true);
                    _generalDescriptionPanel.descriptionImage.sprite = weapon.image;
                }

                if (_generalDescriptionPanel.descriptionNameText != null)
                {
                    _generalDescriptionPanel.descriptionNameText.text = weapon.itemName;
                }

                if (_generalDescriptionPanel.decriptionText != null)
                {
                    _generalDescriptionPanel.decriptionText.text = weapon.itemDescription;
                }

                if (_generalDescriptionPanel.actionButton1 != null)
                {
                    _generalDescriptionPanel.actionButton1.gameObject.SetActive(true);
                    _generalDescriptionPanel.actionButton1.GetComponentInChildren<Text>().text = weapon.actionButton1Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.GetComponentInChildren<Text>().text = weapon.actionButton2Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.gameObject.SetActive(weapon.showActionButton2);
                }
            }
        }


        if (_itemInfos.Count > index && _hoverdItemType == ItemType.Ammo)
        {
            if (_itemInfos[index].ammoMount.item != null)
            {
                _clickedItemType = ItemType.Ammo;

                InventoryAmmo ammo = _itemInfos[index].ammoMount.item;

                if(_generalDescriptionPanel.descriptionImage != null)
                {
                    _generalDescriptionPanel.descriptionImage.gameObject.SetActive(true);
                    _generalDescriptionPanel.descriptionImage.sprite = ammo.image;
                }

                if (_generalDescriptionPanel.descriptionNameText != null)
                {
                    _generalDescriptionPanel.descriptionNameText.text = ammo.itemName;
                }

                if (_generalDescriptionPanel.decriptionText != null)
                {
                    _generalDescriptionPanel.decriptionText.text = _itemInfos[index].ammoMount.rounds + "/" + _itemInfos[index].ammoMount.item.capacity;
                }

                if (_generalDescriptionPanel.actionButton1 != null)
                {
                    _generalDescriptionPanel.actionButton1.gameObject.SetActive(true);
                    _generalDescriptionPanel.actionButton1.GetComponentInChildren<Text>().text = ammo.actionButton1Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.GetComponentInChildren<Text>().text = ammo.actionButton2Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.gameObject.SetActive(ammo.showActionButton2);
                }
            }
        }

        if (_itemInfos.Count > index && _hoverdItemType == ItemType.Consumable)
        {
            if (_itemInfos[index].consumableMount.item != null)
            {
                _clickedItemType = ItemType.Consumable;

                InventoryConsumable consumable = _itemInfos[index].consumableMount.item;

                if (_generalDescriptionPanel.descriptionImage != null)
                {
                    _generalDescriptionPanel.descriptionImage.gameObject.SetActive(true);
                    _generalDescriptionPanel.descriptionImage.sprite = consumable.image;
                }

                if (_generalDescriptionPanel.descriptionNameText != null)
                {
                    _generalDescriptionPanel.descriptionNameText.text = consumable.itemName;
                }

                if (_generalDescriptionPanel.decriptionText != null)
                {
                    _generalDescriptionPanel.decriptionText.text = _itemInfos[index].consumableMount.item.itemDescription;
                }

                if (_generalDescriptionPanel.actionButton1 != null)
                {
                    _generalDescriptionPanel.actionButton1.gameObject.SetActive(true);
                    _generalDescriptionPanel.actionButton1.GetComponentInChildren<Text>().text = consumable.actionButton1Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.GetComponentInChildren<Text>().text = consumable.actionButton2Text;
                }

                if (_generalDescriptionPanel.actionButton2 != null)
                {
                    _generalDescriptionPanel.actionButton2.gameObject.SetActive(consumable.showActionButton2);
                }
            }
        }
    }

    public void ResetGeneralDescription()
    {
        if (_generalDescriptionPanel.decriptionText != null)
        {
            _generalDescriptionPanel.decriptionText.text = "";
        }

        if (_generalDescriptionPanel.descriptionNameText != null)
        {
            _generalDescriptionPanel.descriptionNameText.text = "";
        }

        if (_generalDescriptionPanel.descriptionImage != null)
        {
            _generalDescriptionPanel.descriptionImage.sprite = null;
            _generalDescriptionPanel.descriptionImage.gameObject.SetActive(false);
        }

        if (_generalDescriptionPanel.actionButton1 != null)
        {
            _generalDescriptionPanel.actionButton1.gameObject.SetActive(false);
        }

        if (_generalDescriptionPanel.actionButton2 != null)
        {
            _generalDescriptionPanel.actionButton2.gameObject.SetActive(false);
        }

        _clickedItemType = ItemType.None;
    }

    public void Drop()
    {
        switch(_clickedItemType)
        {
            case ItemType.Weapon:
                // We instantiate the weapon in front of us:
                InventoryWeaponMount clickedWeaponMount = null;
                if (_clickedItemIndex == 0) clickedWeaponMount = _inventory.rifle1;
                else if (_clickedItemIndex == 1) clickedWeaponMount = _inventory.rifle2;
                else if (_clickedItemIndex == 2) clickedWeaponMount = _inventory.handgun;
                else if (_clickedItemIndex == 3) clickedWeaponMount = _inventory.melee;

                if (clickedWeaponMount.item.collectableWeapon != null)
                    Instantiate(clickedWeaponMount.item.collectableWeapon,
                        GoneWrong.Player.instance.transform.position + GoneWrong.Player.instance.transform.forward,
                        Quaternion.Euler(clickedWeaponMount.item.collectableWeapon.instantiateRotation));

                // We play the drop weapon sound
                clickedWeaponMount.item.collectableWeapon.PlayDropSound();

                // We find the weapon control and we destroy it
                GoneWrong.WeaponControl weaponControl = clickedWeaponMount.item.collectableWeapon.weaponControl;
                foreach (Transform child in GoneWrong.Player.instance.weaponHolder.transform)
                {
                    if (child.GetComponent<GoneWrong.WeaponControl>().inventoryWeapon == clickedWeaponMount.item)
                    {
                        Destroy(child.gameObject);
                    }
                }

                // If the equipped weapon is the weapon that we just dropped, we switch back to no equipped weapon
                if (GoneWrong.Player.instance.equippedWeapon == _clickedItemIndex + 1)
                {
                    GoneWrong.Player.instance.SwitchWeapon(0);
                }

                clickedWeaponMount.rounds = 0;
                clickedWeaponMount.item = null;
                break;
            case ItemType.Ammo:
                // We instantiate the ammo in front of us
                if (_inventory.ammo[_clickedItemIndex].item.collectableAmmo != null)
                    Instantiate(_inventory.ammo[_clickedItemIndex].item.collectableAmmo,
                        GoneWrong.Player.instance.transform.position + GoneWrong.Player.instance.transform.forward,
                        Quaternion.identity);

                // We play the drop ammo sound
                _inventory.ammo[_clickedItemIndex].item.collectableAmmo.PlayDropSound();

                _inventory.ammo[_clickedItemIndex].rounds = 0;
                _inventory.ammo[_clickedItemIndex].item = null;
                _inventory.ammo.RemoveAt(_clickedItemIndex);
                break;
            case ItemType.Consumable:
                // We get the index of the intem in the inventory consumable list
                // Because consumables are stored after ammo.
                int indexAtConsumableList = _clickedItemIndex - _inventory.ammo.Count;

                // We instantiate the consumable in front of us

                if (_inventory.consumables[indexAtConsumableList].item.collectableConsumable != null)
                {
                    CollectableConsumable consumable = _inventory.consumables[indexAtConsumableList].item.collectableConsumable;

                    if (!consumable.consumableMount.item.canBeDropped) {
                        // If the item can't be dropped (because it's a key item), we get out after notifyng the player
                        if (Notifications.instance != null)
                        {
                            Notifications.instance.EnqueNotification("This item can't be dropped");
                        }
                        return;
                    }

                    Instantiate(_inventory.consumables[indexAtConsumableList].item.collectableConsumable,
                        GoneWrong.Player.instance.transform.position + GoneWrong.Player.instance.transform.forward,
                        Quaternion.Euler(_inventory.consumables[indexAtConsumableList].item.collectableConsumable.instantiateRotation));

                    // We play the drop consumable sound
                    _inventory.consumables[indexAtConsumableList].item.collectableConsumable.PlayDropSound();

                    _inventory.consumables[indexAtConsumableList].item = null;
                    _inventory.consumables.RemoveAt(indexAtConsumableList);
                }
                break;
        }

        _clickedItemIndex = -1;
        _clickedItemType = ItemType.None;

        Repaint(false);
    }

    public void Consume()
    {
        switch(_clickedItemType) {
            case ItemType.Consumable:
                int indexAtConsumableList = _clickedItemIndex - _inventory.ammo.Count;

                // We instantiate the consumable in front of us
                CollectableConsumable collectableConsumable = _inventory.consumables[indexAtConsumableList].item.collectableConsumable;
                if (collectableConsumable != null)
                {
                    InventoryConsumable consumable = collectableConsumable.consumableMount.item;
                    if (consumable != null)
                    {
                        // We play the drop consumable sound
                        if (consumable.consumeSound != null && GoneWrong.AudioManager.instance != null)
                        {
                            GoneWrong.AudioManager.instance.PlayOneShotSound(consumable.consumeSound, 1, 0, 0);
                        }

                        if (_healthSharedFloat != null)
                        {
                            float newHealth = _healthSharedFloat.value += consumable.healthAterlation;

                            if (newHealth < 0)
                                newHealth = 0f;
                            if (newHealth > 100)
                                newHealth = 100;

                            _healthSharedFloat.value = newHealth;
                        }

                        if (_staminaSharedFloat != null)
                        {
                            float newStamina = _staminaSharedFloat.value += consumable.staminaAlteration;

                            if (newStamina < 0)
                                newStamina = 0f;
                            if (newStamina > 100)
                                newStamina = 100;

                            _staminaSharedFloat.value = newStamina;
                        }

                        if (_infectionSharedFloat != null)
                        {
                            float newInfection = _infectionSharedFloat.value += consumable.infectionAlteration;

                            if (newInfection < 0)
                                newInfection = 0f;
                            if (newInfection > 100)
                                newInfection = 100;

                            _infectionSharedFloat.value = newInfection;
                        }


                        _inventory.consumables[indexAtConsumableList].item = null;
                        _inventory.consumables.RemoveAt(indexAtConsumableList);
                    }
                }
                    break;
        }

        _clickedItemIndex = -1;
        _clickedItemType = ItemType.None;

        Repaint(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] SharedInt _equippedWeapon = null;
    [SerializeField] Text _equippedRoundsText = null;
    [SerializeField] Text _remainingRoundsText = null;
    [SerializeField] Text _interactiveText = null;
    [SerializeField] SharedFloat _healthSharedFloat = null;
    [SerializeField] Slider _healthSlider = null;
    [SerializeField] Text _healthText = null;
    [SerializeField] SharedFloat _staminaSharedFloat = null;
    [SerializeField] Slider _staminaSlider = null;
    [SerializeField] Text _staminaText = null;
    [SerializeField] SharedFloat _infectionSharedFloat = null;
    [SerializeField] Slider _infectionSlider = null;
    [SerializeField] Text _infectionText = null;
    [SerializeField] Text _levelObjectiveText = null;

    public static PlayerHUD instance = null;

    public string nextObjective { get { return _levelObjectiveText.text; } }

    private void Awake()
    {
        instance = this;

        _interactiveText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Get the weapon mount of the current equipped weapon
        InventoryWeaponMount weaponMount = null;
        if (_equippedWeapon.value == 0) weaponMount = _playerInventory.rifle1;
        else if (_equippedWeapon.value == 1) weaponMount = _playerInventory.rifle2;
        else if (_equippedWeapon.value == 2) weaponMount = _playerInventory.handgun;
        if (_equippedWeapon.value == 3) weaponMount = _playerInventory.melee;

        // For Screen ammo
        if (_playerInventory != null && _equippedWeapon != null
            && _equippedWeapon.value >= 0 && _equippedWeapon.value < 3
            )
        {
            if (weaponMount.item != null) {
                // Activating ammo text
                _remainingRoundsText.gameObject.SetActive(true);
                _equippedRoundsText.gameObject.SetActive(true);

                _equippedRoundsText.text = weaponMount.rounds + "";

                // Walk through all the remaining ammo in our backpack an add them to the remaining amm text
                int remainingRounds = 0;
                for (int i = 0; i < _playerInventory.ammo.Count; i++)
                {
                    if (_playerInventory.ammo[i] != null
                        && _playerInventory.ammo[i].item != null
                        && _playerInventory.ammo[i].item.weapon == weaponMount.item)
                    {
                        remainingRounds += _playerInventory.ammo[i].rounds;
                    }
                }

                _remainingRoundsText.text = remainingRounds + "";
            }
        } else
        {
            // Deactivating ammo text
            _remainingRoundsText.gameObject.SetActive(false);
            _equippedRoundsText.gameObject.SetActive(false);
        }

        // For screan health
        if (_healthSlider != null && _healthSharedFloat != null)
        {
            _healthSlider.value = _healthSharedFloat.value / 100f;
            if (_healthText != null)
                _healthText.text = "Health: " + _healthSharedFloat.value + "%"; 
        }

        // For screan stamina
        if (_staminaSlider != null && _staminaSharedFloat != null)
        {
            _staminaSlider.value = _staminaSharedFloat.value / 100f;
            int staminaValue = (int)_staminaSharedFloat.value;
            if (_staminaText != null)
                _staminaText.text = "Stamina: " + staminaValue.ToString() + "%";
        }

        // For screan infection
        if (_infectionSlider != null && _infectionSharedFloat != null)
        {
            _infectionSlider.value = _infectionSharedFloat.value / 100f;
            if (_infectionText != null)
                _infectionText.text = "Infection: " + _infectionSharedFloat.value.ToString() + "%";
        }
    }

    public void SetInteractiveText(string text)
    {
        _interactiveText.text = text;
        _interactiveText.gameObject.SetActive(true);
    }

    public void DeactivateInteractiveText()
    {
        _interactiveText.gameObject.SetActive(false);
    }

    public void ChangeLevelObjectiveText(string text) {
        if (_levelObjectiveText != null)
        {
            _levelObjectiveText.text = text;
        }
    }
}

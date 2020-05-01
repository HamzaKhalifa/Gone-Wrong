using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarHUD : MonoBehaviour
{
    public static CarHUD instance = null;

    [SerializeField] private SharedFloat _healthSharedFloat = null;
    [SerializeField] private Slider _healthSlider = null;
    [SerializeField] private Text _levelObjectiveText = null;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (_healthSlider != null && _healthSharedFloat != null) {
            _healthSlider.value = _healthSharedFloat.value / 100;
        }
    }

    public void ChangeLevelObjectiveText(string text)
    {
        if (_levelObjectiveText != null)
        {
            _levelObjectiveText.text = text;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScopeUI : MonoBehaviour
{
    public static ScopeUI instance = null;

    [SerializeField] Image _scopeImage = null;

    private void Start()
    {
        instance = this;
    }
    public void SetScopeImage(Sprite scopeSprite, bool active)
    {
        if (_scopeImage != null)
        {
            _scopeImage.sprite = scopeSprite;
            _scopeImage.gameObject.SetActive(active);
        }
    }
}

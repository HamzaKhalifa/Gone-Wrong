using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField][Range(0, 250)] int _priority = 250;
    [SerializeField] protected string _interactiveText = "Object";
    [SerializeField] protected bool _endLevel = false;
    [SerializeField] protected int _nextScene = 0;
    [SerializeField] protected UnityEvent _interactEvents = null;

    protected bool _didInteract = false;

    public int priority { get { return _priority; } }

    protected string _text = "";

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _text = _interactiveText;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void ShowText()
    {
        if (PlayerHUD.instance != null)
        {
            PlayerHUD.instance.SetInteractiveText(_text);
        }
    }

    public virtual bool Interact(Transform interactor) {

        // If this is the first time we interact with the object, we trigger its events
        if (!_didInteract)
        {
            _didInteract = true;
            _interactEvents.Invoke();
        }

        return true;
    }
}

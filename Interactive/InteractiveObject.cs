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

    [Header("For a switch")]
    [SerializeField] private bool _switchedOn = false;
    [SerializeField] private UnityEvent _switchOnEvents = null;
    [SerializeField] private UnityEvent _switchOffEvents = null;

    [Header("For switching light materials")]
    [SerializeField] private Light _light = null;
    [SerializeField] private MeshRenderer _lightMesh = null;
    [SerializeField] private List<int> _materialIndexesInMesh = new List<int>();
    [SerializeField] private Material _switchOnMaterial = null;
    [SerializeField] private Material _switchOffMaterial = null;


    protected bool _didInteract = false;

    public int priority { get { return _priority; } }

    protected string _text = "";

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _text = _interactiveText;

        if (_switchedOn && _switchOnEvents != null)
        {
            _switchOnEvents.Invoke();
        }

        if (!_switchedOn && _switchOffEvents != null)
        {
            _switchOffEvents.Invoke();
        }

        ChangeMaterial();
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

        _switchedOn = !_switchedOn;
        if (_switchedOn && _switchOnEvents != null)
        {
            _switchOnEvents.Invoke();
        }

        if (!_switchedOn && _switchOffEvents != null)
        {
            _switchOffEvents.Invoke();
        }

        ChangeMaterial();

        return true;
    }

    public void ChangeMaterial()
    {
        if (_lightMesh == null || _switchOnMaterial == null || _switchOffMaterial == null || _materialIndexesInMesh.Count == 0) return;

        if (_light != null)
        {
            _light.gameObject.SetActive(_switchedOn);

        }
        Material[] matArray = _lightMesh.materials;
        foreach (int i in _materialIndexesInMesh)
        {
            if (matArray.Length > i)
                matArray[i] = _switchedOn ? _switchOnMaterial : _switchOffMaterial;
        }

        _lightMesh.materials = matArray;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> _barrierComponents = new List<ParticleSystem>();
    [SerializeField] private float _recoverRate = 50f;
    [SerializeField] private float _scaleThreshold = 0.1f;

    private float _initialScale = 1;
    private bool _takingDamage = false;
    private float _damageTimer = 0f;
    private float _damageDelay = 4f;

    // Cache variables
    private Collider _collider = null;

    private void Start()
    {
        if(_barrierComponents.Count > 0 && _barrierComponents[0] != null) {
            _initialScale = _barrierComponents[0].transform.localScale.x;
        }

        _damageTimer = _damageDelay;

        _collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_damageTimer < _damageDelay)
        {
            _damageTimer += Time.deltaTime;
        } else
        {
            _takingDamage = false;

            // We reanable the collider if we are done taking damage and are now in the process of returning the fire again
            if (!_collider.enabled)
            {
                _collider.enabled = true;
            }
        }

        if (_barrierComponents.Count > 0
            && _barrierComponents[0] != null
            && _barrierComponents[0].transform.localScale.x != _initialScale
            && !_takingDamage)
        {
            foreach (ParticleSystem barrierComponent in _barrierComponents)
            {
                barrierComponent.transform.localScale = new Vector3(barrierComponent.transform.localScale.x + (_recoverRate * Time.deltaTime),
                    barrierComponent.transform.localScale.y + (_recoverRate * Time.deltaTime),
                    barrierComponent.transform.localScale.z + (_recoverRate * Time.deltaTime));

                if (barrierComponent.transform.localScale.x >= _initialScale || _initialScale - barrierComponent.transform.localScale.x < _scaleThreshold)
                {
                    barrierComponent.transform.localScale = new Vector3(_initialScale, _initialScale, _initialScale);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_barrierComponents.Count > 0
            && _barrierComponents[0] != null)
        {
            _takingDamage = true;
            _damageTimer = 0f;

            foreach (ParticleSystem barrierComponent in _barrierComponents)
            {
                barrierComponent.transform.localScale = new Vector3(barrierComponent.transform.localScale.x - damage,
                    barrierComponent.transform.localScale.y - damage,
                    barrierComponent.transform.localScale.z - damage);

                if (barrierComponent.transform.localScale.x <= 0)
                {
                    // We disable the collider one the gaz/fire goes away
                    _collider.enabled = false;
                    barrierComponent.transform.localScale = Vector3.zero;
                }
            }
        }
    }
}

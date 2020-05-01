using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] bool _enemyAttack = true;
    [SerializeField] float _damage = 20f;
    [SerializeField] float _vehicleDamage = 3f;
    [SerializeField] List<AudioClip> _attackSounds = new List<AudioClip>();

    private bool _inContact = false;
    private string _tagName = "Player";
    private string _vehicleTagName = "Car";

    // Start is called before the first frame update
    void Start()
    {
        if (!_enemyAttack)
        {
            _tagName = "Enemy";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tagName) && !_inContact)
        {
            _inContact = true;

            if (_enemyAttack && GoneWrong.Player.instance != null)
            {
                if (_attackSounds.Count > 0 && GoneWrong.AudioManager.instance != null)
                {
                    AudioClip choseAttackSound = _attackSounds[Random.Range(0, _attackSounds.Count)];
                    if (choseAttackSound != null)
                        GoneWrong.AudioManager.instance.PlayOneShotSound(choseAttackSound, 1, 0, 1);
                }

                GoneWrong.Player.instance.TakeDamage(_damage);
            }
        }

        if (other.CompareTag(_vehicleTagName) && !_inContact)
        {
            _inContact = true;
            Vehicle vehicle = other.GetComponentInParent<Vehicle>();

            List<AudioClip> damageSounds = vehicle.damageSounds;

            if (GoneWrong.AudioManager.instance != null && damageSounds.Count > 0)
            {
                AudioClip sound = damageSounds[Random.Range(0, damageSounds.Count)];
                if (sound != null)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(sound, 1, 0, 1, other.transform.position);
                }

                if (vehicle != null)
                {
                    vehicle.TakeDamage(_vehicleDamage);
                }

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag(_tagName) || other.CompareTag(_vehicleTagName)) && _inContact)
        {
            _inContact = false;
        }
    }

    private void OnDisable()
    {
        _inContact = false;
    }
}

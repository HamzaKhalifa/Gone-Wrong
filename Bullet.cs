using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoneWrong
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float _speed = 1f;
        [SerializeField] float _rayCastDistance = .1f;

        // Private
        int _defaultLayer = -1;

        // Cache
        Rigidbody _rigidBody = null;

        private void Awake()
        {
            _defaultLayer = LayerMask.GetMask("Default");

            _rigidBody = GetComponent<Rigidbody>();
            
        }

        void Update()
        {
            Debug.DrawRay(transform.position, transform.forward, Color.green);
            if (Physics.Raycast(transform.position, transform.forward, _rayCastDistance, _defaultLayer)) {
                Destroy(gameObject);
            }

            Vector3 velocity = transform.forward * _speed;
            _rigidBody.velocity = velocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            
        }
    }
}

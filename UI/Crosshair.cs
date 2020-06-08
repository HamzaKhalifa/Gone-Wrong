using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GoneWrong
{
    public class Crosshair : MonoBehaviour
    {
        private Animator _animator = null;
        private GoneWrong.Player _player = null;

        private int _walkingHash = Animator.StringToHash("Walking");
        private int _runningHash = Animator.StringToHash("Running");

        private void Start()
        {
            _player = GoneWrong.Player.instance;

            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_player == null) return;

            if (_player.isMoving) _animator.SetBool(_walkingHash, true);
            else _animator.SetBool(_walkingHash, false);

            if (_player.isRunning) _animator.SetBool(_runningHash, true);
            else _animator.SetBool(_runningHash, false);
        }
    }
}

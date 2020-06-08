using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshAnimatorController : MonoBehaviour
{
    [SerializeField] Animator _animator = null;
    [SerializeField] List<GameObject> _skinnedMeshRenderers = new List<GameObject>();

    // Cache Fields
    private FSG.MeshAnimator.MeshAnimator _meshAnimator = null;
    private MeshRenderer _meshRenderer = null;

    private AnimationClip[] _animationClips = null;
    private string _currentAnimation = "";

    private void Start()
    {
        _meshAnimator = GetComponent<FSG.MeshAnimator.MeshAnimator>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (_animator != null && _meshAnimator != null)
        {
            string animationName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (_currentAnimation != animationName)
            {
                _currentAnimation = animationName;
                _meshAnimator.Play(animationName);
            }

            _meshAnimator.SetTime((_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1) * _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        }

        bool activateMeshAnimator = (!_animator.IsInTransition(0));
        _meshRenderer.enabled = activateMeshAnimator;
        foreach (GameObject skinnedMeshRendere in _skinnedMeshRenderers)
        {
            skinnedMeshRendere.SetActive(!activateMeshAnimator);
        }
    }
}

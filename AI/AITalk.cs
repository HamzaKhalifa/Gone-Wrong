using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITalk : MonoBehaviour
{
    [SerializeField] private GameObject _jaw = null;
    [SerializeField] private Vector3 _mouthClosed = Vector3.zero;
    [SerializeField] private Vector3 _mouthOpen = Vector3.zero;
    [SerializeField] List<float> _talkSpeedRange = new List<float>();
    [SerializeField] List<float> _talkDelayRange = new List<float>();

    private bool _talking = false;
    private bool _opening = false;
    private IEnumerator _talkCoroutine = null;
    private bool _closedMouth = false;
    private Quaternion _nextRotation = Quaternion.identity;

    #region Public Accessors

    public bool talking { set { _talking = value; } }

    #endregion

    private void Start()
    {
        _nextRotation = transform.rotation;
    }

    private void Update()
    {
        if (_talking)
        {
            _closedMouth = false;
            if (_talkCoroutine == null)
            {
                _talkCoroutine = TalkCoroutine();
                StartCoroutine(_talkCoroutine);
            }
        } else if (!_closedMouth)
        {
            StartCoroutine(CloseMouth());
        }
    }

    private void LateUpdate()
    {
        _jaw.transform.localRotation = _nextRotation;
    }

    private IEnumerator TalkCoroutine()
    {
        _opening = !_opening;

        yield return new WaitForSeconds(_talkDelayRange[Random.Range(0, _talkDelayRange.Count)]);

        float time = 0f;

        Quaternion initialRotation = Quaternion.Euler(_opening ? _mouthClosed : _mouthOpen);
        Quaternion finalRotation = Quaternion.Euler(_opening ? _mouthOpen : _mouthClosed);

        float talkSpeed = _talkSpeedRange[Random.Range(0, _talkSpeedRange.Count)];

        while (time <= talkSpeed)
        {
            float normalizedTime = time / talkSpeed;
            _nextRotation = Quaternion.Lerp(initialRotation, finalRotation, normalizedTime);

            time += Time.deltaTime;

            yield return null;
        }

        _nextRotation = finalRotation;

        _talkCoroutine = null;
    }

    private IEnumerator CloseMouth()
    {
        float time = 0f;

        Quaternion initialRotation = transform.rotation;
        Quaternion finalRotation = Quaternion.Euler(_mouthClosed);

        float talkSpeed = _talkSpeedRange[Random.Range(0, _talkSpeedRange.Count)];

        while (time <= talkSpeed)
        {
            float normalizedTime = time / talkSpeed;
            _nextRotation = Quaternion.Lerp(initialRotation, finalRotation, normalizedTime);

            time += Time.deltaTime;

            yield return null;
        }

        _nextRotation = finalRotation;

        _closedMouth = true;
    }
}

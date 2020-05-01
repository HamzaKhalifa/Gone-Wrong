using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioThreat
{
    public Transform transform = null;
    public SphereCollider collider = null;
    public float radius = 0;
    public float depletionDelay = 0;
    public Vector3 position = Vector3.zero;
    public IEnumerator depletionCoroutine = null;
    public bool active = false;
}

[RequireComponent(typeof(SphereCollider))]
public class AudioThreatManager : MonoBehaviour
{
    public static AudioThreatManager instance = null;

    [SerializeField] private float _capacity = 20;

    private List<AudioThreat> _audioThreats = new List<AudioThreat>();

    private void Awake()
    {
        instance = this;

        transform.position = Vector3.zero;

        for (int i = 0; i < _capacity; i++)
        {
            GameObject audioThreatGameObject = new GameObject();
            audioThreatGameObject.tag = "Audio";
            audioThreatGameObject.layer = LayerMask.NameToLayer("Audio");
            audioThreatGameObject.transform.parent = transform;
            AudioThreat audioThreat = new AudioThreat();
            audioThreat.transform = audioThreatGameObject.transform;
            audioThreat.collider = audioThreatGameObject.AddComponent<SphereCollider>();
            audioThreat.collider.radius = 0f;
            audioThreat.collider.enabled = false;
            audioThreat.collider.isTrigger = true;
            Rigidbody rb = audioThreatGameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            audioThreatGameObject.SetActive(false);

            _audioThreats.Add(audioThreat);
        }
    }

    public void MakeNoise(float radius, float depletionDelay, Vector3 position)
    {
        foreach(AudioThreat audioThreat in _audioThreats)
        {
            if(!audioThreat.active)
            {
                audioThreat.transform.gameObject.SetActive(true);
                audioThreat.transform.position = position;
                audioThreat.collider.radius = radius;
                audioThreat.collider.enabled = true;
                audioThreat.radius = radius;
                audioThreat.active = true;
                audioThreat.depletionDelay = depletionDelay;

                audioThreat.depletionCoroutine = DepletionCoroutine(audioThreat);
                StartCoroutine(audioThreat.depletionCoroutine);

                break;
            }
        }
    }

    public IEnumerator DepletionCoroutine(AudioThreat audioThreat)
    {
        float time = 0f;

        while (time <= audioThreat.depletionDelay)
        {
            float normalizedTime = time / audioThreat.depletionDelay;
            audioThreat.collider.radius = Mathf.Lerp(audioThreat.radius, 0, normalizedTime);

            time += Time.deltaTime;

            yield return null;
        }

        audioThreat.collider.radius = 0f;

        // After the sound is gone
        audioThreat.depletionCoroutine = null;
        audioThreat.active = false;
        audioThreat.collider.enabled = false;
        audioThreat.transform.gameObject.SetActive(false);
    }
}

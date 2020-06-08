using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager instance = null;

    [SerializeField] private ParticleSystem _bloodParticles = null;
    [SerializeField] private List<GameObject> _bloodSplatters = new List<GameObject>();
    [SerializeField] private bool _wallSpaller = false;
    [SerializeField] private float _numberOfSplatters = 10;

    [Header("Fire")]
    [SerializeField] private ParticleSystem _fire = null;

    [Header("Explosion")]
    [SerializeField] private ParticleSystem _explosion = null;

    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    private void Awake()
    {
        instance = this;
    }

    public void Explosion(Transform position)
    {
        if (_explosion == null) return;

        _explosion.gameObject.SetActive(true);
        _explosion.transform.position = position.position;
        _explosion.Emit(10);
        IEnumerator coroutine = Deactivate(5, _explosion.gameObject);
        StartCoroutine(coroutine);
    }

    public void Fire(Transform position)
    {
        if (_fire ==null) return;

        _fire.gameObject.SetActive(true);
        _fire.transform.position = position.position;
        IEnumerator coroutine = Deactivate(20, _fire.gameObject);
        StartCoroutine(coroutine);
    } 

    IEnumerator Deactivate(float time, GameObject effect)
    {
        yield return new WaitForSeconds(time);

        if (effect != null)
        {
            effect.SetActive(false);
        }
    }

    public void SplatterBlood(Transform origin)
    {
        IEnumerator coroutine = SplatterBloodCoroutine(origin);
        StartCoroutine(coroutine);
    }

    private IEnumerator SplatterBloodCoroutine(Transform origin)
    {
        yield return new WaitForSeconds(.5f);

        if (_bloodSplatters.Count > 0)
        {
            for (int i = 0; i < _numberOfSplatters; i++)
            {
                GameObject splatter = _bloodSplatters[Random.Range(0, _bloodSplatters.Count)];
                if (splatter != null)
                {
                    Vector3 randomPosition = new Vector3(Random.Range(0f, float.MaxValue), origin.transform.position.y, Random.Range(0f, float.MaxValue));

                    Vector3 direction;

                    if (Random.Range(0, 4) == 0)
                        direction = (randomPosition - origin.position).normalized / Random.Range(1, 11);
                    else
                        direction = (randomPosition + origin.position).normalized / Random.Range(1, 11);

                    Vector3 splatterHorizontalPosition = origin.position + direction;
                    RaycastHit hit;
                    if (Physics.Raycast(splatterHorizontalPosition, -Vector3.up, out hit, float.MaxValue, LayerMask.GetMask("Default", "DecorationBase", "Wood", "Metal")))
                    {
                        GameObject tmp = Instantiate(splatter, hit.point, Quaternion.identity);
                        tmp.transform.up = hit.normal;
                    }
                }
            }

            if (_wallSpaller)
            {
                // Make a splatter on the wall
                Collider[] closetColliders = Physics.OverlapSphere(origin.position, 2f, LayerMask.GetMask("Default", "DecorationBase", "Metal", "Wood"));

                if (closetColliders.Length > 0)
                {
                    // Choose one random collider amongst the ones found
                    Collider chosenCollider = closetColliders[Random.Range(0, closetColliders.Length)];

                    Vector3 closestPoint = chosenCollider.ClosestPointOnBounds(origin.position);
                    Vector3 raycastDirection = (closestPoint - origin.position).normalized;

                    RaycastHit hit;
                    if (Physics.Raycast(origin.position, raycastDirection, out hit, float.MaxValue, LayerMask.GetMask("Default", "DecorationBase", "Wood", "Metal")))
                    {
                        GameObject splatter = _bloodSplatters[Random.Range(0, _bloodSplatters.Count)];
                        GameObject tmp = Instantiate(splatter, hit.point, Quaternion.identity);
                        tmp.transform.up = hit.normal;
                    }
                }
            }

            // Make a splatter on the wall
            /*Collider[] closetColliders = Physics.OverlapSphere(origin.position, 2f, LayerMask.GetMask("Default", "DecorationBase", "Metal", "Wood"));

            foreach(Collider collider in closetColliders)
            {
                // Choose one random collider amongst the ones found
                Collider chosenCollider = closetColliders[Random.Range(0, closetColliders.Length)];

                Vector3 closestPoint = collider.ClosestPointOnBounds(origin.position);
                Vector3 raycastDirection = (closestPoint - origin.position).normalized;

                RaycastHit hit;
                if (Physics.Raycast(origin.position, raycastDirection, out hit, float.MaxValue, LayerMask.GetMask("Default", "DecorationBase", "Wood", "Metal")))
                {
                    GameObject splatter = _bloodSplatters[Random.Range(0, _bloodSplatters.Count)];
                    GameObject tmp = Instantiate(splatter, hit.point, Quaternion.identity);
                    tmp.transform.up = hit.normal;
                }
            }*/

        }
    }
}

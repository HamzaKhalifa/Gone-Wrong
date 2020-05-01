using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDecoration : MonoBehaviour
{
    [SerializeField] private List<GameObject> _decorations = new List<GameObject>();
    [SerializeField] private List<GameObject> _cars = new List<GameObject>();
    [SerializeField] private List<GameObject> _zombies = new List<GameObject>();

    [SerializeField] private Transform _A = null;
    [SerializeField] private Transform _B = null;
    [SerializeField] private Transform _C = null;
    [SerializeField] private Transform _D = null;
    [SerializeField] private float _rayCastDistance = 100000f;

    [SerializeField] private float _minHorizontalIncrementation = .005f;
    [SerializeField] private float _maxHorizontalIncrementation = 0.01f;
    [SerializeField] private float _minVerticalIncrementation = .005f;
    [SerializeField] private float _maxVerticalIncrementation = 0.01f;

    [SerializeField] List<NavigationPath> _navigationPaths = new List<NavigationPath>();

    void Awake()
    {
        HandleDecorations(_decorations, false);
        HandleDecorations(_cars, false);
        HandleDecorations(_zombies, true);
    }

    private void HandleDecorations(List<GameObject> decorations, bool zombie)
    {
        if (decorations.Count == 0) return;

        LayerMask layerMask = LayerMask.GetMask("DecorationBase");

        float i = .01f;
        while (i <= 1)
        {
            Vector3 E = _A.position + ((_B.position - _A.position) * i);
            Vector3 F = _C.position + ((_D.position - _C.position) * i);

            float j = .01f;

            while (j <= 1)
            {
                Vector3 G = E + ((F - E) * j);

                // Now we cast a ray from G to the bottom
                RaycastHit hitInfo;
                if (Physics.Raycast(G, -transform.up, out hitInfo, _rayCastDistance, layerMask))
                {
                    GameObject decoration = decorations[Random.Range(0, decorations.Count)];
                    if (decoration != null)
                    {
                        GameObject tmp = Instantiate(decoration, hitInfo.point, Quaternion.identity);
                        if (zombie)
                        {
                            tmp.GetComponent<AIStatePatrol>().navigationPath = _navigationPaths[Random.Range(0, _navigationPaths.Count)];
                        }
                    }
                }

                j += Random.Range(_minVerticalIncrementation, _maxVerticalIncrementation); 
            }
            i += Random.Range(_minHorizontalIncrementation, _maxHorizontalIncrementation);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NavigationPath : MonoBehaviour
{
    [SerializeField] List<Transform> _navigationPoints = new List<Transform>();

    public List<Transform> navigationPoints
    {
        get { return _navigationPoints; }
    }

    private void OnDrawGizmos()
    {
        if (_navigationPoints.Count > 1)
        {
            for(int i = 0; i < _navigationPoints.Count; i++) {
                int nextNavigationPointIndex = i + 1;
                if (nextNavigationPointIndex == _navigationPoints.Count) nextNavigationPointIndex = 0;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(_navigationPoints[i].transform.position, _navigationPoints[nextNavigationPointIndex].transform.position);
            }
        }
    }
}

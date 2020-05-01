using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x,
            Random.Range(0, 360), transform.rotation.eulerAngles.z));
    }
}

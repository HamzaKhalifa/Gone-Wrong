using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILook : MonoBehaviour
{
    // Cache Fields
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}

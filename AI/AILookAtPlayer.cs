using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILookAtPlayer : MonoBehaviour
{
    private void Update()
    {
        /*Quaternion targetRotation = Quaternion.LookRotation(GoneWrong.Player.instance.transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);*/

        transform.LookAt(GoneWrong.Player.instance.transform.position);
    }
}

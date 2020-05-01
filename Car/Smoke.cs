using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour {
    
    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        float h = Input.GetAxis("Vertical");
        ParticleSystem MyPartSystem = GetComponent<ParticleSystem>();        
	}
}

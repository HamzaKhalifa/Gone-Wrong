using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager instance = null;

    [SerializeField] private ParticleSystem _bloodParticles = null;

    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    private void Awake()
    {
        instance = this;
    }
}

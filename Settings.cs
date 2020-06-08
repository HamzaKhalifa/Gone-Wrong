using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField] private int _targetFrameRate = 300;

    void Start()
    {
        QualitySettings.skinWeights = SkinWeights.OneBone;
        Application.targetFrameRate = _targetFrameRate;
    }
}

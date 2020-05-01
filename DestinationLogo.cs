using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestinationLogo : MonoBehaviour
{

    [SerializeField] Text _destinationDistanceText = null;
    [SerializeField] SharedFloat _destinationDistanceSharedFloat = null;
    [SerializeField] Image _destinationLogo = null;

    [SerializeField] SharedFloat _angleWithHorizontalShredFloat = null;
    [SerializeField] SharedFloat _angleWithCameraUpSharedFloat = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // For destination distance
        if (_destinationDistanceSharedFloat != null && _destinationDistanceText != null)
        {
            _destinationDistanceText.text = Mathf.Floor(_destinationDistanceSharedFloat.value) + "";
        }

        if (_destinationLogo != null)
        {
            float angleWithHorizontal = _angleWithHorizontalShredFloat.value;
            float angleWithVertical = _angleWithCameraUpSharedFloat.value;

            float screenWidth = 600;
            float screenHeight = 400;

            float logoXPosition = Mathf.Clamp(screenWidth * angleWithHorizontal / 90, -screenWidth, screenWidth);
            float angleWithVerticalValue = 0;
            if (angleWithVertical > 90 && angleWithVertical < 180)
            {
                if (angleWithVertical > 110)
                {
                    angleWithVerticalValue = -400;
                }
                else
                {
                    angleWithVerticalValue = -(screenHeight / 90) * (angleWithVertical - 90);
                }
            }
            else if (angleWithVertical <= 90 && angleWithVertical > 0)
            {
                if (angleWithVertical < 70)
                {
                    angleWithVerticalValue = 400;
                }
                else
                {
                    angleWithVerticalValue = (screenHeight / 90) * (90 - angleWithVertical);
                }
            }
            float logoYPosition = Mathf.Clamp(angleWithVerticalValue, -screenHeight, screenHeight);

            _destinationLogo.rectTransform.anchoredPosition3D = new Vector3(logoXPosition, logoYPosition,
                _destinationLogo.transform.position.z);
        }
    }
}

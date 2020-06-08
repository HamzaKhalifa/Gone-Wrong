using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveCar : InteractiveObject
{
    [SerializeField] private string _getOutText = "'E' to get out";
    [SerializeField] private Transform _playerSit = null;
    [SerializeField] private Transform _playerOut = null;
    [SerializeField] private InteractiveDoor _carDoor = null;
    [SerializeField] private float _getInCarDuration = 2f;
    [SerializeField] private float _getOutOfCarDuration = 2f;

    private bool _playerIn = false;
    private IEnumerator _sitCoroutine = null;
    private Vehicle _carController = null;
    private Vector3 _playerLocalScale = new Vector3(1, 1, 1);

    protected override void Start()
    {
        base.Start();
        _carController = GetComponentInParent<Vehicle>();

        if (GoneWrong.Player.instance != null)
        {
            _playerLocalScale = GoneWrong.Player.instance.transform.localScale;
        }
    }

    public override bool Interact(Transform interactor)
    {
        base.Interact(interactor);

        if (_sitCoroutine == null)
        {
            _sitCoroutine = Sit();
            StartCoroutine(_sitCoroutine);
        }

        return true;
    }
    public IEnumerator Sit()
    {
        GoneWrong.Player player = GoneWrong.Player.instance;

        // Reset blood screen
        if (player != null) player.ResetBloodScreen();

        if (player == null || _playerSit == null || _carController == null || _playerOut == null) {
            _sitCoroutine = null;
            yield break;
        }

        _playerIn = !_playerIn;

        player.canMove = !_playerIn;

        player.transform.parent = _playerIn ? _playerSit : _playerOut;

        Vector3 playerInitialPosition = player.transform.localPosition;
        Quaternion playerInitialRotation = player.transform.localRotation;

        float duration = _playerIn ? _getInCarDuration : _getOutOfCarDuration;

        if (!_playerIn)
        {
            // We open the door before getting outside the car
            if (_carDoor != null)
                _carDoor.Interact(transform);

            // We deactivate the car controller if we are getting out
            _carController.enabled = false;
        } else
        {
            // If we are getting inside the car, we deactivate the player controls
            player.GetComponent<CharacterController>().enabled = !_playerIn;
            // We also tell the player that we are inside a car
            player.insideACar = true;
        }

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;

            player.transform.localPosition = Vector3.Lerp(playerInitialPosition, Vector3.zero, normalizedTime);
            player.transform.localRotation = Quaternion.Slerp(playerInitialRotation, Quaternion.Euler(Vector3.zero), normalizedTime);

            yield return null;
        }

        // Make sure we got the right values
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // Make sure the main camera gets set to rotation zero after we get out
        if (!_playerIn)
        {
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        // We close the door after getting inside the car
        if (_carDoor != null && _playerIn)
        {
            _carDoor.Interact(transform);
        }

        if (_playerIn)
        {
            // We activate the char controller after it is enabled
            _carController.enabled = true;
        } else {
            player.transform.parent = null;
            player.GetComponent<CharacterController>().enabled = true;
            player.insideACar = false;
            // Then we close the door after we are out.
            if (_carDoor != null) _carDoor.Interact(transform);
        }

        // Now we change the text
        _text = _playerIn ? _getOutText : _interactiveText;

        // Force the player to go back to his initial local scale after we get him out of the car
        if (!_playerIn)
        {
            player.transform.localScale = _playerLocalScale;
        }

        // Now we activate or deactive playerHUD or carHUD
        if (CarHUD.instance != null)
            CarHUD.instance.gameObject.SetActive(_playerIn);
        if (PlayerHUD.instance != null)
            PlayerHUD.instance.gameObject.SetActive(!_playerIn);

        // Set the player drived vehicle
        player.drivedVehicle = _playerIn ? GetComponentInParent<Vehicle>() : null;

        // Activate the flashlight the moment the player gets out of the car
        if (!_playerIn && Flashlight.instance != null) {
            Flashlight.instance.Look(!Flashlight.instance.looking);
        }

        // Play the vehicle's startup sound:
        if (_playerIn && player.drivedVehicle.startUpSound != null && GoneWrong.AudioManager.instance != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(player.drivedVehicle.startUpSound, 1, 0, 0, transform.position);
        }

        _sitCoroutine = null;
    }
}

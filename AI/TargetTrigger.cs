using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    private AIStateMachine _stateMachine = null;
    private LayerMask _viewableMask = -1;
    private SphereCollider _collider = null;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _viewableMask = LayerMask.GetMask("Default", "Player", "Interactive", "DecorationBase", "Car", "Wood", "Metal");
    }

    public void RegisterTargetTrigger(AIStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_stateMachine == null || !_stateMachine.enabled) return;

        if (other.CompareTag("Player") || other.CompareTag("Car") || other.CompareTag("Audio"))
        {
            bool threatInSight = false;

            if ((other.CompareTag("Player") || other.CompareTag("Car")) && _stateMachine != null)
            {
                //Debug.DrawRay(transform.position, other.bounds.center - transform.position, Color.green);

                //RaycastHit[] hits = Physics.SphereCast(transform.position, 10f, other.bounds.center - transform.position, _collider.radius, _viewableMask);
                RaycastHit[] hits = Physics.RaycastAll(transform.position, other.bounds.center - transform.position, _collider.radius, _viewableMask);

                if (hits.Length > 0)
                {
                    RaycastHit closestHit = hits[0];
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.distance < closestHit.distance)
                        {
                            closestHit = hit;
                        }
                    }

                    if (_stateMachine.seeThroughWalls && GoneWrong.Player.instance.drivedVehicle == null && other.CompareTag("Player")
                        || (_stateMachine.seeThroughWalls && GoneWrong.Player.instance.drivedVehicle != null && closestHit.transform.gameObject == GoneWrong.Player.instance.drivedVehicle.gameObject && other.CompareTag("Car"))
                        || closestHit.transform.gameObject == GoneWrong.Player.instance.gameObject && other.CompareTag("Player")
                        || (GoneWrong.Player.instance.drivedVehicle != null && closestHit.transform.gameObject == GoneWrong.Player.instance.drivedVehicle.gameObject && other.CompareTag("Car")))
                    {
                        Transform targetTransform = null;
                        targetTransform = GoneWrong.Player.instance.transform;

                        // If there is no obstacle between the zombie and the player, we check if the player is in the field of view
                        if (Vector3.Angle(_stateMachine.transform.forward, targetTransform.position - transform.position) <= _stateMachine.fieldOfView)
                        {
                            if (_stateMachine.currentTarget == null || _stateMachine.currentTarget.type != TargetType.Player)
                            {
                                // We create the target of type player
                                Target target = new Target();
                                target.position = new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z);
                                if (other.CompareTag("Player"))
                                {
                                    target.transform = other.transform;
                                }
                                else if (other.CompareTag("Car"))
                                {
                                    target.transform = other.GetComponentInParent<Vehicle>().transform;
                                }
                                target.type = TargetType.Player;

                                // Then we set the new target to the state machine
                                _stateMachine.currentTarget = target;
                            }

                            // If we already have the player as the target, we need to update his position each frame we have him exposed
                            if (_stateMachine.currentTarget != null && _stateMachine.currentTarget.type == TargetType.Player)
                            {
                                _stateMachine.currentTarget.position = new Vector3(other.transform.position.x, other.transform.position.y, other.transform.position.z);
                            }

                            threatInSight = true;
                        }
                    }
                }
            }

            if (!threatInSight && (!_stateMachine.seeThroughWalls && _stateMachine.fieldOfView != 360))
            {
                if (other.CompareTag("Audio") && _stateMachine.sensitiveToSound &&
                    (_stateMachine.currentTarget == null || (_stateMachine.currentTarget != null && _stateMachine.currentTarget.type != TargetType.Player)
                    ))
                {
                    Target target = new Target();
                    target.transform = other.transform;
                    target.position = other.transform.position;
                    target.type = TargetType.Audio;
                    _stateMachine.currentTarget = target;
                } else
                {
                    // When we have the player in sight and the target trigger is colliding with the car, and the player is not inside the car, then we don't reset the target
                    if (!(_stateMachine.currentTarget != null
                        && _stateMachine.currentTarget.type == TargetType.Player
                        && other.CompareTag("Car")
                        && GoneWrong.Player.instance.drivedVehicle == null))
                        ResetTarget();
                }
            }
        } else
        {
            // If we aren't colliding with anything that isn't of layer car
            // Some objects are of layer car but don't have the tag car
            // So these objects will still trigger the collision and reset the target when we don't want that to happen
            if (LayerMask.LayerToName(other.transform.gameObject.layer) != "Car")
            {
                ResetTarget();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Car") || other.CompareTag("Audio"))
        {
            ResetTarget();
        }
    }

    private void ResetTarget()
    {
        // if we don't have a remaining path
        if (_stateMachine != null)
        {
            if (!_stateMachine.navMeshAgent.hasPath || _stateMachine.navMeshAgent.isPathStale)
            {
                _stateMachine.currentTarget = null;
            }
        }
    }
}

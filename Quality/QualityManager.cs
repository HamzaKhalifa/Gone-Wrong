using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    public static QualityManager instance = null;

    [Header("Enemies")]
    [SerializeField] private bool _optimize = false;
    [SerializeField] private float _optimizationDistance = 10f;
    [SerializeField] private Transform _enemiesParent = null;

    [Header("Mesh renderers")]
    [SerializeField] private bool _optimizeMeshRenderers = false;
    [SerializeField] private float _optimizationDistanceForMeshRenderers = 30f;


    private List<AIStateMachine> _stateMachines = new List<AIStateMachine>();
    private List<MeshRenderer> _meshRenderes = new List<MeshRenderer>();
    private Transform _player = null;

    public Transform player { set { _player = value; } }

    private void Start()
    {
        instance = this;

        if (_optimize)
        {
            // Loop through all the enemies and add them one by one to our list if they are activated
            AIStateMachine[] stateMachines;
            if (_enemiesParent != null)
            {
                stateMachines = _enemiesParent.GetComponentsInChildren<AIStateMachine>(true);
            } else
                stateMachines = FindObjectsOfType<AIStateMachine>();

            for (int i = 0; i < stateMachines.Length; i++)
            {
                _stateMachines.Add(stateMachines[i]);

                // Then we deactivate the enemy
                stateMachines[i].ChangeState(AIStateType.Idle);
                stateMachines[i].gameObject.SetActive(false);
            }
        }

        if (_optimizeMeshRenderers)
        {
            MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();
            for(int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i].gameObject.activeSelf)
                {
                    _meshRenderes.Add(meshRenderers[i]);
                }
            }
        }

        if (GoneWrong.Player.instance != null)
            _player = GoneWrong.Player.instance.transform;
    }

    private void Update()
    {
        if (_optimize && _player != null)
        {
            foreach(AIStateMachine stateMachine in _stateMachines)
            {
                if (stateMachine == null) continue;

                // We calculate the distance between the player and the enemy
                float distance = (stateMachine.transform.position - _player.position).magnitude;
                // Then we activate or deactivate the enemy depending on his distance to the player
                if (distance > _optimizationDistance && stateMachine.gameObject.activeSelf)
                {
                    // We change the zombie to idle before deactivating him only if he isn't dead
                    if (!stateMachine.dead)
                        stateMachine.ChangeState(AIStateType.Idle);
                    stateMachine.gameObject.SetActive(false);
                }
                else if (distance <= _optimizationDistance && !stateMachine.gameObject.activeSelf && !stateMachine.dead)
                    stateMachine.gameObject.SetActive(true);
            }
        }

        if (_optimizeMeshRenderers && _player != null && _meshRenderes.Count > 0)
        {
            foreach (MeshRenderer meshRenderer in _meshRenderes)
            {
                // If the object hasn't been destroyed by our savegame script or anything else
                if (meshRenderer != null)
                {
                    // We calculate the distance between the player and the enemy
                    float distance = (meshRenderer.transform.position - _player.transform.position).magnitude;
                    // Then we activate or deactivate the enemy depending on his distance to the player
                    if (distance > _optimizationDistanceForMeshRenderers && meshRenderer.gameObject.activeSelf)
                    {
                        meshRenderer.gameObject.SetActive(false);
                    }
                    else if (distance <= _optimizationDistanceForMeshRenderers && !meshRenderer.gameObject.activeSelf)
                        meshRenderer.gameObject.SetActive(true);
                }
            }
        }
    }
}

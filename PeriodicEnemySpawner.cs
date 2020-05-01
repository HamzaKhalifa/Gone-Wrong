using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeriodicEnemySpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> _positions = new List<Transform>();
    [SerializeField] private List<AIStateMachine> _enemies = new List<AIStateMachine>();
    [SerializeField] private int _maxEnemies = 7;
    [SerializeField] private float _spawnDelay = 10f;

    private float _spawnedEnemies = 0f;
    private float _spawnTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _spawnTimer = _spawnDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (_positions.Count > 0 && _enemies.Count > 0)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnDelay)
            {
                _spawnTimer = 0f;
                AIStateMachine stateMachine = _enemies[Random.Range(0, _enemies.Count)];
                Transform position = _positions[Random.Range(0, _positions.Count)];
                if (stateMachine != null && position != null && _spawnedEnemies < _maxEnemies)
                {
                    AIStateMachine tmp = (AIStateMachine)Instantiate(stateMachine, transform.position, Quaternion.identity);
                    NavMeshAgent agent = tmp.GetComponent<NavMeshAgent>();
                    agent.enabled = false;
                    tmp.transform.position = position.position;
                    tmp.transform.rotation = position.rotation;
                    agent.enabled = true;
                    tmp.spawner = this;
                    _spawnedEnemies++;
                }
                
            }
        }
    }

    public void UnregisterEnemy() {
        _spawnedEnemies--;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemySpot
{
    public Transform transform;
    public bool occupied = false;
    public int indexInAvailableEnemies = -1;
}

[System.Serializable]
public class Enemy
{
    public bool isOccupyingASpot = false;
    public AIStateMachine stateMachine = null;
}

public class EnemyTeleporter : MonoBehaviour
{
    public static EnemyTeleporter instance = null;

    [SerializeField] List<Transform> _spots = new List<Transform>();
    [SerializeField] List<AIStateMachine> _enemies = new List<AIStateMachine>();
    [SerializeField] List<Transform> _items = new List<Transform>();

    [SerializeField] private float _occupationDistance = 40f;
    [SerializeField] private float _checkDelay = 2f;

    private List<EnemySpot> _enemySpots = new List<EnemySpot>();
    private List<Enemy> _availableEnemies = new List<Enemy>();
    private float _checkTimer = 0f;

    private void Awake()
    {
        instance = this;

        foreach(Transform spot in _spots)
        {
            EnemySpot enemySpot = new EnemySpot();
            enemySpot.transform = spot;
            enemySpot.occupied = false;
            enemySpot.indexInAvailableEnemies = -1;

            _enemySpots.Add(enemySpot);
        }

        foreach(AIStateMachine stateMachine in _enemies)
        {
            Enemy enemy = new Enemy();
            stateMachine.gameObject.SetActive(false);
            enemy.stateMachine = stateMachine;
            enemy.isOccupyingASpot = false;

            _availableEnemies.Add(enemy);
        }
    }

    private void Update()
    {
        if (_enemySpots.Count <= 0 || GoneWrong.Player.instance == null) return;

        bool canCheck = false;

        _checkTimer += Time.deltaTime;
        if (_checkTimer >= _checkDelay)
        {
            canCheck = true;
            _checkTimer = 0f;
        }

        // If we can't check yet, we return;
        if (!canCheck) return;

        foreach(EnemySpot enemySpot in _enemySpots)
        {
            if ((enemySpot.transform.position - GoneWrong.Player.instance.transform.position).magnitude < _occupationDistance)
            {
                if (!enemySpot.occupied)
                {
                    // Search through the enemies and find an enemy who isn't occupying any spot
                    for(int i = Random.Range(0, _availableEnemies.Count); i < _availableEnemies.Count; i++)
                    {
                        if (!_availableEnemies[i].isOccupyingASpot)
                        {
                            if (_availableEnemies[i].stateMachine.navMeshAgent != null)
                                _availableEnemies[i].stateMachine.navMeshAgent.enabled = true;

                            _availableEnemies[i].stateMachine.gameObject.SetActive(false);
                            _availableEnemies[i].isOccupyingASpot = true;
                            enemySpot.indexInAvailableEnemies = i;
                            enemySpot.occupied = true;

                            // Now we revive the zombie, and we put his position to that of the spot
                            _availableEnemies[i].stateMachine.Revive();
                            _availableEnemies[i].stateMachine.transform.position = enemySpot.transform.position;
                            _availableEnemies[i].stateMachine.gameObject.SetActive(true);

                            break;
                        }
                    }
                }
            } else
            {
                // We unocuppy both the spot and the enemy

                if (enemySpot.indexInAvailableEnemies != -1)
                {
                    Enemy theEnemy = _availableEnemies[enemySpot.indexInAvailableEnemies];
                    // We only uoccupy a spot if the enemy is far from the player
                    if ((theEnemy.stateMachine.transform.position - GoneWrong.Player.instance.transform.position).magnitude >= _occupationDistance)
                    {
                        enemySpot.occupied = false;

                        // We unoccupy the enemy from the availableEnemies list by its index in enemyspot
                        theEnemy.isOccupyingASpot = false;
                        theEnemy.stateMachine.gameObject.SetActive(false);
                        enemySpot.indexInAvailableEnemies = -1;
                    }
                }

            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_spots.Count > 1)
        {
            for (int i = 0; i < _spots.Count; i++)
            {
                int nextSpotIndex = i + 1;
                if (nextSpotIndex == _spots.Count) nextSpotIndex = 0;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(_spots[i].transform.position, _spots[nextSpotIndex].transform.position);
            }
        }
    }

    public void SpawnItem(Vector3 position)
    {
        Transform item = _items[Random.Range(0, _items.Count)];
        if (item != null)
        {
            Instantiate(item, position, Quaternion.identity);
        }
    }

    public static void Shuffle(List<AIStateMachine> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

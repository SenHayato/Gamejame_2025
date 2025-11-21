using Code.Scripts.Enemy;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Enemies")]
    [SerializeField] private GameObject _enemyPrefab;
    [Header("Spawn Position")]
    [SerializeField] private Transform _spawnPositions;
    #endregion

    #region Properties

    #endregion

    #region Singleton
    public static EnemySpawnManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Methods

    public void Restart() {
        _spawnPositions = GameObject.Find("EnemySpawnPositions").transform;
    }

    public void SpawnEnemies(int enemyNumber)
    {
        for (var i = 0; i < enemyNumber; i++)
        {
            var randomIndex = Random.Range(0, _spawnPositions.childCount);
            var spawnPoint = _spawnPositions.GetChild(randomIndex);
            var enemy = Instantiate(_enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.GetComponent<EnemyAgent>().SetSpawn(spawnPoint);
        }
    }
    #endregion
}

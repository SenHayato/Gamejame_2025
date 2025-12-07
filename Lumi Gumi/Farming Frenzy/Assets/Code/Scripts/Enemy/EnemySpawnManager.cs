using Code.Scripts.Enemy;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Enemies")]
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Spawn Position")]
    [SerializeField] private Transform _spawnPositions;

    [Header("Spawn Settings")]
    [Tooltip("How many seconds between each spawn wave?")]
    [SerializeField] private float _spawnInterval = 5.0f;

    [Tooltip("How many enemies appear at once?")]
    [SerializeField] private int _enemiesPerSpawn = 1;
    #endregion

    #region Private Variables
    private float _timer;
    private bool _isSpawningActive = true; // Use this to stop spawning if Game Over
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

    private void Start()
    {
        // Reset timer on start so it doesn't spawn immediately (optional)
        _timer = 0f;
    }

    private void Update()
    {
        if (!_isSpawningActive) return;
        if (_spawnPositions == null) return; // Prevents crash if scene changes

        // 1. Count up the time
        _timer += Time.deltaTime;

        // 2. If time is up, Spawn!
        if (_timer >= _spawnInterval)
        {
            SpawnEnemies(_enemiesPerSpawn);
            _timer = 0f; // Reset timer
        }
    }

    public void Restart()
    {
        _spawnPositions = GameObject.Find("EnemySpawnPositions").transform;
        _timer = 0f; // Reset timer when game restarts
        _isSpawningActive = true;
    }

    // Call this if you want to stop spawning (e.g., Game Over)
    public void StopSpawning()
    {
        _isSpawningActive = false;
    }

    public void SpawnEnemies(int enemyNumber)
    {
        // Safety check: Don't spawn if we lost the reference
        if (_spawnPositions == null) return;

        for (var i = 0; i < enemyNumber; i++)
        {
            var randomIndex = Random.Range(0, _spawnPositions.childCount);
            var spawnPoint = _spawnPositions.GetChild(randomIndex);
            var enemy = Instantiate(_enemyPrefab, spawnPoint.position, Quaternion.identity);

            // Safety check: ensure the prefab actually has the script
            if (enemy.TryGetComponent(out EnemyAgent agent))
            {
                agent.SetSpawn(spawnPoint);
            }
        }
    }
    #endregion
}
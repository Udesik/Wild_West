using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private int _poolSize;
    [SerializeField] private int _poolCapacity;

    [SerializeField] private float _spawnInterval;
    [SerializeField] private float _spawnRadious;

    private int _currentWaveSize = 0;
    private int _currentWave = -1;
    private List<int> _spawnedEnemies;
    private int _currentBossHP = 150;
    private float _scaleHP = 1.2f;

    [SerializeField] private Enemy _enemyPrefab;

    private ObjectPool<Enemy> _pool;
    private bool _spawnBoss = false;
    private bool _spawnWave = true;

    public event Action Win;

    private void Awake()
    {
        _pool = new ObjectPool<Enemy>(
            createFunc: () => Instantiate(_enemyPrefab),
            actionOnGet: (enemy) => OnGetFromPool(enemy),
            actionOnRelease: (enemy) => OnReleaseToPool(enemy),
            actionOnDestroy: (enemy) => Destroy(enemy.gameObject),
            collectionCheck: false,
            defaultCapacity: _poolCapacity,
            maxSize: _poolSize
        );

        _spawnedEnemies = new List<int>() { 12, 18, 24};
        _spawnWave = true;
    }

    private void Update()
    {
        if (_spawnWave == false) return;

        if (_currentWaveSize == 0)
        {
            if (_currentWave == _spawnedEnemies.Count - 1)
            {
                Win?.Invoke();
                _spawnWave = false;
                return;
            }

            Debug.Log("Wave start");
            _currentWave++;
            _currentWaveSize = _spawnedEnemies[_currentWave] + 1;
            _currentBossHP = (int)(_currentBossHP * _scaleHP);

            StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        var interval = new WaitForSeconds(_spawnInterval);
        var bossInterval = new WaitForSeconds(_spawnInterval * 3f);

        for (int i = 0; i < _spawnedEnemies[_currentWave]; i++)
        {
            _pool.Get();
            yield return interval;
        }

        yield return bossInterval;
        _spawnBoss = true;
        _pool.Get();
    }

    private void OnGetFromPool(Enemy enemy)
    {
        float theta = Random.Range(0f, Mathf.PI * 2f);
        float x = _spawnRadious * Mathf.Cos(theta);
        float z = _spawnRadious * Mathf.Sin(theta);

        enemy.transform.position = new Vector3(x, 0f, z) + transform.position;
        enemy.gameObject.SetActive(true);

        if (_spawnBoss)
        {
            enemy.SetBoss(_currentBossHP);
            _spawnBoss = false;
        }
        else
        {
            enemy.SetEnemy();
        }

        enemy.Died += ReleasEnemy;
        Debug.Log("Spawned");
    }

    private void OnReleaseToPool(Enemy enemy)
    {
        enemy.Died -= ReleasEnemy;
        enemy.gameObject.SetActive(false);
    }

    private void ReleasEnemy(Enemy enemy)
    {
        _pool.Release(enemy);
        _currentWaveSize--;
    }
}

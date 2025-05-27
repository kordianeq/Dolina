using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemiesManager : MonoBehaviour
{
    [SerializeField] private EnemiesToSpawn[] _enemiesToSpawn;
    [SerializeField] private BoxCollider2D _spawnArea;

    private List<GameObject> _spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        if (_spawnedEnemies.Count == 0)
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in _enemiesToSpawn)
        {
            for (int i = 0; i < enemy.NumberToSpawn; i++)
            {
                Vector2 spawnPosition = GetRandomPositionInBox();
                GameObject go = Instantiate(enemy.EnemyPrefab, spawnPosition, Quaternion.identity);
                _spawnedEnemies.Add(go);
            }
        }
    }

    private Vector2 GetRandomPositionInBox()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

    [System.Serializable]
    private class EnemiesToSpawn
    {
        public int NumberToSpawn;
        public GameObject EnemyPrefab;
    }

    #region Save and Load

    public void Save(ref EnemiesSaveData saveData)
    {
        saveData.Enemies = new EnemySaveData[_spawnedEnemies.Count];
        for (int i = 0; i < _spawnedEnemies.Count; i++)
        {
            EnemyCore enemyCore = _spawnedEnemies[i].GetComponent<EnemyCore>();
            saveData.Enemies[i] = new EnemySaveData
            {
                Hp = enemyCore.hp,
                Dead = enemyCore.dead,
                Position = _spawnedEnemies[i].transform.position,
                EnemyPrefab = _spawnedEnemies[i]
            };
        }
    }
    
    public void Load(EnemiesSaveData saveData)
    {
        for (int i = 0; i < saveData.Enemies.Length; i++)
        {
            EnemySaveData enemyData = saveData.Enemies[i];
            GameObject go = Instantiate(enemyData.EnemyPrefab, enemyData.Position, Quaternion.identity);
            EnemyCore enemyCore = go.GetComponent<EnemyCore>();
            enemyCore.hp = enemyData.Hp;
            enemyCore.dead = enemyData.Dead;
            _spawnedEnemies.Add(go);
        }
    }

    #endregion
}

[System.Serializable]
public struct EnemiesSaveData
{
    public EnemySaveData[] Enemies;
    
}

[System.Serializable]
public struct EnemySaveData
{
    public float Hp;
    public bool Dead;
    public Vector3 Position;
    public GameObject EnemyPrefab;
}




using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public int enemiesNumber;

    private Dictionary<GameObject, GameObject> _enemyToPrefabMap = new Dictionary<GameObject, GameObject>();
    public List<GameObject> enemies;
    private void Start()
    {
        //count enemies on scene
        ChceckEnemiesState();
        enemiesNumber = enemies.Count;
        EnemyToPrefabMap();

    }

    void EnemyToPrefabMap()
    {
        foreach (var enemy in enemies)
        {
            EnemyPrefabReference reference = enemy.GetComponent<EnemyPrefabReference>();
            if (reference != null && reference.prefab != null)
            {
                _enemyToPrefabMap[enemy] = reference.prefab;
            }
            else
            {
                Debug.LogWarning($"Enemy {enemy.name} is missing prefab reference!");
            }
        }

    }
    void ChceckEnemiesState()
    {
        enemies.Clear();
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Enemy");


        for (int i = 0; i < gameObjects.Length; i++)
        {
            enemies.Add(gameObjects[i]);
            
        }
    }



    [System.Serializable]
    private class EnemiesToSpawn
    {
        public int NumberToSpawn;
        public GameObject EnemyPrefab;
    }

    #region Save and Load

    public void Save(ref SceneEnemyData data)
    {
        ChceckEnemiesState();

        List<EnemySaveData> enemySaveDataList = new List<EnemySaveData>();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] != null)
            {
                GameObject enemy = enemies[i];
                EnemySaveData saveData = new EnemySaveData
                {
                    //Data to save
                    Position = enemy.transform.position,
                    Hp =200f, // Assuming a default HP value, adjust as needed
                    EnemyPrefab = _enemyToPrefabMap[enemy]
                };

                enemySaveDataList.Add(saveData);
            }
            else
            {
                enemies.RemoveAt(i);
            }
        }
        data.Enemies = enemySaveDataList.ToArray();
    }

    public void Load(SceneEnemyData data)
    {
       // ChceckEnemiesState();

        //Destroy existing enemies
        foreach (var enemy in enemies)
        {
            if(enemy != null)
            {
                Destroy(enemy);
            }
        }

        enemies.Clear();
        _enemyToPrefabMap.Clear();

        //ChceckEnemiesState();
        foreach (var enemyData in data.Enemies )
        {
            Debug.Log("przeciwnik: " + enemyData.EnemyPrefab.name);
            if (enemyData.EnemyPrefab != null)
            {
                GameObject spawnedEnemy = Instantiate(enemyData.EnemyPrefab, enemyData.Position, Quaternion.identity);
                spawnedEnemy.GetComponent<EnemyCore>().hp = enemyData.Hp;
                spawnedEnemy.GetComponent<EnemyCore>().dead = enemyData.Dead;
                enemies.Add(spawnedEnemy);
                _enemyToPrefabMap[spawnedEnemy] = enemyData.EnemyPrefab;
            }
        }
    }

    #endregion
}

[System.Serializable]
public struct SceneEnemyData
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




using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    
    public int zoneID; // ID strefy, w której znajdują się wrogowie. Może być używane do różnych celów, np. do określenia poziomu trudności lub typu wrogów w danej strefie.

    public int enemiesNumber;

    [Header("Baza Prefabów")]
    [Tooltip("Lista prefabów. Index na tej liście odpowiada 'prefabID' w skrypcie EnemySave.")]
    public List<GameObject> enemyPrefabs;

    // Zamiast trzyma martwe dane, trzymamy referencje do aktywnych wrogw
    public List<EnemySave> activeEnemies = new List<EnemySave>();

    private void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemiesManager(this);
    }

    private void Update()
    {
        enemiesNumber = activeEnemies.Count; // Opcjonalnie do podglądu w Inspektorze
    }

    // Nowe, proste metody do zarz�dzania list�
    public void RegisterEnemy(EnemySave enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemySave enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    #region Save and Load

    public void Save(ref SceneEnemyData data)
    {
        List<EnemySaveData> enemySaveDataList = new List<EnemySaveData>();

        // Zbieramy dane z momentu klikni�cia SAVE
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemySave enemy = activeEnemies[i];

            if (enemy != null && enemy.enemyCore != null)
            {
                float currentHp = (enemy.enemyCore.dmgMannager != null) ? enemy.enemyCore.dmgMannager.EnemyHp : 0f;

                // Nie zapisujemy martwych wrog�w
                if (currentHp <= 0 || enemy.enemyCore.dead) continue;

                EnemySaveData saveData = new EnemySaveData
                {
                    Position = enemy.transform.position,
                    Hp = currentHp,
                    PrefabID = enemy.prefabID
                };

                enemySaveDataList.Add(saveData);
            }
        }

        data.Enemies = enemySaveDataList.ToArray();
    }

    public void Load(SceneEnemyData data)
    {
        // 1. Zniszcz obecnych wrog�w na scenie (�eby ich nie dublowa�)
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                Destroy(activeEnemies[i].gameObject);
            }
        }
        activeEnemies.Clear();

        // Je�li tablica jest pusta lub null (wszyscy nie �yj�), ko�czymy tu wczytywanie
        if (data.Enemies == null || data.Enemies.Length == 0) return;

        // 2. Spawn wrog�w z pliku
        foreach (var enemyData in data.Enemies)
        {
            if (enemyPrefabs != null && enemyData.PrefabID >= 0 && enemyData.PrefabID < enemyPrefabs.Count)
            {
                GameObject prefab = enemyPrefabs[enemyData.PrefabID];
                if (prefab != null)
                {
                    // Spawnujemy. Skrypt EnemySave na nim automatycznie odpali Start() i zarejestruje go do listy 'activeEnemies'
                    GameObject spawnedEnemy = Instantiate(prefab, enemyData.Position, Quaternion.identity);

                    // Nadpisanie wczytanego HP
                    EnemyCore core = spawnedEnemy.GetComponent<EnemyCore>();
                    if (core != null && core.dmgMannager != null)
                    {
                        core.dmgMannager.EnemyHp = enemyData.Hp;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Brak prefabu o ID {enemyData.PrefabID} w li�cie enemyPrefabs!");
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
    public Vector3 Position;
    public float Hp;
    public int PrefabID; // U�ywamy ID zamiast GameObjectu do zapisu w JSON
}




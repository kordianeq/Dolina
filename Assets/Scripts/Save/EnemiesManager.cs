using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance { get; private set; }

    [Header("Strefy wrogów")]
    public List<EnemyZone> zones = new List<EnemyZone>();

    [Tooltip("Liczba aktualnie żywych wrogów we wszystkich strefach.")]
    public int enemiesNumber;

    [Header("Aktywna Strefa")]
    [Tooltip("Strefa, w której gracz aktualnie walczy.")]
    public EnemyZone currentZone;

    [Header("Baza Prefabów")]
    [Tooltip("Lista prefabów. Index na tej liście odpowiada 'prefabID' w skrypcie EnemySave.")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    public void SetCurrentZone(EnemyZone zone)
    {
        currentZone = zone;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemiesManager(this);
        }

        CleanNullZones();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CleanNullZones()
    {
        if (zones == null)
        {
            zones = new List<EnemyZone>();
            return;
        }

        for (int i = zones.Count - 1; i >= 0; i--)
        {
            if (zones[i] == null)
                zones.RemoveAt(i);
        }
    }

    public void RegisterZone(EnemyZone zone)
    {
        if (zone == null)
            return;

        zone.SetEnemiesManager(this);

        if (zones == null)
            zones = new List<EnemyZone>();

        if (!zones.Contains(zone))
        {
            EnsureUniqueZoneID(zone);
            zones.Add(zone);
        }

        enemiesNumber = GetEnemyCount();
    }

    private void EnsureUniqueZoneID(EnemyZone zone)
    {
        bool duplicate = false;
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] != null && zones[i] != zone && zones[i].zoneID == zone.zoneID)
            {
                duplicate = true;
                break;
            }
        }

        if (duplicate)
        {
            int nextId = 0;
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null && zones[i].zoneID >= nextId)
                    nextId = zones[i].zoneID + 1;
            }
            Debug.Log($"[EnemiesManager] Strefa '{zone.gameObject.name}' miała zduplikowane zoneID={zone.zoneID}. Przypisano nowe unikalne ID: {nextId}");
            zone.zoneID = nextId;
        }
    }

    public void UnregisterZone(EnemyZone zone)
    {
        if (zone == null || zones == null)
            return;

        zones.Remove(zone);
        enemiesNumber = GetEnemyCount();
    }

    public EnemyZone GetZone(int zoneID, string zoneName = null)
    {
        CleanNullZones();

        if (!string.IsNullOrEmpty(zoneName))
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null && zones[i].gameObject.name == zoneName)
                    return zones[i];
            }
        }

        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] != null && zones[i].zoneID == zoneID)
                return zones[i];
        }

        return null;
    }

    public void RegisterEnemy(EnemySave enemy)
    {
        if (enemy == null)
            return;

        if (enemy.zone != null)
        {
            enemy.zone.RegisterEnemy(enemy);
        }
        else
        {
            EnemyZone zone = ResolveZone(enemy);
            if (zone == null)
            {
                zone = CreateZone(enemy.zoneID);
            }
            zone.RegisterEnemy(enemy);
        }

        enemiesNumber = GetEnemyCount();
    }

    public void UnregisterEnemy(EnemySave enemy)
    {
        if (enemy == null)
            return;

        if (enemy.zone != null)
        {
            enemy.zone.UnregisterEnemy(enemy);
        }
        else
        {
            CleanNullZones();
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null)
                    zones[i].UnregisterEnemy(enemy);
            }
        }

        enemiesNumber = GetEnemyCount();
    }

    private EnemyZone ResolveZone(EnemySave enemy)
    {
        if (enemy == null)
            return null;

        if (enemy.zone != null)
            return enemy.zone;

        if (enemy.zoneID >= 0)
        {
            EnemyZone zone = GetZone(enemy.zoneID);
            if (zone != null)
            {
                enemy.zone = zone;
                return zone;
            }
        }

        EnemyZone zoneFromHierarchy = enemy.GetComponentInParent<EnemyZone>();
        if (zoneFromHierarchy != null)
        {
            enemy.zone = zoneFromHierarchy;
            enemy.zoneID = zoneFromHierarchy.zoneID;
            RegisterZone(zoneFromHierarchy);
            return zoneFromHierarchy;
        }

        return null;
    }

    public EnemyZone CreateZone(int zoneID)
    {
        GameObject zoneObject = new GameObject($"EnemyZone_{zoneID}");
        zoneObject.transform.SetParent(transform, false);

        EnemyZone zone = zoneObject.AddComponent<EnemyZone>();
        zone.zoneID = zoneID;
        RegisterZone(zone);
        return zone;
    }

    public int GetEnemyCount()
    {
        if (zones == null) return 0;

        int count = 0;
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] != null)
                count += zones[i].ActiveEnemiesCount;
        }

        return count;
    }

    /// <summary>
    /// Resetuje wrogów we wszystkich strefach (np. po śmierci gracza lub wczytaniu checkpointa).
    /// </summary>
    public void ResetAllZones()
    {
        CleanNullZones();
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] != null)
            {
                zones[i].ResetZone();
            }
        }
        enemiesNumber = GetEnemyCount();
    }

    public void Save(ref SceneEnemyData data)
    {
        CleanNullZones();

        if (zones.Count == 0)
        {
            data.Zones = new EnemyZoneSaveData[0];
            data.Enemies = new EnemySaveData[0];
            return;
        }

        List<EnemyZoneSaveData> zoneDataList = new List<EnemyZoneSaveData>();

        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] == null)
                continue;

            zoneDataList.Add(zones[i].Save());
        }

        data.Zones = zoneDataList.ToArray();
        data.Enemies = new EnemySaveData[0]; // Zgodność wsteczna
    }

    public void Load(SceneEnemyData data)
    {
        CleanNullZones();

        if (data.Zones != null && data.Zones.Length > 0)
        {
            for (int i = 0; i < data.Zones.Length; i++)
            {
                EnemyZone zone = GetZone(data.Zones[i].ZoneID, data.Zones[i].ZoneName);
                if (zone == null)
                    zone = CreateZone(data.Zones[i].ZoneID);

                zone.SetEnemiesManager(this);
                zone.Load(data.Zones[i]);
            }
            enemiesNumber = GetEnemyCount();
            return;
        }

        // Jeśli brak zapisu stref, resetujemy obecne strefy do stanu startowego
        ResetAllZones();
    }
}

[System.Serializable]
public struct SceneEnemyData
{
    public EnemyZoneSaveData[] Zones;
    public EnemySaveData[] Enemies; // Legacy support
}

[System.Serializable]
public struct EnemyZoneSaveData
{
    public int ZoneID;
    public string ZoneName;
    public bool IsCleared;
    public bool RespawnAllOnReload;
    public bool HasBeenTriggered;
    public EnemySaveData[] Enemies;
}

[System.Serializable]
public struct EnemySaveData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Hp;
    public int PrefabID;
}



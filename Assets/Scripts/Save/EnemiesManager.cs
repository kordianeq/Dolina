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

    [Header("Aktywni Wrogowie (Publiczna Lista dla Wszystkich Skryptów)")]
    [Tooltip("Publiczna lista wszystkich aktualnie żywych i aktywnych wrogów (EnemySave) na scenie.")]
    public List<EnemySave> activeEnemies = new List<EnemySave>();

    [Tooltip("Publiczna lista komponentów EnemyCore wszystkich aktualnie aktywnych wrogów na scenie.")]
    public List<EnemyCore> activeEnemyCores = new List<EnemyCore>();

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
        RefreshActiveEnemiesList();
    }

    private void Start()
    {
        RefreshActiveEnemiesList();
    }

    private void Update()
    {
        CleanDeadAndNullEnemies();
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

        if (enemy.gameObject.activeInHierarchy && (enemy.enemyCore == null || !enemy.enemyCore.dead))
        {
            RegisterActiveEnemy(enemy);
        }

        enemiesNumber = GetEnemyCount();
    }

    public void RegisterEnemy(EnemyCore core)
    {
        if (core == null) return;
        EnemySave save = core.GetComponent<EnemySave>();
        if (save != null)
        {
            RegisterEnemy(save);
        }
        else
        {
            RegisterActiveEnemy(core);
        }
    }

    public void UnregisterEnemy(EnemySave enemy)
    {
        if (enemy == null)
            return;

        UnregisterActiveEnemy(enemy);

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

    public void UnregisterEnemy(EnemyCore core)
    {
        if (core == null) return;
        EnemySave save = core.GetComponent<EnemySave>();
        if (save != null)
        {
            UnregisterEnemy(save);
        }
        else
        {
            UnregisterActiveEnemy(core);
        }
    }

    /// <summary>
    /// Rejestruje wroga na liście aktywnych przeciwników w grze.
    /// </summary>
    public void RegisterActiveEnemy(EnemySave enemy)
    {
        if (enemy == null) return;
        if (!enemy.gameObject.activeInHierarchy) return;
        if (enemy.enemyCore != null && enemy.enemyCore.dead) return;

        if (activeEnemies == null) activeEnemies = new List<EnemySave>();
        if (activeEnemyCores == null) activeEnemyCores = new List<EnemyCore>();

        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }

        EnemyCore core = enemy.enemyCore != null ? enemy.enemyCore : enemy.GetComponent<EnemyCore>();
        if (core != null && !core.dead && !activeEnemyCores.Contains(core))
        {
            activeEnemyCores.Add(core);
        }

        enemiesNumber = GetEnemyCount();
    }

    /// <summary>
    /// Rejestruje EnemyCore na liście aktywnych przeciwników w grze.
    /// </summary>
    public void RegisterActiveEnemy(EnemyCore core)
    {
        if (core == null || core.dead) return;
        if (!core.gameObject.activeInHierarchy) return;

        if (activeEnemyCores == null) activeEnemyCores = new List<EnemyCore>();
        if (activeEnemies == null) activeEnemies = new List<EnemySave>();

        if (!activeEnemyCores.Contains(core))
        {
            activeEnemyCores.Add(core);
        }

        EnemySave save = core.GetComponent<EnemySave>();
        if (save != null && !activeEnemies.Contains(save))
        {
            activeEnemies.Add(save);
        }

        enemiesNumber = GetEnemyCount();
    }

    /// <summary>
    /// Usuwa wroga z listy aktywnych przeciwników (np. po śmierci lub usunięciu z areny).
    /// </summary>
    public void UnregisterActiveEnemy(EnemySave enemy)
    {
        if (enemy == null) return;

        if (activeEnemies != null)
        {
            activeEnemies.Remove(enemy);
        }

        EnemyCore core = enemy.enemyCore != null ? enemy.enemyCore : enemy.GetComponent<EnemyCore>();
        if (core != null && activeEnemyCores != null)
        {
            activeEnemyCores.Remove(core);
        }

        enemiesNumber = GetEnemyCount();
    }

    /// <summary>
    /// Usuwa EnemyCore z listy aktywnych przeciwników.
    /// </summary>
    public void UnregisterActiveEnemy(EnemyCore core)
    {
        if (core == null) return;

        if (activeEnemyCores != null)
        {
            activeEnemyCores.Remove(core);
        }

        EnemySave save = core.GetComponent<EnemySave>();
        if (save != null && activeEnemies != null)
        {
            activeEnemies.Remove(save);
        }

        enemiesNumber = GetEnemyCount();
    }

    /// <summary>
    /// Wywoływane w momencie śmierci wroga, aby natychmiast wycofać go z listy aktywnych celów.
    /// </summary>
    public void NotifyEnemyDied(EnemySave enemy)
    {
        UnregisterActiveEnemy(enemy);
    }

    public void NotifyEnemyDied(EnemyCore core)
    {
        UnregisterActiveEnemy(core);
    }

    /// <summary>
    /// Czyści listę aktywnych wrogów z obiektów usuniętych (null), nieaktywnych lub martwych.
    /// </summary>
    public void CleanDeadAndNullEnemies()
    {
        if (activeEnemies != null)
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemySave enemy = activeEnemies[i];
                if (enemy == null || enemy.gameObject == null || !enemy.gameObject.activeInHierarchy ||
                    (enemy.enemyCore != null && enemy.enemyCore.dead))
                {
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        if (activeEnemyCores != null)
        {
            for (int i = activeEnemyCores.Count - 1; i >= 0; i--)
            {
                EnemyCore core = activeEnemyCores[i];
                if (core == null || core.gameObject == null || !core.gameObject.activeInHierarchy || core.dead)
                {
                    activeEnemyCores.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Odświeża i synchronizuje pełną listę aktywnych wrogów w całej scenie.
    /// </summary>
    public void RefreshActiveEnemiesList()
    {
        if (activeEnemies == null) activeEnemies = new List<EnemySave>();
        if (activeEnemyCores == null) activeEnemyCores = new List<EnemyCore>();

        activeEnemies.Clear();
        activeEnemyCores.Clear();

        // 1. Dodaj wrogów ze stref
        CleanNullZones();
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i] == null) continue;
            for (int j = 0; j < zones[i].activeEnemies.Count; j++)
            {
                EnemySave e = zones[i].activeEnemies[j];
                if (e != null && e.gameObject.activeInHierarchy && (e.enemyCore == null || !e.enemyCore.dead))
                {
                    if (!activeEnemies.Contains(e)) activeEnemies.Add(e);
                    EnemyCore core = e.enemyCore ?? e.GetComponent<EnemyCore>();
                    if (core != null && !core.dead && !activeEnemyCores.Contains(core)) activeEnemyCores.Add(core);
                }
            }
        }

        // 2. Dodaj aktywnych wrogów ze sceny
        EnemySave[] allSaves = FindObjectsByType<EnemySave>(FindObjectsSortMode.None);
        for (int i = 0; i < allSaves.Length; i++)
        {
            EnemySave e = allSaves[i];
            if (e != null && e.gameObject.activeInHierarchy && (e.enemyCore == null || !e.enemyCore.dead))
            {
                if (!activeEnemies.Contains(e)) activeEnemies.Add(e);
                EnemyCore core = e.enemyCore ?? e.GetComponent<EnemyCore>();
                if (core != null && !core.dead && !activeEnemyCores.Contains(core)) activeEnemyCores.Add(core);
            }
        }

        // 3. Dodaj aktywne EnemyCore bez komponentu EnemySave (np. testowe prefabrykaty)
        EnemyCore[] allCores = FindObjectsByType<EnemyCore>(FindObjectsSortMode.None);
        for (int i = 0; i < allCores.Length; i++)
        {
            EnemyCore c = allCores[i];
            if (c != null && c.gameObject.activeInHierarchy && !c.dead)
            {
                if (!activeEnemyCores.Contains(c)) activeEnemyCores.Add(c);
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
        RefreshActiveEnemiesList();
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
            RefreshActiveEnemiesList();
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



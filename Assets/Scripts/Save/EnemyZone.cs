using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyZone : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointInfo
    {
        public EnemySave enemySave;
        public GameObject enemyInstance;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public float initialHp;
        public int prefabID;
        public GameObject prefab;
    }

    [Header("Podstawowe")]
    public int zoneID;

    [Tooltip("Jeśli włączone, po wczytaniu gry / śmierci gracza wszyscy przeciwnicy wracają do życia na pozycjach startowych.")]
    public bool respawnAllOnReload = true;

    [Tooltip("Czy strefa została całkowicie oczyszczona z przeciwników.")]
    public bool isCleared = false;

    [Tooltip("Jeśli strefa została oczyszczona, czy ma pozostać pusta po wczytaniu checkpointa?")]
    public bool keepClearedOnLoad = true;

    [Header("Wykrywanie wrogów i Triggery")]
    [Tooltip("Główny collider strefy (np. BoxCollider obejmujący obszar areny). Służy do wykrywania wrogów wewnątrz strefy.")]
    public Collider zoneCollider;

    [Tooltip("Czy główny collider strefy ma działać jako trigger wyzwalający walkę? Ustaw na false, jeśli chcesz startować arenę tylko z osobnego obiektu-triggera.")]
    public bool useZoneColliderAsTrigger = true;

    [Tooltip("Opcjonalny pojedynczy collider wejściowy (dla kompatybilności wstecznej).")]
    public Collider startTriggerCollider;

    [Tooltip("Lista osobnych colliderów/obiektów startowych (np. wejście główne, korytarz, okno). EnemyZone automatycznie podepnie pod nie obsługę wejścia gracza.")]
    public List<Collider> startTriggerColliders = new List<Collider>();

    [Tooltip("Lista zarejestrowanych komponentów ArenaTrigger dla tej strefy.")]
    public List<ArenaTrigger> registeredTriggers = new List<ArenaTrigger>();

    [Tooltip("Czy po wejściu gracza usunąć (Destroy) ten trigger oraz wszystkie pozostałe triggery powiązane z tą strefą?")]
    public bool destroyTriggersOnEnter = true;

    [Tooltip("Lista aktywnych wrogów przypisanych do strefy.")]
    public List<EnemySave> activeEnemies = new List<EnemySave>();

    [Header("Zapamiętane punkty startowe")]
    [SerializeField] private List<SpawnPointInfo> initialEnemies = new List<SpawnPointInfo>();

    [Header("Eventy Areny (Doom Style)")]
    public UnityEvent onZoneCleared;
    public UnityEvent onZoneReset;

    [Header("Tryb Spawnowania Areny (Doom Style)")]
    [Tooltip("Jeśli włączone, przeciwnicy są uśpieni (ukryci) na starcie i pojawiają się dopiero po wejściu gracza na arenę.")]
    public bool spawnOnPlayerEnter = true;

    [Tooltip("Czy strefa została już wyzwolona (wrogowie pojawili się na arenie).")]
    public bool hasBeenTriggered = false;

    [Tooltip("Opcjonalny prefab efektu spawnu (np. cząsteczki dymu, portal, błyskawica) spawnowany na pozycji każdego wroga.")]
    public GameObject spawnEffectPrefab;

    [Tooltip("Opcjonalny dźwięk spawnu/rozpoczęcia walki.")]
    public AudioClip spawnSound;

    [Tooltip("Event wywoływany w momencie wejścia gracza na arenę (np. zatrzaśnięcie drzwi, włączenie muzyki walki, otwarcie monster closets).")]
    public UnityEvent onZoneTriggered;

    [Header("Licznik Wrogów (UI)")]
    [Tooltip("Czy ta strefa ma wyświetlać liczbę wrogów w UI (enemyCountText)?")]
    public bool showEnemyCountInUI = true;

    [Tooltip("Po ilu sekundach od pokonania wszystkich wrogów wyczyścić licznik w UI (0 = nie czyść)?")]
    public float clearUIDelayOnCleared = 3f;

    [Tooltip("Czy wyczyścić licznik w UI po opuszczeniu collidera strefy (jeśli strefa nie została ukończona)?")]
    public bool clearUIOnTriggerExit = false;

    [Header("Checkpointy (Automatyczny Zapis)")]
    [Tooltip("Czy automatycznie zapisać checkpoint po zabiciu wszystkich przeciwników w strefie?")]
    public bool saveCheckpointOnCleared = true;

    [Tooltip("Czy automatycznie zapisać checkpoint, gdy gracz wejdzie do strefy (w trakcie lub przed areną)?")]
    public bool saveCheckpointOnEnter = false;

    [Tooltip("Czy checkpoint przy wejściu do strefy ma zapisać się tylko raz?")]
    public bool saveCheckpointOnEnterOnce = true;

    private bool enterCheckpointSaved = false;

    private EnemiesManager enemiesManager;

    public int ActiveEnemiesCount
    {
        get
        {
            if (isCleared) return 0;

            // Jeśli wrogowie są uśpieni przed wejściem na arenę, żaden jeszcze nie zginął
            if (spawnOnPlayerEnter && !hasBeenTriggered)
            {
                return initialEnemies.Count;
            }

            int count = 0;
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemySave enemy = activeEnemies[i];
                if (enemy == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                if (enemy.enemyCore != null && (enemy.enemyCore.dead || (enemy.enemyCore.dmgMannager != null && enemy.enemyCore.dmgMannager.EnemyHp <= 0)))
                {
                    continue;
                }

                count++;
            }
            return count;
        }
    }

    private void Awake()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<Collider>();

        if (startTriggerCollider != null && !startTriggerColliders.Contains(startTriggerCollider))
        {
            startTriggerColliders.Add(startTriggerCollider);
        }

        for (int i = 0; i < startTriggerColliders.Count; i++)
        {
            Collider col = startTriggerColliders[i];
            if (col != null)
            {
                ArenaTrigger trigger = col.GetComponent<ArenaTrigger>();
                if (trigger == null)
                {
                    trigger = col.gameObject.AddComponent<ArenaTrigger>();
                }
                trigger.targetZone = this;
                RegisterTrigger(trigger);
            }
        }

        RegisterToManager();
        InitializeZoneEnemies();
    }

    private void OnEnable()
    {
        RegisterToManager();
    }

    private void OnDisable()
    {
        UnregisterFromManager();
    }

    private void OnDestroy()
    {
        UnregisterFromManager();
    }

    private void RegisterToManager()
    {
        if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.RegisterZone(this);
            enemiesManager = EnemiesManager.Instance;
        }
    }

    private void UnregisterFromManager()
    {
        if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.UnregisterZone(this);
        }
    }

    public void SetEnemiesManager(EnemiesManager manager)
    {
        enemiesManager = manager;
    }

    public void InitializeZoneEnemies()
    {
        // 1. Zbieranie z hierarchii (dzieci)
        EnemySave[] children = GetComponentsInChildren<EnemySave>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (!activeEnemies.Contains(children[i]))
            {
                activeEnemies.Add(children[i]);
            }
        }

        // 2. Zbieranie z collidera (jeśli istnieje)
        if (zoneCollider != null)
        {
            EnemySave[] allInScene = FindObjectsByType<EnemySave>(FindObjectsSortMode.None);
            Bounds bounds = zoneCollider.bounds;
            for (int i = 0; i < allInScene.Length; i++)
            {
                if (bounds.Contains(allInScene[i].transform.position) && !activeEnemies.Contains(allInScene[i]))
                {
                    activeEnemies.Add(allInScene[i]);
                }
            }
        }

        // 3. Inicjalizacja i zapamiętanie pozycji startowych
        initialEnemies.Clear();
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemySave enemy = activeEnemies[i];
            if (enemy == null) continue;

            enemy.zone = this;
            enemy.zoneID = zoneID;
            enemy.InitInitialState();

            SpawnPointInfo info = new SpawnPointInfo
            {
                enemySave = enemy,
                enemyInstance = enemy.gameObject,
                spawnPosition = enemy.InitialPosition,
                spawnRotation = enemy.InitialRotation,
                initialHp = enemy.InitialHp,
                prefabID = enemy.prefabID
            };
            initialEnemies.Add(info);
        }

        // 4. Jeśli włączony jest tryb spawnowania po wejściu, uśpij wrogów na starcie
        if (spawnOnPlayerEnter && !isCleared && !hasBeenTriggered)
        {
            SleepEnemies();
        }
    }

    public void SleepEnemies()
    {
        for (int i = 0; i < initialEnemies.Count; i++)
        {
            if (initialEnemies[i].enemyInstance != null)
            {
                initialEnemies[i].enemyInstance.SetActive(false);
            }
        }
        activeEnemies.Clear();
    }

    public void RegisterEnemy(EnemySave enemy)
    {
        if (enemy == null)
            return;

        enemy.zone = this;
        enemy.zoneID = zoneID;
        enemy.InitInitialState();

        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }

        bool foundInInitial = false;
        for (int i = 0; i < initialEnemies.Count; i++)
        {
            if (initialEnemies[i].enemySave == enemy || initialEnemies[i].enemyInstance == enemy.gameObject)
            {
                foundInInitial = true;
                break;
            }
        }

        if (!foundInInitial)
        {
            initialEnemies.Add(new SpawnPointInfo
            {
                enemySave = enemy,
                enemyInstance = enemy.gameObject,
                spawnPosition = enemy.InitialPosition,
                spawnRotation = enemy.InitialRotation,
                initialHp = enemy.InitialHp,
                prefabID = enemy.prefabID
            });
        }

        if (showEnemyCountInUI && IsActiveZone())
        {
            UpdateEnemyCountUI();
        }
    }

    private UiMenager GetUiManager()
    {
        if (UiMenager.Instance != null) return UiMenager.Instance;
        if (GameManager.Instance != null && GameManager.Instance.UiMenager != null) return GameManager.Instance.UiMenager;
        return null;
    }

    public bool IsActiveZone()
    {
        if (EnemiesManager.Instance != null && EnemiesManager.Instance.currentZone != null)
        {
            return EnemiesManager.Instance.currentZone == this;
        }
        return hasBeenTriggered || !spawnOnPlayerEnter;
    }

    public void UpdateEnemyCountUI()
    {
        if (!showEnemyCountInUI) return;
        if (!IsActiveZone()) return;

        int total = initialEnemies.Count;
        if (total == 0) return;

        int remaining = ActiveEnemiesCount;

        UiMenager ui = GetUiManager();
        if (ui != null)
        {
            ui.UpdateEnemyCount(remaining, total);
        }
    }

    public void UnregisterEnemy(EnemySave enemy)
    {
        if (enemy == null)
            return;

        activeEnemies.Remove(enemy);
        CheckZoneCleared();
    }

    public void NotifyEnemyDied(EnemySave enemy)
    {
        CheckZoneCleared();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useZoneColliderAsTrigger) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null)
        {
            OnPlayerEnterZone();
        }
    }

    public void RegisterTrigger(ArenaTrigger trigger)
    {
        if (trigger != null && !registeredTriggers.Contains(trigger))
        {
            registeredTriggers.Add(trigger);
        }
    }

    public void UnregisterTrigger(ArenaTrigger trigger)
    {
        if (trigger != null)
        {
            registeredTriggers.Remove(trigger);
        }
    }

    public void DestroyAllTriggers()
    {
        for (int i = registeredTriggers.Count - 1; i >= 0; i--)
        {
            ArenaTrigger trigger = registeredTriggers[i];
            if (trigger != null)
            {
                if (trigger.gameObject != gameObject)
                {
                    Destroy(trigger.gameObject);
                }
                else
                {
                    Destroy(trigger);
                }
            }
        }
        registeredTriggers.Clear();
    }

    public void OnPlayerEnterZone()
    {
        if (EnemiesManager.Instance != null && !isCleared)
        {
            EnemiesManager.Instance.SetCurrentZone(this);
        }

        // 1. Spawnowanie / Aktywacja wrogów na arenie (Doom Style)
        if (spawnOnPlayerEnter && !hasBeenTriggered && !isCleared)
        {
            Debug.Log("Triggering Enemy Zone Spawn...");
            TriggerZoneSpawn();
        }
        else if (!isCleared && showEnemyCountInUI)
        {
            UpdateEnemyCountUI();
        }

        // 2. Checkpoint przy wejściu (jeśli włączony)
        if (saveCheckpointOnEnter)
        {
            if (!saveCheckpointOnEnterOnce || !enterCheckpointSaved)
            {
                enterCheckpointSaved = true;
                Debug.Log($"[EnemyZone {zoneID}] Zapisano checkpoint przy wejściu gracza do strefy.");
                SaveSystem.TriggerCheckpoint();
            }
        }

        // 3. Usunięcie wszystkich triggerów powiązanych z tą strefą (zapobiega powtórzeniu spawnu)
        if (destroyTriggersOnEnter)
        {
            DestroyAllTriggers();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (clearUIOnTriggerExit && (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null))
        {
            if (!isCleared && showEnemyCountInUI && IsActiveZone())
            {
                UiMenager ui = GetUiManager();
                if (ui != null)
                {
                    ui.ClearEnemyCount();
                }
            }
        }
    }

    public void TriggerZoneSpawn(bool force = false, bool playEffects = true)
    {
        if ((hasBeenTriggered && !force) || isCleared) return;
        hasBeenTriggered = true;

        Debug.Log($"[EnemyZone {zoneID}] Gracz wszedł na arenę! Spawnowanie wrogów ({initialEnemies.Count})...");

        if (playEffects)
        {
            onZoneTriggered?.Invoke();

            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, transform.position);
            }
        }

        activeEnemies.Clear();

        List<GameObject> prefabs = (EnemiesManager.Instance != null) ? EnemiesManager.Instance.enemyPrefabs : null;

        for (int i = 0; i < initialEnemies.Count; i++)
        {
            SpawnPointInfo info = initialEnemies[i];

            if (playEffects && spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, info.spawnPosition, info.spawnRotation);
            }

            if (info.enemyInstance != null)
            {
                info.enemyInstance.SetActive(true);
                if (info.enemySave != null)
                {
                    info.enemySave.ResetToSpawnPoint();
                    if (!activeEnemies.Contains(info.enemySave))
                    {
                        activeEnemies.Add(info.enemySave);
                    }
                }
            }
            else
            {
                GameObject prefabToSpawn = info.prefab;
                if (prefabToSpawn == null && prefabs != null && info.prefabID >= 0 && info.prefabID < prefabs.Count)
                {
                    prefabToSpawn = prefabs[info.prefabID];
                }

                if (prefabToSpawn != null)
                {
                    GameObject spawned = Instantiate(prefabToSpawn, info.spawnPosition, info.spawnRotation, transform);
                    info.enemyInstance = spawned;

                    EnemySave newSave = spawned.GetComponent<EnemySave>();
                    if (newSave == null)
                        newSave = spawned.AddComponent<EnemySave>();

                    newSave.prefabID = info.prefabID;
                    newSave.zone = this;
                    newSave.zoneID = zoneID;
                    newSave.SetInitialState(info.spawnPosition, info.spawnRotation, info.initialHp);

                    EnemyCore core = spawned.GetComponent<EnemyCore>();
                    if (core != null && core.dmgMannager != null)
                    {
                        core.dmgMannager.EnemyHp = info.initialHp;
                    }

                    info.enemySave = newSave;
                    activeEnemies.Add(newSave);
                }
                else
                {
                    Debug.LogWarning($"[EnemyZone {zoneID}] Nie można zrespawnować wroga (brak prefabu o ID {info.prefabID})");
                }
            }
        }

        if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.SetCurrentZone(this);
            EnemiesManager.Instance.enemiesNumber = EnemiesManager.Instance.GetEnemyCount();
        }

        UpdateEnemyCountUI();
    }

    public void CheckZoneCleared()
    {
        if (isCleared) return;

        // Jeśli strefa czeka na wejście gracza (wrogowie są uśpieni), arena jeszcze się nie rozpoczęła!
        if (spawnOnPlayerEnter && !hasBeenTriggered)
        {
            return;
        }

        int remaining = ActiveEnemiesCount;
        int total = initialEnemies.Count;

        if (remaining == 0 && total > 0)
        {
            isCleared = true;
            Debug.Log($"[EnemyZone {zoneID}] Wszyscy przeciwnicy pokonani! Strefa oczyszczona.");

            if (showEnemyCountInUI && IsActiveZone())
            {
                UiMenager ui = GetUiManager();
                if (ui != null)
                {
                    ui.UpdateEnemyCount(0, total);
                    if (clearUIDelayOnCleared > 0f)
                    {
                        ui.ClearEnemyCountDelayed(clearUIDelayOnCleared);
                    }
                }
            }

            onZoneCleared?.Invoke();

            if (saveCheckpointOnCleared)
            {
                SaveSystem.TriggerCheckpoint();
            }
        }
        else if (showEnemyCountInUI && IsActiveZone())
        {
            UpdateEnemyCountUI();
        }

        if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.enemiesNumber = EnemiesManager.Instance.GetEnemyCount();
        }
    }

    public void ResetZone()
    {
        if (isCleared && keepClearedOnLoad)
        {
            DestroyOrHideAllEnemies();
            return;
        }

        isCleared = false;
        enterCheckpointSaved = false;
        activeEnemies.Clear();

        List<GameObject> prefabs = (EnemiesManager.Instance != null) ? EnemiesManager.Instance.enemyPrefabs : null;

        for (int i = 0; i < initialEnemies.Count; i++)
        {
            SpawnPointInfo info = initialEnemies[i];

            // 1. Wróg nadal istnieje w scenie (żyje lub leży zabity)
            if (info.enemyInstance != null)
            {
                info.enemyInstance.SetActive(true);
                if (info.enemySave != null)
                {
                    info.enemySave.ResetToSpawnPoint();
                    if (!activeEnemies.Contains(info.enemySave))
                    {
                        activeEnemies.Add(info.enemySave);
                    }
                }
            }
            // 2. Wróg został zniszczony (np. Overkill / Destroy)
            else
            {
                GameObject prefabToSpawn = info.prefab;
                if (prefabToSpawn == null && prefabs != null && info.prefabID >= 0 && info.prefabID < prefabs.Count)
                {
                    prefabToSpawn = prefabs[info.prefabID];
                }

                if (prefabToSpawn != null)
                {
                    GameObject spawned = Instantiate(prefabToSpawn, info.spawnPosition, info.spawnRotation, transform);
                    info.enemyInstance = spawned;

                    EnemySave newSave = spawned.GetComponent<EnemySave>();
                    if (newSave == null)
                        newSave = spawned.AddComponent<EnemySave>();

                    newSave.prefabID = info.prefabID;
                    newSave.zone = this;
                    newSave.zoneID = zoneID;
                    newSave.SetInitialState(info.spawnPosition, info.spawnRotation, info.initialHp);

                    EnemyCore core = spawned.GetComponent<EnemyCore>();
                    if (core != null && core.dmgMannager != null)
                    {
                        core.dmgMannager.EnemyHp = info.initialHp;
                    }

                    info.enemySave = newSave;
                    activeEnemies.Add(newSave);
                }
                else
                {
                    Debug.LogWarning($"[EnemyZone {zoneID}] Nie można zrespawnować wroga (brak prefabu o ID {info.prefabID})");
                }
            }
        }

        if (spawnOnPlayerEnter)
        {
            hasBeenTriggered = false;
            SleepEnemies();
        }

        if (showEnemyCountInUI && IsActiveZone())
        {
            UiMenager ui = GetUiManager();
            if (ui != null)
            {
                ui.ClearEnemyCount();
            }
        }

        onZoneReset?.Invoke();
    }

    public void DestroyOrHideAllEnemies()
    {
        isCleared = true;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                activeEnemies[i].gameObject.SetActive(false);
            }
        }
        activeEnemies.Clear();

        for (int i = 0; i < initialEnemies.Count; i++)
        {
            if (initialEnemies[i].enemyInstance != null)
            {
                initialEnemies[i].enemyInstance.SetActive(false);
            }
        }

        if (showEnemyCountInUI && IsActiveZone())
        {
            UiMenager ui = GetUiManager();
            if (ui != null)
            {
                ui.ClearEnemyCount();
            }
        }
    }

    public void DestroyEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                Destroy(activeEnemies[i].gameObject);
            }
        }
        activeEnemies.Clear();
    }

    public EnemyZoneSaveData Save()
    {
        if (!spawnOnPlayerEnter || hasBeenTriggered)
        {
            CheckZoneCleared();
        }

        return new EnemyZoneSaveData
        {
            ZoneID = zoneID,
            ZoneName = gameObject.name,
            IsCleared = isCleared,
            RespawnAllOnReload = respawnAllOnReload,
            HasBeenTriggered = hasBeenTriggered,
            Enemies = new EnemySaveData[0]
        };
    }

    public void Load(EnemyZoneSaveData data, List<GameObject> enemyPrefabs = null)
    {
        zoneID = data.ZoneID;
        isCleared = data.IsCleared;
        respawnAllOnReload = data.RespawnAllOnReload;

        if (isCleared && keepClearedOnLoad)
        {
            hasBeenTriggered = true;
            DestroyOrHideAllEnemies();
            onZoneCleared?.Invoke();
        }
        else if (respawnAllOnReload)
        {
            ResetZone();
            if (data.HasBeenTriggered && spawnOnPlayerEnter)
            {
                TriggerZoneSpawn(force: true);
            }
        }
        else
        {
            hasBeenTriggered = data.HasBeenTriggered;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Find & Assign Enemies In Zone")]
    public void EditorFindAndAssignEnemies()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<Collider>();

        List<EnemySave> found = new List<EnemySave>();

        EnemySave[] children = GetComponentsInChildren<EnemySave>(true);
        foreach (var c in children)
        {
            if (!found.Contains(c)) found.Add(c);
        }

        if (zoneCollider != null)
        {
            EnemySave[] allInScene = FindObjectsByType<EnemySave>(FindObjectsSortMode.None);
            Bounds bounds = zoneCollider.bounds;
            foreach (var e in allInScene)
            {
                if (bounds.Contains(e.transform.position) && !found.Contains(e))
                {
                    found.Add(e);
                }
            }
        }

        UnityEditor.Undo.RecordObject(this, "Assign Enemies to Zone");
        foreach (var e in found)
        {
            if (!activeEnemies.Contains(e))
            {
                activeEnemies.Add(e);
            }
            e.zone = this;
            e.zoneID = zoneID;
            UnityEditor.EditorUtility.SetDirty(e);
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[EnemyZone {zoneID}] Znaleziono i przypisano {activeEnemies.Count} wrogów.");
    }
#endif
}

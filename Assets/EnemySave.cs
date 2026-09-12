using UnityEngine;

public class EnemySave : MonoBehaviour
{
    public EnemyCore enemyCore;
    public EnemyZone zone;

    [Tooltip("ID prefabu z listy 'Enemy Prefabs' w EnemiesManagerze (np. 0 dla pierwszego typu wroga).")]
    public int prefabID = 0;

    [Tooltip("ID strefy, do której należy ten wróg.")]
    public int zoneID = 0;

    public Vector3 InitialPosition { get; private set; }
    public Quaternion InitialRotation { get; private set; }
    public float InitialHp { get; private set; }
    public bool Initialized { get; private set; }

    private bool wasDead = false;

    private void Awake()
    {
        InitInitialState();
    }

    private void OnEnable()
    {
        if (enemyCore == null)
            enemyCore = GetComponent<EnemyCore>();

        if (enemyCore != null)
        {
            enemyCore.OnDeath -= HandleDeath;
            enemyCore.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (enemyCore != null)
        {
            enemyCore.OnDeath -= HandleDeath;
        }
    }

    private void Start()
    {
        ResolveZone();

        if (zone != null)
        {
            zone.RegisterEnemy(this);
        }
        else if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.RegisterEnemy(this);
        }
    }

    private void HandleDeath()
    {
        wasDead = true;
        if (zone != null)
        {
            zone.NotifyEnemyDied(this);
        }
    }

    private void Update()
    {
        if (enemyCore != null && enemyCore.dead != wasDead)
        {
            wasDead = enemyCore.dead;
            if (wasDead && zone != null)
            {
                zone.NotifyEnemyDied(this);
            }
        }
    }

    public void InitInitialState()
    {
        if (Initialized) return;

        InitialPosition = transform.position;
        InitialRotation = transform.rotation;

        if (enemyCore == null)
            enemyCore = GetComponent<EnemyCore>();

        if (enemyCore != null && enemyCore.dmgMannager != null)
        {
            InitialHp = enemyCore.dmgMannager.EnemyHp;
        }

        Initialized = true;
    }

    public void SetInitialState(Vector3 pos, Quaternion rot, float hp)
    {
        InitialPosition = pos;
        InitialRotation = rot;
        InitialHp = hp;
        Initialized = true;
    }

    public void ResetToSpawnPoint()
    {
        if (!Initialized)
            InitInitialState();

        transform.position = InitialPosition;
        transform.rotation = InitialRotation;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        if (enemyCore == null)
            enemyCore = GetComponent<EnemyCore>();

        if (enemyCore != null)
        {
            if (enemyCore.dmgMannager != null)
            {
                enemyCore.dmgMannager.EnemyHp = InitialHp;
            }

            enemyCore.dead = false;
            enemyCore.SetIncapacitated(false);
            enemyCore.SetStunned(false);
            enemyCore.SetMoveOverrideRotation(false);

            if (enemyCore.moveBrain != null)
                enemyCore.moveBrain.ForceResurectState();

            if (enemyCore.behaviorBrain != null)
                enemyCore.behaviorBrain.ForceResurectState();
        }

        wasDead = false;
        gameObject.SetActive(true);
    }

    public void ResolveZone()
    {
        if (zone != null)
        {
            zoneID = zone.zoneID;
            return;
        }

        EnemyZone parentZone = GetComponentInParent<EnemyZone>();
        if (parentZone != null)
        {
            zone = parentZone;
            zoneID = parentZone.zoneID;
            return;
        }

        if (EnemiesManager.Instance != null && zoneID >= 0)
        {
            EnemyZone foundZone = EnemiesManager.Instance.GetZone(zoneID);
            if (foundZone != null)
            {
                zone = foundZone;
            }
        }
    }

    private void OnDestroy()
    {
        if (zone != null)
        {
            zone.UnregisterEnemy(this);
        }
        else if (EnemiesManager.Instance != null)
        {
            EnemiesManager.Instance.UnregisterEnemy(this);
        }
    }
}
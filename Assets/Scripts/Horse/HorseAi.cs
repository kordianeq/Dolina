

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HorseAi : MonoBehaviour, IKickeable
{
    [Header("Komponenty")]
    public Animator horseAnimator;
    public NavMeshAgent agent;
    public NavMeshAgent navMeshAgent; // Kompatybilność wsteczna
    public Breakeable breakableScript;
    public Horse horseData;

    [Header("Odwołanie do Gracza")]
    public Transform player;
    public SourceMovement playerMovement;

    [Header("Maski Warstw")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("Ustawienia Chodów i Poruszania")]
    [Tooltip("Prędkość spokojnego stępu podczas patrolowania/pasienia się.")]
    public float walkSpeed = 2.5f;
    [Tooltip("Prędkość galopu podczas ucieczki/spłoszenia.")]
    public float fleeSpeed = 8.5f;
    [Tooltip("Promień szukania punktów patrolowania.")]
    public float walkPointRange = 15f;
    [Tooltip("Dystans, przy którym koń zaczyna uważać na gracza.")]
    public float cautiousDistance = 4.0f;


    [Header("Behavioral Settings")]
    [Tooltip("Wyskokość w hierarchi koni od 1 do 10 generowana na starcie gry randomowo")]
    public int hierarchyLevel;
    public bool isFolowing;
    public bool isLeader;
    public HorseAi horseLeader;
    public float distanceToLeader = 0;

    public List<HorseAi> otherHorses;
    public bool isLoaner;


    [Header("Fizyka Kopnięcia")]
    public float localKickForce = 10f;
    public float localUpKickForce = 5f;

    [Header("Kompatybilność Wsteczna / Pola Prefabów")]
    public float gunSightDistance;
    public int damage;
    public float timeBetweenAtacks;
    public float sightRange;
    public float attackRange;
    public bool playerInSightRange;
    public bool playerInAttackRange;
    public bool mounted = false;

    [Header ("Walk Point")]
    public Vector3 walkPoint;

    // Zmienne wewnętrzne
    private HorseGroupManager horseGroupManager;
    private Rigidbody rb;
    private Horse horse;
    
    private bool lookForWater = false;
    private bool walkPointSet = false;
    private bool isEating = false;
    private bool isFleeing = false;
    private bool kicked = false;

    private float stuckTimer = 0f;
    private float waypointTimeout = 0f;
    private NavMeshPath reusablePath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        navMeshAgent = agent;
        rb = GetComponent<Rigidbody>();
        breakableScript = GetComponent<Breakeable>();
        horse = GetComponent<Horse>();
        
        horseGroupManager = GetComponentInParent<HorseGroupManager>();
        otherHorses = horseGroupManager.horsesInGroup;
        otherHorses.Remove(this);

        reusablePath = new NavMeshPath();

        if (horseAnimator == null)
        {
            horseAnimator = GetComponentInChildren<Animator>();
        }

        if (agent != null)
        {
            agent.speed = walkSpeed;
        }

        if (breakableScript != null)
        {
            breakableScript.enabled = false;
        }
        if(hierarchyLevel == 0) hierarchyLevel = UnityEngine.Random.Range(0,10);
        if(isLoaner == false)
        {
            if(isLeader) return;
            int i = Random.Range(0,20);
            if(i >= 16) isLoaner = true;
        }
        
    }

    private void Start()
    {
        TryGetPlayerReference();
    }

    private void TryGetPlayerReference()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerRef != null)
        {
            playerMovement = GameManager.Instance.PlayerRef;
            player = playerMovement.transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            TryGetPlayerReference();
        }

        // 1. Aktualizacja parametru animacji 'speed'
        UpdateAnimatorSpeed();

        // 2. Jeśli koń nie żyje lub jest ujeżdżany -> wyłącz poruszanie AI
        if (horse != null && horse.isDead)
        {
            if (agent != null && agent.enabled) agent.enabled = false;
            if (breakableScript != null) breakableScript.enabled = false;
            return;
        }

        if(horseLeader != this && isFolowing)
        {
            distanceToLeader = Vector3.Distance(horseLeader.gameObject.transform.position, gameObject.transform.position);
        }
        if (mounted)
        {
            if (agent != null && agent.enabled) agent.enabled = false;
            if (breakableScript != null) breakableScript.enabled = false;
            return;
        }

        // 3. Jeśli koń został kopnięty -> fizyka przejmuje kontrolę
        if (kicked)
        {
            return;
        }

        if (lookForWater)
        {
        
        }

        // 4. Standardowe zachowanie zwierzęcia na NavMeshu
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (isFleeing)
            {
                FleeingBehavior();
            }
            else
            {
                GrazingAndPatrolBehavior();
            }
        }

        
    }

    private void UpdateAnimatorSpeed()
    {
        if (horseAnimator == null) return;

        float currentSpeed = 0f;
        if (kicked && rb != null)
        {
            currentSpeed = rb.linearVelocity.magnitude;
        }
        else if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            currentSpeed = agent.velocity.magnitude;
        }

        horseAnimator.SetFloat("speed", currentSpeed);
    }

    /// <summary>
    /// Spokojne zachowanie: spacerowanie stępem i skubanie trawy.
    /// </summary>
    private void GrazingAndPatrolBehavior()
    {
        // Szansa na wyrwanie się z pod kontroli lidera
        int number = Random.Range(0,15);
        if(number >= 13)
        {
            isLeader = false;
            isFolowing = false;
        }
        // Sprawdź czy gracz nie podchodzi zbyt blisko (reakcja czujności)
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer < cautiousDistance)
            {
                AvoidPlayer(distToPlayer);
                return;
            }
        }

        if (isEating) return;

        if(distanceToLeader > 4 )
        {
            walkPointSet = false;
            return;
        }
        if (!walkPointSet)
        {
            if(isFolowing)
            {
                SetWalkPointBasedOnLeader();
            }
            else
            {
                SearchWalkPoint();
            }
            
            return;
        }

        // Koń ma wyznaczony punkt i do niego idzie
        waypointTimeout += Time.deltaTime;


        // 1. Zabezpieczenie: jeśli ścieżka jest zablokowana lub stała się częściowa -> szukaj nowego punktu
        if (!agent.pathPending && (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid))
        {
            walkPointSet = false;
            return;
        }

        // 2. Watchdog: jeśli koń utknął na przeszkodzie (stoi w miejscu > 2.5s) -> zresetuj punkt
        if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.05f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2.5f)
            {
                walkPointSet = false;
                stuckTimer = 0f;
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        // 3. Maksymalny czas na dotarcie do jednego waypointa (14s)
        if (waypointTimeout > 14f)
        {
            walkPointSet = false;
            waypointTimeout = 0f;
            return;
        }

        // 4. Dotarcie do celu -> rozpoczęcie jedzenia trawy
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.6f)
        {
            Eat();
        }
    }

    private void AvoidPlayer(float distToPlayer)
    {
        if (isEating)
        {
            EatingFinished();
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        Vector3 awayFromPlayer = (transform.position - player.position).normalized;

        // Wypróbuj kilka kątów oddalenia od gracza i wybierz w 100% osiągalną ścieżkę
        for (int i = 0; i < 5; i++)
        {
            Vector3 testDir = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * awayFromPlayer;
            Vector3 targetPos = transform.position + testDir * Random.Range(5f, 9f);

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(navHit.position, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete)
                {
                    agent.speed = walkSpeed * 1.3f;
                    agent.SetDestination(navHit.position);
                    walkPoint = navHit.position;
                    walkPointSet = true;
                    waypointTimeout = 0f;
                    stuckTimer = 0f;
                    return;
                }
            }
        }
    }

    private void SearchWalkPoint()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        // Losujemy punkt w promieniu i sprawdzamy, czy agent FIZYCZNIE ma do niego pełną drogę
        // (eliminuje odcięte wyspy, dachy budynków czy miejsca za zamkniętym płotem)
        for (int i = 0; i < 8; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * walkPointRange;
            randomDir.y = 0;
            randomDir += transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit navHit, 6f, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(navHit.position, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = navHit.position;
                    walkPointSet = true;
                    waypointTimeout = 0f;
                    stuckTimer = 0f;

                    agent.speed = walkSpeed;
                    agent.SetDestination(walkPoint);
                    return;
                }
            }
        }
    }

    private void SearchWalkPoint(Vector3 leaderPosition)
    {
         if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        // Losujemy punkt w promieniu i sprawdzamy, czy agent FIZYCZNIE ma do niego pełną drogę
        // (eliminuje odcięte wyspy, dachy budynków czy miejsca za zamkniętym płotem)
        for (int i = 0; i < 8; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * walkPointRange/2;
            randomDir.y = 0;
            randomDir += leaderPosition;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit navHit, 6f, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(navHit.position, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = navHit.position;
                    walkPointSet = true;
                    waypointTimeout = 0f;
                    stuckTimer = 0f;

                    agent.speed = walkSpeed;
                    agent.SetDestination(walkPoint);
                    return;
                }
            }
        }
    }

    private void Eat()
    {
        isEating = true;
        if (horseAnimator != null) horseAnimator.SetBool("Eat", true);
        if (horse != null) horse.EatSound();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        // Czas jedzenia: 3 do 6 sekund
        Invoke(nameof(EatingFinished), Random.Range(3f, 6f));
    }

    public void EatingFinished()
    {
        isEating = false;
        if (horseAnimator != null) horseAnimator.SetBool("Eat", false);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        walkPointSet = false;
        waypointTimeout = 0f;
        stuckTimer = 0f;
    }

    /// <summary>
    /// Wywoływane, gdy koń zostanie zraniony lub spłoszony strzałem.
    /// </summary>
    public void OnTookDamage()
    {
        if (kicked || (horse != null && horse.isDead)) return;

        Vector3 dangerSource = player != null ? player.position : transform.position - transform.forward;
        SpookAndFlee(dangerSource);
    }

    public void SpookAndFlee(Vector3 dangerSource)
    {
        if (isEating) EatingFinished();
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (reusablePath == null) reusablePath = new NavMeshPath();

        Vector3 fleeDir = (transform.position - dangerSource).normalized;

        // Szukamy osiągalnego punktu ucieczki w bezpiecznym kierunku
        for (int i = 0; i < 6; i++)
        {
            Vector3 testDir = Quaternion.Euler(0, Random.Range(-50f, 50f), 0) * fleeDir;
            Vector3 targetPos = transform.position + testDir * Random.Range(16f, 26f);

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, 8f, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(navHit.position, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete)
                {
                    agent.speed = fleeSpeed;
                    agent.SetDestination(navHit.position);
                    isFleeing = true;
                    walkPointSet = false;

                    CancelInvoke(nameof(StopFleeing));
                    Invoke(nameof(StopFleeing), Random.Range(4f, 7f));
                    return;
                }
            }
        }
    }

    private void FleeingBehavior()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // Jeśli dotarł do bezpiecznego miejsca -> zakończ ucieczkę
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
            {
                StopFleeing();
            }
        }
    }

    private void StopFleeing()
    {
        isFleeing = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.speed = walkSpeed;
        }
        walkPointSet = false;
        waypointTimeout = 0f;
        stuckTimer = 0f;
    }
    
    public void LookForWater()
    {
        lookForWater = true;
        walkPoint = horseGroupManager.clostestWaterSource.position;
    }
    #region Horse Grouping

   
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Horse") && !isLoaner)
        {
            if(isFolowing == true) return;
            CompareHierarchy(other.GetComponent<HorseAi>());
        }
    }

    void CompareHierarchy(HorseAi horse)
    {
        if(horse == this) return;
        if(horse.hierarchyLevel > hierarchyLevel)
        {
            isLeader = false;
            isFolowing = true;
            horseLeader = horse;
            
        }
        else
        {
            isLeader = true;
            isFolowing = false;
            horseLeader = this;
        }
    }

    void SetWalkPointBasedOnLeader()
    {
        if(horseLeader.walkPoint != null)
        {
           SearchWalkPoint(horseLeader.walkPoint);
        }
    }

    public void MakeThisHorseLeader()
    {
        foreach(HorseAi horse in otherHorses )
        {
            if(horse == this) return;
            horse.isLeader = false;
        }
    }
    #endregion
    #region IKickeable & Fizyczny Pocisk

    public void KickHandle()
    {
        // Pusta implementacja dla zgodności z interfejsem
    }

    public bool kickHandle(Vector3 from, float kickForce)
    {
        if (horse != null && horse.isDead) return false;

        if (isEating) EatingFinished();

        kicked = true;
        isFleeing = false;

        // 1. Przełączenie z NavMesha na pełną fizykę Rigidbody
        if (agent != null) agent.enabled = false;
        if (breakableScript != null) breakableScript.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 flattened = Vector3.ProjectOnPlane(transform.position - from, Vector3.up).normalized;
            rb.AddForce(flattened * localKickForce, ForceMode.Impulse);
            rb.AddForce(Vector3.up * localUpKickForce, ForceMode.Impulse);
        }

        // Zadanie obrażeń od kopnięcia
        if (horse != null)
        {
            horse.Damaged(horse.kickDamage);
        }

        // Po 4 sekundach koń próbuje wstać i wrócić na NavMesh
        CancelInvoke(nameof(KickReset));
        Invoke(nameof(KickReset), 4f);

        return true;
    }

    public void KickReset()
    {
        if (horse != null && horse.isDead) return;

        // Szukamy najbliższego punktu na NavMeshu pod/obok konia
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 6f, NavMesh.AllAreas))
        {
            transform.position = navHit.position;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            if (agent != null)
            {
                agent.enabled = true;
                agent.Warp(navHit.position);
            }

            // Po kopnięciu koń ucieka w panice!
            if (player != null)
            {
                SpookAndFlee(player.position);
            }
        }
        else
        {
            // Jeśli nie znaleziono NavMesha tuż obok, spróbuj ponownie za chwilę
            Invoke(nameof(KickReset), 1.5f);
            return;
        }

        if (breakableScript != null)
        {
            breakableScript.enabled = false;
        }

        kicked = false;
    }

    #endregion

    #region Zdarzenia Animacji (Kompatybilność)

    // Zachowane dla kompatybilności ze zdarzeniami animacji we wbudowanych klipach
    public void AnimationAttack()
    {
    }

    #endregion
}

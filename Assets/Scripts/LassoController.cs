using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LassoTargetType
{
    None,
    GrapplePoint,
    NormalEnemy,
    HeavyEnemy,
    Pickup,
    Prop
}

public class LassoController : MonoBehaviour
{
    [Header("Referencje")]
    public SourceMovement playerMovement;
    public Camera playerCamera;
    public LineRenderer lineRenderer;
    public Transform firePoint;

    [Header("Zasięg i namierzanie")]
    public float maxRange = 25f;
    public float radius = 1.8f;
    public float cooldown = 0.8f;
    public LayerMask hitLayers = ~0;
    public LayerMask obstacleLayers;

    [Header("Wizualia liny")]
    public float throwSpeed = 55f;
    [Tooltip("Liczba segmentów trzonu liny")]
    public int ropeResolution = 16;
    [Tooltip("Liczba segmentów okręgu pętli lassa")]
    public int loopResolution = 12;
    [Tooltip("Ugięcie liny w dół podczas lotu")]
    public float ropeSag = 0.8f;
    [Tooltip("Promień pętli lassa w locie")]
    public float flightLoopRadius = 0.60f;
    [Tooltip("Prędkość zaciskania pętli na celu")]
    public float cinchSpeed = 15f;
    [Tooltip("Prędkość obrotu pętli w locie")]
    public float loopSpinSpeed = 12f;

    [Header("Dynamika fali w locie (Spiral Wave)")]
    [Tooltip("Amplituda bocznych spiralnych fal liny w locie")]
    public float spiralAmplitude = 0.30f;
    [Tooltip("Częstotliwość fali spiralnej")]
    public float spiralFrequency = 2.5f;
    [Tooltip("Prędkość przemieszczania się fali wzdłuż liny")]
    public float spiralSpeed = 14f;

    [Header("Drżenie napięcia (Tension Twang)")]
    [Tooltip("Siła drżenia po uderzeniu w cel")]
    public float tensionVibrationStrength = 0.18f;
    [Tooltip("Częstotliwość drżenia napiętej liny (Hz)")]
    public float tensionFrequency = 28f;
    [Tooltip("Czas trwania wygasania drżenia")]
    public float tensionDuration = 0.35f;

    [Header("Stylistyka i kolory")]
    public Material ropeMaterial;
    public bool autoConfigureLineRenderer = true;
    public Color ropeStartColor = new Color(0.88f, 0.76f, 0.58f, 1f); // Ciepły piaskowy beż (konopie)
    public Color ropeEndColor = new Color(0.50f, 0.35f, 0.20f, 1f);   // Ciemniejszy rzemień / węzeł
    public float baseRopeWidth = 0.070f;
    public float knotWidthMultiplier = 1.5f;

    [Header("Efekty trafienia (VFX)")]
    public GameObject hitVFXPrefab;

    [Header("Ustawienia przyciągania")]
    public float playerPullSpeed = 22f;
    public float enemyPullSpeed = 20f;
    public float pickupPullSpeed = 20f;
    public float propPullSpeed = 18f;
    public float enemyStunDuration = 1.5f;

    [Header("Dźwięki (Opcjonalne)")]
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip pullSound;
    public AudioClip releaseSound;

    [Header("Interfejs celownika")]
    public bool showTargetIndicator = true;

    private bool isOnCooldown = false;
    private bool isBusy = false;
    private GameObject currentHoveredTarget;
    private float currentLoopRadius = 0.2f;
    private float tensionTimer = 999f;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<SourceMovement>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // Zabezpieczenie hitLayers: wykluczamy gracza, ignore raycast, UI oraz postprocesy
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        int playerLayer = LayerMask.NameToLayer("Player");
        int uiLayer = LayerMask.NameToLayer("UI");
        int postProcessLayer = LayerMask.NameToLayer("PostProcess");
        int volumeLayer = LayerMask.NameToLayer("Volume");

        if (hitLayers.value == 0 || hitLayers.value == 64)
        {
            hitLayers = ~0;
        }

        if (playerLayer != -1) hitLayers &= ~(1 << playerLayer);
        if (ignoreRaycast != -1) hitLayers &= ~(1 << ignoreRaycast);
        if (uiLayer != -1) hitLayers &= ~(1 << uiLayer);
        if (postProcessLayer != -1) hitLayers &= ~(1 << postProcessLayer);
        if (volumeLayer != -1) hitLayers &= ~(1 << volumeLayer);

        if (obstacleLayers.value == 0)
        {
            obstacleLayers = LayerMask.GetMask("Default", "Ground");
            if (obstacleLayers.value == 0) obstacleLayers = 1; // Default
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            SetupLineRendererStyling();
        }
    }

    private void Update()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }

        // Aktualizacja namierzanego celu dla celownika
        if (!isBusy && !isOnCooldown)
        {
            currentHoveredTarget = FindBestTarget();
        }
        else
        {
            currentHoveredTarget = null;
        }

        // Klawisz F lub zdefiniowany przycisk "Lasso"
        bool inputTriggered = false;
        try
        {
            inputTriggered = Input.GetButtonDown("Lasso");
        }
        catch
        {
            inputTriggered = Input.GetKeyDown(KeyCode.F);
        }

        if (!inputTriggered && Input.GetKeyDown(KeyCode.F))
        {
            inputTriggered = true;
        }

        if (inputTriggered && !isOnCooldown && !isBusy)
        {
            TryUseLasso();
        }
    }

    private void TryUseLasso()
    {
        GameObject bestTarget = FindBestTarget();
        Vector3 destination;

        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 camFwd = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        if (bestTarget != null)
        {
            destination = GetTargetCenter(bestTarget);
        }
        else
        {
            // Jeśli nie ma celu, sprawdzamy gdzie trafia promień (ściana lub powietrze)
            if (Physics.Raycast(camPos, camFwd, out RaycastHit hit, maxRange, hitLayers))
            {
                destination = hit.point;
            }
            else
            {
                destination = camPos + camFwd * maxRange;
            }
        }

        isOnCooldown = true;
        isBusy = true;

        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, GetFirePointPosition());
        }

        StartCoroutine(SimulateLassoThrow(destination, bestTarget));
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }

    // --- FAZA LOTU LASSA ---
    private IEnumerator SimulateLassoThrow(Vector3 targetPos, GameObject targetObject)
    {
        if (lineRenderer == null) yield break;

        lineRenderer.enabled = true;
        currentLoopRadius = 0.2f;
        tensionTimer = 999f;

        Vector3 startPos = GetFirePointPosition();
        float distance = Vector3.Distance(startPos, targetPos);
        float flightDuration = Mathf.Max(0.08f, distance / throwSpeed);
        float elapsedTime = 0f;

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / flightDuration);

            DrawFlyingLasso(targetPos, t);
            yield return null;
        }

        // Lina dociera do celu – inicjujemy drżenie napięcia i zaciśnięcie
        tensionTimer = 0f;
        currentLoopRadius = flightLoopRadius;
        Vector3 hitNormal = (startPos - targetPos).normalized;

        if (targetObject != null)
        {
            LassoTargetType targetType = GetLassoTargetType(targetObject, out EnemyCore enemyCore, out Rigidbody propRb);

            if (hitSound != null && targetType != LassoTargetType.None)
            {
                AudioSource.PlayClipAtPoint(hitSound, targetPos);
            }

            SpawnHitVFX(targetPos, hitNormal);

            switch (targetType)
            {
                case LassoTargetType.GrapplePoint:
                    yield return StartCoroutine(PullPlayerToTarget(targetPos, LassoTargetType.GrapplePoint, targetObject.transform));
                    break;

                case LassoTargetType.HeavyEnemy:
                    // Ciężki wróg / Boss: gracz przyciąga się do wroga (Meat Hook)
                    Vector3 heavyCenter = GetTargetCenter(enemyCore != null ? enemyCore.gameObject : targetObject);
                    Transform heavyTrans = enemyCore != null ? enemyCore.transform : targetObject.transform;
                    yield return StartCoroutine(PullPlayerToTarget(heavyCenter, LassoTargetType.HeavyEnemy, heavyTrans));
                    break;

                case LassoTargetType.NormalEnemy:
                    // Zwykły wróg: wróg przyciągany do gracza i ogłuszany
                    yield return StartCoroutine(PullEnemyToPlayer(targetObject, enemyCore));
                    break;

                case LassoTargetType.Pickup:
                    yield return StartCoroutine(PullPickupToPlayer(targetObject));
                    break;

                case LassoTargetType.Prop:
                    yield return StartCoroutine(PullPropToPlayer(targetObject, propRb));
                    break;

                case LassoTargetType.None:
                default:
                    // Trafienie w zwykłą ścianę / teren - brak przyciągania, szybkie zwinięcie ze zderzeniem
                    if (hitSound != null)
                    {
                        AudioSource.PlayClipAtPoint(hitSound, targetPos);
                    }
                    float wallTimer = 0f;
                    while (wallTimer < 0.12f)
                    {
                        wallTimer += Time.deltaTime;
                        DrawAttachedLasso(targetPos, LassoTargetType.None, null);
                        yield return null;
                    }
                    break;
            }
        }
        else
        {
            // Pudło (brak namierzonego celu) – płynne zwinięcie liny z powietrza
            float retractTimer = 0f;
            float retractDuration = 0.14f;
            while (retractTimer < retractDuration)
            {
                retractTimer += Time.deltaTime;
                float retT = 1f - (retractTimer / retractDuration);
                DrawFlyingLasso(targetPos, retT);
                yield return null;
            }
        }

        lineRenderer.enabled = false;
        isBusy = false;
        Invoke(nameof(ResetCooldown), cooldown);
    }

    // --- LOGIKA PRZYCIĄGANIA GRACZA DO PUNKTU / CIĘŻKIEGO WROGA ---
    private IEnumerator PullPlayerToTarget(Vector3 targetPos, LassoTargetType targetType = LassoTargetType.GrapplePoint, Transform targetTransform = null)
    {
        if (pullSound != null) AudioSource.PlayClipAtPoint(pullSound, transform.position);

        float timer = 0f;
        float maxDuration = 1.4f;
        bool slingshotUsed = false;
        Vector3 lastPullDir = (targetPos - transform.position).normalized;

        while (timer < maxDuration)
        {
            Vector3 currentTargetPos = targetTransform != null ? GetTargetCenter(targetTransform.gameObject) : targetPos;
            float dist = Vector3.Distance(transform.position, currentTargetPos);
            if (dist <= 2.2f) break;

            timer += Time.deltaTime;
            DrawAttachedLasso(currentTargetPos, targetType, targetTransform);

            lastPullDir = (currentTargetPos - transform.position).normalized;

            // SLINGSHOT: Wciśnięcie Skoku (Spacja) podczas przyciągania daje kontrolowany wyskok
            if (Input.GetButtonDown("Jump"))
            {
                if (releaseSound != null) AudioSource.PlayClipAtPoint(releaseSound, transform.position);
                if (playerMovement != null)
                {
                    Vector3 slingshotDir = (lastPullDir + Vector3.up * 0.35f).normalized;
                    playerMovement.SetVelocity(slingshotDir * (playerPullSpeed * 0.85f));
                }
                slingshotUsed = true;
                break;
            }

            if (playerMovement != null)
            {
                // Stała, stabilna prędkość bez kumulowania szalonej siły
                playerMovement.SetVelocity(lastPullDir * playerPullSpeed);
            }

            yield return null;
        }

        // Łagodne lądowanie na końcu przyciągania (jeśli nie użyto slingshota)
        if (!slingshotUsed && playerMovement != null)
        {
            // Delikatne wyhamowanie z lekkim uniesieniem, aby nie wypaść poza mapę
            playerMovement.SetVelocity(lastPullDir * 4f + Vector3.up * 2f);
        }
    }

    // --- LOGIKA PRZYCIĄGANIA WROGA DO GRACZA ---
    private IEnumerator PullEnemyToPlayer(GameObject hitObj, EnemyCore enemyCore)
    {
        if (hitObj == null) yield break;

        // Ustalenie głównego obiektu wroga (root / EnemyCore)
        Transform enemyTransform = null;
        if (enemyCore != null)
        {
            enemyTransform = enemyCore.transform;
        }
        else if (hitObj.GetComponentInParent<EnemyLogic>() != null)
        {
            enemyTransform = hitObj.GetComponentInParent<EnemyLogic>().transform;
        }
        else
        {
            enemyTransform = hitObj.transform.root;
        }

        if (enemyTransform == null) yield break;

        if (pullSound != null) AudioSource.PlayClipAtPoint(pullSound, transform.position);

        // Pobranie Rigidbody wroga
        Rigidbody enemyRb = null;
        if (enemyCore != null)
        {
            if (enemyCore.moveBrain != null)
            {
                enemyRb = enemyCore.moveBrain.GetMainTransform().GetComponent<Rigidbody>();
            }
            if (enemyRb == null)
            {
                enemyRb = enemyCore.GetComponent<Rigidbody>() ?? enemyCore.GetComponentInChildren<Rigidbody>();
            }
        }
        if (enemyRb == null)
        {
            enemyRb = enemyTransform.GetComponent<Rigidbody>() ?? hitObj.GetComponent<Rigidbody>();
        }

        // Wyłączenie logiki ruchu AI podczas przyciągania, aby nie kontrowała pozycji gracza
        bool moveBrainWasEnabled = false;
        if (enemyCore != null && enemyCore.moveBrain != null)
        {
            moveBrainWasEnabled = enemyCore.moveBrain.enabled;
            enemyCore.moveBrain.enabled = false;
        }

        // Ogłuszenie i unieruchomienie wroga
        if (enemyCore != null)
        {
            enemyCore.SetStunned(true);
            enemyCore.SetIncapacitated(true);
        }

        bool wasKinematic = enemyRb != null ? enemyRb.isKinematic : false;
        bool wasGravity = enemyRb != null ? enemyRb.useGravity : true;
        if (enemyRb != null)
        {
            enemyRb.isKinematic = true;
            enemyRb.useGravity = false;
        }

        float timer = 0f;
        float maxDuration = 1.0f;

        while (timer < maxDuration && enemyTransform != null)
        {
            timer += Time.deltaTime;

            // Pozycja zatrzymania: 1.8m przed graczem na poziomie podłogi gracza
            Vector3 playerFloorPos = transform.position;
            Vector3 stopPoint = playerFloorPos + transform.forward * 1.8f;
            stopPoint.y = playerFloorPos.y;

            float dist = Vector3.Distance(enemyTransform.position, stopPoint);
            if (dist <= 0.6f) break;

            Vector3 pullDir = (stopPoint - enemyTransform.position).normalized;
            float step = enemyPullSpeed * Time.deltaTime;

            // Sprawdzenie czy wróg nie uderzy w przeszkodę terenową
            if (Physics.SphereCast(enemyTransform.position + Vector3.up * 0.5f, 0.35f, pullDir, out RaycastHit obsHit, step, obstacleLayers))
            {
                if (obsHit.transform != enemyTransform &&
                    !obsHit.transform.IsChildOf(enemyTransform) &&
                    !enemyTransform.IsChildOf(obsHit.transform))
                {
                    break;
                }
            }

            Vector3 newPos = Vector3.MoveTowards(enemyTransform.position, stopPoint, step);
            enemyTransform.position = newPos;
            if (enemyRb != null)
            {
                enemyRb.position = newPos;
            }

            Vector3 ropeTarget = GetTargetCenter(enemyTransform.gameObject);
            DrawAttachedLasso(ropeTarget, LassoTargetType.NormalEnemy, enemyTransform);

            yield return null;
        }

        // Przywrócenie fizyki i AI wroga
        if (enemyTransform != null)
        {
            if (enemyRb != null)
            {
                enemyRb.isKinematic = wasKinematic;
                enemyRb.useGravity = wasGravity;
                enemyRb.linearVelocity = Vector3.zero;
                enemyRb.angularVelocity = Vector3.zero;
            }

            if (enemyCore != null && enemyCore.moveBrain != null && moveBrainWasEnabled)
            {
                enemyCore.moveBrain.enabled = true;
            }

            // Przedłużenie ogłuszenia, aby gracz mógł zadać cios (np. kopniak lub strzał)
            if (enemyCore != null)
            {
                StartCoroutine(UnstunAfterDelay(enemyCore, enemyStunDuration));
            }
        }
    }

    private IEnumerator UnstunAfterDelay(EnemyCore core, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (core != null && !core.dead)
        {
            core.SetStunned(false);
            core.SetIncapacitated(false);
        }
    }

    // --- LOGIKA PRZYCIĄGANIA PICKUPÓW ---
    private IEnumerator PullPickupToPlayer(GameObject pickup)
    {
        if (pickup == null) yield break;

        Rigidbody rb = pickup.GetComponent<Rigidbody>() ?? pickup.GetComponentInParent<Rigidbody>();
        bool wasGravity = rb != null ? rb.useGravity : true;
        if (rb != null) rb.useGravity = false;

        float timer = 0f;
        while (timer < 1.0f && pickup != null)
        {
            timer += Time.deltaTime;
            Vector3 targetHandPos = GetFirePointPosition();
            float dist = Vector3.Distance(pickup.transform.position, targetHandPos);

            if (dist <= 1.2f) break;

            Vector3 dir = (targetHandPos - pickup.transform.position).normalized;

            if (rb != null)
            {
                rb.linearVelocity = dir * pickupPullSpeed;
            }
            else
            {
                pickup.transform.position = Vector3.MoveTowards(pickup.transform.position, targetHandPos, pickupPullSpeed * Time.deltaTime);
            }

            DrawAttachedLasso(pickup.transform.position, LassoTargetType.Pickup, pickup.transform);
            yield return null;
        }

        if (pickup != null && rb != null)
        {
            rb.useGravity = wasGravity;
            rb.linearVelocity = Vector3.zero;
        }
    }

    // --- LOGIKA PRZYCIĄGANIA FIZYCZNYCH REKWIZYTÓW (np. Beczki wybuchowe) ---
    private IEnumerator PullPropToPlayer(GameObject prop, Rigidbody rb)
    {
        if (prop == null || rb == null) yield break;

        bool wasGravity = rb.useGravity;
        rb.useGravity = false;

        float timer = 0f;
        while (timer < 1.2f && prop != null)
        {
            timer += Time.deltaTime;
            Vector3 stopPoint = transform.position + transform.forward * 2.0f + Vector3.up * 0.3f;
            float dist = Vector3.Distance(prop.transform.position, stopPoint);

            if (dist <= 1.0f) break;

            Vector3 dir = (stopPoint - prop.transform.position).normalized;
            rb.linearVelocity = dir * propPullSpeed;

            DrawAttachedLasso(prop.transform.position, LassoTargetType.Prop, prop.transform);
            yield return null;
        }

        if (prop != null && rb != null)
        {
            rb.useGravity = wasGravity;
            rb.linearVelocity = Vector3.zero;
        }
    }

    // --- IDENTYFIKACJA TYPU CELU ---
    public LassoTargetType GetLassoTargetType(GameObject obj, out EnemyCore enemyCore, out Rigidbody propRb)
    {
        enemyCore = null;
        propRb = null;

        if (obj == null) return LassoTargetType.None;

        // 1. Punkt zaczepienia (GrapplePoint)
        if (obj.CompareTag("GrapplePoint") ||
            obj.name.ToLowerInvariant().Contains("grapple") ||
            obj.name.ToLowerInvariant().Contains("hookpoint"))
        {
            return LassoTargetType.GrapplePoint;
        }

        // 2. Przeciwnik (EnemyCore / EnemyLogic / Tag / Layer)
        if (IsEnemy(obj, out enemyCore))
        {
            if (enemyCore != null && enemyCore.dead)
            {
                return LassoTargetType.None; // Nie celujemy w martwych wrogów
            }

            if (IsHeavyEnemy(obj, enemyCore))
                return LassoTargetType.HeavyEnemy;

            return LassoTargetType.NormalEnemy;
        }

        // 3. Przedmioty do podniesienia (Pickup)
        if (obj.CompareTag("Pickup") ||
            obj.GetComponentInParent<pickUp>() != null ||
            obj.GetComponentInChildren<pickUp>() != null)
        {
            return LassoTargetType.Pickup;
        }

        // 4. Dynamiczne rekwizyty fizyczne (beczki, skrzynie)
        propRb = obj.GetComponentInParent<Rigidbody>();
        if (propRb != null && !propRb.isKinematic && propRb.gameObject != gameObject && !propRb.transform.IsChildOf(transform))
        {
            if (!propRb.CompareTag("Player") && !propRb.CompareTag("Enemy"))
            {
                return LassoTargetType.Prop;
            }
        }

        // 5. Statyczna geometria świata (ściany, podłoga, skały) - nie jest celem lassa!
        return LassoTargetType.None;
    }

    // --- WYSZUKIWANIE CELU (SMART TARGETING) ---
    private GameObject FindBestTarget()
    {
        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 camFwd = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(camPos, radius, camFwd, maxRange, hitLayers);

        List<RaycastHit> candidates = new List<RaycastHit>();

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue;
            if (hit.collider.transform.IsChildOf(transform)) continue;

            // Akceptujemy TYLKO prawidłowe cele (wróg, grapple point, pickup, rekwizyt)
            LassoTargetType targetType = GetLassoTargetType(hit.collider.gameObject, out _, out _);
            if (targetType == LassoTargetType.None) continue;

            Vector3 center = GetTargetCenter(hit.collider.gameObject);
            if (!IsLineOfSightClear(hit.collider.transform, center)) continue;

            candidates.Add(hit);
        }

        if (candidates.Count == 0) return null;

        return GetClosestToCrosshair(candidates);
    }

    private bool IsLineOfSightClear(Transform target, Vector3 targetCenter)
    {
        Vector3 origin = playerCamera != null ? playerCamera.transform.position : (transform.position + Vector3.up * 1.6f);
        Vector3 dir = targetCenter - origin;
        float dist = dir.magnitude;

        if (dist <= 0.3f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist - 0.2f, obstacleLayers))
        {
            if (hit.transform != target &&
                !hit.transform.IsChildOf(target) &&
                !target.IsChildOf(hit.transform))
            {
                return false;
            }
        }
        return true;
    }

    private GameObject GetClosestToCrosshair(List<RaycastHit> hits)
    {
        GameObject bestObj = null;
        float minAngle = float.MaxValue;
        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 camFwd = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        foreach (var hit in hits)
        {
            Vector3 center = GetTargetCenter(hit.collider.gameObject);
            Vector3 dirToTarget = (center - camPos).normalized;
            float angle = Vector3.Angle(camFwd, dirToTarget);

            if (angle < minAngle)
            {
                minAngle = angle;
                bestObj = hit.collider.gameObject;
            }
        }

        return bestObj;
    }

    private bool IsEnemy(GameObject obj, out EnemyCore core)
    {
        core = obj.GetComponentInParent<EnemyCore>();
        if (core == null)
            core = obj.GetComponentInChildren<EnemyCore>();

        if (core != null) return true;

        if (obj.CompareTag("Enemy") || obj.CompareTag("EnemyGrapple"))
            return true;

        if (obj.GetComponentInParent<EnemyLogic>() != null || obj.GetComponentInChildren<EnemyLogic>() != null)
            return true;

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int hurtBoxLayer = LayerMask.NameToLayer("HurtBox");
        if ((enemyLayer != -1 && obj.layer == enemyLayer) || (hurtBoxLayer != -1 && obj.layer == hurtBoxLayer))
            return true;

        return false;
    }

    private bool IsHeavyEnemy(GameObject obj, EnemyCore core)
    {
        string lowerName = obj.name.ToLowerInvariant();
        if (core != null)
        {
            lowerName += " " + core.gameObject.name.ToLowerInvariant();
        }

        if (lowerName.Contains("fat") || lowerName.Contains("boss") || lowerName.Contains("heavy"))
            return true;

        if (core != null && core.dmgMannager != null && core.dmgMannager.EnemyHp >= 150f)
            return true;

        return false;
    }

    private Vector3 GetTargetCenter(GameObject obj)
    {
        if (obj == null) return transform.position;

        if (obj.TryGetComponent<Collider>(out var col))
            return col.bounds.center;

        var colInParent = obj.GetComponentInParent<Collider>();
        if (colInParent != null)
            return colInParent.bounds.center;

        return obj.transform.position + Vector3.up * 0.9f;
    }

    public Vector3 GetFirePointPosition()
    {
        if (firePoint != null)
            return firePoint.position;

        if (playerCamera != null)
        {
            return playerCamera.transform.position +
                   playerCamera.transform.forward * 0.4f +
                   playerCamera.transform.right * 0.35f -
                   playerCamera.transform.up * 0.25f;
        }

        return transform.position + Vector3.up * 1.2f;
    }

    // --- RYSOWANIE LINY ---
    private void DrawStraightRope(Vector3 endPos)
    {
        if (lineRenderer == null) return;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, GetFirePointPosition());
        lineRenderer.SetPosition(1, endPos);
    }

    private void DrawBezierCurve(Vector3 endPos, float sag)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = ropeResolution;
        Vector3 p0 = GetFirePointPosition();
        Vector3 p2 = endPos;
        Vector3 p1 = (p0 + p2) / 2f;
        p1.y -= sag;

        for (int i = 0; i < ropeResolution; i++)
        {
            float t = i / (float)(ropeResolution - 1);
            Vector3 pos = CalculateQuadraticBezierPoint(t, p0, p1, p2);
            lineRenderer.SetPosition(i, pos);
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }

    // --- PROCEDURALNA DYNAMIKA LASSA (PĘTLA, SPIRALA, NAPIĘCIE) ---

    private void DrawFlyingLasso(Vector3 targetPos, float t)
    {
        if (lineRenderer == null) return;

        int stemCount = Mathf.Max(4, ropeResolution);
        int loopCount = Mathf.Max(6, loopResolution);
        int totalPoints = stemCount + loopCount;

        if (lineRenderer.positionCount != totalPoints)
        {
            lineRenderer.positionCount = totalPoints;
        }

        Vector3 startPos = GetFirePointPosition();
        Vector3 headPos = Vector3.Lerp(startPos, targetPos, t);
        Vector3 throwDir = (targetPos - startPos);
        Vector3 fwd = throwDir.sqrMagnitude > 0.001f ? throwDir.normalized : (playerCamera != null ? playerCamera.transform.forward : transform.forward);

        Vector3 right = Vector3.Cross(fwd, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.Cross(fwd, Vector3.right);
        }
        right.Normalize();
        Vector3 up = Vector3.Cross(right, fwd).normalized;

        // Pętla rozszerza się w miarę wylotu z dłoni
        float currentRadius = Mathf.Lerp(0.2f, flightLoopRadius, Mathf.Clamp01(t * 3.5f));

        // Obrót pętli wokół osi lotu
        float spin = Time.time * loopSpinSpeed;
        Vector3 loopU = (right * Mathf.Cos(spin) + up * Mathf.Sin(spin)).normalized;
        Vector3 loopV = Vector3.Cross(fwd, loopU).normalized;

        // Węzeł na obwodzie pętli
        Vector3 knotPos = headPos + loopU * currentRadius;

        // 1. Trzon liny (od dłoni do węzła)
        for (int i = 0; i < stemCount; i++)
        {
            float u = i / (float)(stemCount - 1);
            Vector3 basePt = Vector3.Lerp(startPos, knotPos, u);

            // Tłumione ugięcie grawitacyjne
            float sag = ropeSag * Mathf.Sin(u * Mathf.PI) * (1f - t * 0.65f);
            Vector3 sagOffset = Vector3.down * sag;

            // Spiralna fala świstu w locie
            float wavePhase = u * spiralFrequency * Mathf.PI * 2f - Time.time * spiralSpeed;
            float envelope = Mathf.Sin(u * Mathf.PI);
            float amp = spiralAmplitude * envelope * (1f - t * 0.45f);
            Vector3 spiralOffset = (right * Mathf.Cos(wavePhase) + up * Mathf.Sin(wavePhase)) * amp;

            lineRenderer.SetPosition(i, basePt + sagOffset + spiralOffset);
        }

        // 2. Wirująca pętla lassa (od węzła dookoła głowicy i powrót do węzła)
        for (int j = 1; j <= loopCount; j++)
        {
            float angle = (j / (float)loopCount) * Mathf.PI * 2f;
            Vector3 loopPt = headPos + (loopU * Mathf.Cos(angle) + loopV * Mathf.Sin(angle)) * currentRadius;
            lineRenderer.SetPosition(stemCount - 1 + j, loopPt);
        }
    }

    private void DrawAttachedLasso(Vector3 targetCenter, LassoTargetType targetType, Transform targetTransform = null)
    {
        if (lineRenderer == null) return;

        int stemCount = Mathf.Max(4, ropeResolution);
        int loopCount = Mathf.Max(6, loopResolution);
        int totalPoints = stemCount + loopCount;

        if (lineRenderer.positionCount != totalPoints)
        {
            lineRenderer.positionCount = totalPoints;
        }

        Vector3 startPos = GetFirePointPosition();

        // Wyznaczenie orientacji płaszczyzny pętli
        Vector3 loopNormal = Vector3.up;
        if (targetTransform != null && (targetType == LassoTargetType.NormalEnemy || targetType == LassoTargetType.HeavyEnemy))
        {
            loopNormal = targetTransform.up;
        }
        else if (targetType == LassoTargetType.GrapplePoint)
        {
            loopNormal = (targetCenter - startPos).normalized;
        }

        // Wektor ku graczowi rzutowany na płaszczyznę pętli (węzeł lassa zawsze skierowany w stronę gracza)
        Vector3 toPlayer = (startPos - targetCenter);
        Vector3 loopU = Vector3.ProjectOnPlane(toPlayer, loopNormal).normalized;
        if (loopU.sqrMagnitude < 0.001f)
        {
            loopU = Vector3.ProjectOnPlane(Vector3.forward, loopNormal).normalized;
            if (loopU.sqrMagnitude < 0.001f) loopU = Vector3.right;
        }
        Vector3 loopV = Vector3.Cross(loopNormal, loopU).normalized;

        // Docelowy promień zaciśnięcia w zależności od typu celu
        float targetCinchRadius = 0.32f;
        switch (targetType)
        {
            case LassoTargetType.GrapplePoint: targetCinchRadius = 0.18f; break;
            case LassoTargetType.HeavyEnemy:  targetCinchRadius = 0.52f; break;
            case LassoTargetType.NormalEnemy: targetCinchRadius = 0.30f; break;
            case LassoTargetType.Pickup:      targetCinchRadius = 0.12f; break;
            case LassoTargetType.Prop:        targetCinchRadius = 0.28f; break;
            default:                          targetCinchRadius = 0.15f; break;
        }

        currentLoopRadius = Mathf.MoveTowards(currentLoopRadius, targetCinchRadius, cinchSpeed * Time.deltaTime);

        Vector3 knotPos = targetCenter + loopU * currentLoopRadius;

        // Obliczanie drżenia napięcia (Tension Twang)
        tensionTimer += Time.deltaTime;
        float tensionDamping = Mathf.Exp(-tensionTimer * (4f / Mathf.Max(0.01f, tensionDuration)));
        float vibration = Mathf.Sin(tensionTimer * tensionFrequency * Mathf.PI * 2f) * tensionVibrationStrength * tensionDamping;

        Vector3 ropeDir = (knotPos - startPos).normalized;
        Vector3 vibeAxis = Vector3.Cross(ropeDir, Vector3.up).normalized;
        if (vibeAxis.sqrMagnitude < 0.001f) vibeAxis = Vector3.right;

        // 1. Trzon liny z harmonicznym drżeniem napięcia
        for (int i = 0; i < stemCount; i++)
        {
            float u = i / (float)(stemCount - 1);
            Vector3 basePt = Vector3.Lerp(startPos, knotPos, u);

            float envelope = Mathf.Sin(u * Mathf.PI);
            Vector3 vibeOffset = vibeAxis * (vibration * envelope);
            Vector3 sagOffset = Vector3.down * (0.04f * envelope);

            lineRenderer.SetPosition(i, basePt + vibeOffset + sagOffset);
        }

        // 2. Zaciśnięta pętla wokół celu
        for (int j = 1; j <= loopCount; j++)
        {
            float angle = (j / (float)loopCount) * Mathf.PI * 2f;
            Vector3 loopPt = targetCenter + (loopU * Mathf.Cos(angle) + loopV * Mathf.Sin(angle)) * currentLoopRadius;
            lineRenderer.SetPosition(stemCount - 1 + j, loopPt);
        }
    }

    private void SpawnHitVFX(Vector3 position, Vector3 normal)
    {
        if (hitVFXPrefab != null)
        {
            Quaternion rot = normal != Vector3.zero ? Quaternion.LookRotation(normal) : Quaternion.identity;
            GameObject vfx = Instantiate(hitVFXPrefab, position, rot);
            Destroy(vfx, 2.5f);
        }
    }

    private void SetupLineRendererStyling()
    {
        if (lineRenderer == null) return;

        lineRenderer.numCornerVertices = 6;
        lineRenderer.numCapVertices = 6;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;

        // Profil grubości: dłoń -> trzon -> węzeł -> pętla
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, baseRopeWidth * 1.15f));
        curve.AddKey(new Keyframe(0.60f, baseRopeWidth));
        curve.AddKey(new Keyframe(0.70f, baseRopeWidth * knotWidthMultiplier));
        curve.AddKey(new Keyframe(0.78f, baseRopeWidth * 0.95f));
        curve.AddKey(new Keyframe(1f, baseRopeWidth * 0.90f));
        lineRenderer.widthCurve = curve;
        lineRenderer.widthMultiplier = 1f;

        if (ropeMaterial != null)
        {
            lineRenderer.material = ropeMaterial;
        }
        else if (lineRenderer.sharedMaterial == null || 
                (lineRenderer.sharedMaterial.name != null && lineRenderer.sharedMaterial.name.IndexOf("Grape", System.StringComparison.OrdinalIgnoreCase) >= 0))
        {
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (unlitShader != null)
            {
                lineRenderer.material = new Material(unlitShader);
            }
        }

        if (autoConfigureLineRenderer)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(ropeStartColor, 0.0f),
                    new GradientColorKey(Color.Lerp(ropeStartColor, ropeEndColor, 0.45f), 0.65f),
                    new GradientColorKey(ropeEndColor, 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
            lineRenderer.colorGradient = gradient;
        }
    }

    // --- ZNACZNIK CELOWNIKA ---
    private void OnGUI()
    {
        if (!showTargetIndicator || currentHoveredTarget == null || isBusy || playerCamera == null)
            return;

        Vector3 targetCenter = GetTargetCenter(currentHoveredTarget);
        Vector3 screenPos = playerCamera.WorldToScreenPoint(targetCenter);

        if (screenPos.z <= 0) return;

        float guiY = Screen.height - screenPos.y;
        float size = 26f;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        LassoTargetType type = GetLassoTargetType(currentHoveredTarget, out _, out _);
        string labelText = "[F] Lasso";
        Color labelColor = Color.yellow;

        switch (type)
        {
            case LassoTargetType.GrapplePoint:
                labelText = "[F] Zaczep";
                labelColor = new Color(0.35f, 1f, 0.45f);
                break;
            case LassoTargetType.HeavyEnemy:
                labelText = "[F] Przyciągnij się";
                labelColor = new Color(1f, 0.45f, 0.2f);
                break;
            case LassoTargetType.NormalEnemy:
                labelText = "[F] Przyciągnij wroga";
                labelColor = new Color(1f, 0.85f, 0.2f);
                break;
            case LassoTargetType.Pickup:
                labelText = "[F] Weź przedmiot";
                labelColor = new Color(0.4f, 0.85f, 1f);
                break;
            case LassoTargetType.Prop:
                labelText = "[F] Przyciągnij";
                labelColor = new Color(0.95f, 0.95f, 0.5f);
                break;
        }

        style.normal.textColor = labelColor;

        GUI.Box(new Rect(screenPos.x - size / 2f, guiY - size / 2f, size, size), GUIContent.none);
        GUI.Label(new Rect(screenPos.x - 75f, guiY + 14f, 150f, 20f), labelText, style);
    }
}
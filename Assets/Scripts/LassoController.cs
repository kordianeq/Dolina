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
    public int ropeResolution = 16;
    public float ropeSag = 1.6f;

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
        lineRenderer.positionCount = ropeResolution;

        Vector3 startPos = GetFirePointPosition();
        float distance = Vector3.Distance(startPos, targetPos);
        float flightDuration = Mathf.Max(0.08f, distance / throwSpeed);
        float elapsedTime = 0f;

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / flightDuration);

            Vector3 currentEndPos = Vector3.Lerp(GetFirePointPosition(), targetPos, t);
            float currentSag = Mathf.Lerp(ropeSag, 0f, t);

            DrawBezierCurve(currentEndPos, currentSag);
            yield return null;
        }

        // Lina się napina po dotarciu do celu
        DrawStraightRope(targetPos);

        if (targetObject != null)
        {
            LassoTargetType targetType = GetLassoTargetType(targetObject, out EnemyCore enemyCore, out Rigidbody propRb);

            if (hitSound != null && targetType != LassoTargetType.None)
            {
                AudioSource.PlayClipAtPoint(hitSound, targetPos);
            }

            switch (targetType)
            {
                case LassoTargetType.GrapplePoint:
                    yield return StartCoroutine(PullPlayerToTarget(targetPos));
                    break;

                case LassoTargetType.HeavyEnemy:
                    // Ciężki wróg / Boss: gracz przyciąga się do wroga (Meat Hook)
                    Vector3 heavyCenter = GetTargetCenter(enemyCore != null ? enemyCore.gameObject : targetObject);
                    yield return StartCoroutine(PullPlayerToTarget(heavyCenter));
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
                    // Trafienie w zwykłą ścianę / teren - brak przyciągania, szybkie zwinięcie
                    if (hitSound != null)
                    {
                        AudioSource.PlayClipAtPoint(hitSound, targetPos);
                    }
                    yield return new WaitForSeconds(0.08f);
                    break;
            }
        }
        else
        {
            // Pudło (brak namierzonego celu) – szybkie zwinięcie liny
            yield return new WaitForSeconds(0.08f);
        }

        lineRenderer.enabled = false;
        isBusy = false;
        Invoke(nameof(ResetCooldown), cooldown);
    }

    // --- LOGIKA PRZYCIĄGANIA GRACZA DO PUNKTU / CIĘŻKIEGO WROGA ---
    private IEnumerator PullPlayerToTarget(Vector3 targetPos)
    {
        if (pullSound != null) AudioSource.PlayClipAtPoint(pullSound, transform.position);

        float timer = 0f;
        float maxDuration = 1.4f;
        bool slingshotUsed = false;
        Vector3 lastPullDir = (targetPos - transform.position).normalized;

        while (timer < maxDuration)
        {
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= 2.2f) break;

            timer += Time.deltaTime;
            DrawStraightRope(targetPos);

            lastPullDir = (targetPos - transform.position).normalized;

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
            DrawStraightRope(ropeTarget);

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

            DrawStraightRope(pickup.transform.position);
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

            DrawStraightRope(prop.transform.position);
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
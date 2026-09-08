using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 

public class LassoController : MonoBehaviour
{
    [Header("References")]
    public SourceMovement playerMovement;
    public Camera playerCamera;
    public LineRenderer lineRenderer; 
    public Transform firePoint;       

    [Header("Lasso Target Settings")]
    public float maxRange = 20f;      
    public float radius = 1.5f;       
    public float cooldown = 1f;        
    public LayerMask hitLayers;       
    public LayerMask obstacleLayers;  

    [Header("Lasso Rope Visuals")]
    public float throwSpeed = 40f;    // Prędkość lotu lassa
    public int ropeResolution = 15;   // Z ilu punktów składa się lina
    public float ropeSag = 2f;        // Zwis liny w trakcie lotu

    [Header("Pull Settings")]
    public float playerPullForce = 25f; 
    public float pickupPullSpeed = 15f; 

    private bool isOnCooldown = false;

    private void Awake()
    {
        playerMovement = GetComponent<SourceMovement>();
        playerCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Lasso") && !isOnCooldown)
        {
            isOnCooldown = true;
            TryUseLasso();
            Invoke(nameof(ResetCooldown), cooldown); 
        }
    }

    void ResetCooldown()
    {
        isOnCooldown = false;
    }

    private void TryUseLasso()
    {
        GameObject bestTarget = FindBestTarget();
        Vector3 endDestination;

        if (bestTarget != null)
            endDestination = bestTarget.transform.position;
        else
            endDestination = playerCamera.transform.position + playerCamera.transform.forward * maxRange;

        // Zamiast od razu przyciągać, uruchamiamy symulację lotu lassa
        StartCoroutine(SimulateLassoThrow(endDestination, bestTarget));
    }

    // --- ANIMACJA LOTU LASSA ---
    private IEnumerator SimulateLassoThrow(Vector3 targetPos, GameObject targetObject)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = ropeResolution;

        Vector3 startPos = firePoint.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float flightDuration = distance / throwSpeed;
        float elapsedTime = 0f;

        // Faza 1: Lot pocisku i zwis liny
        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flightDuration;

            // Obliczamy aktualną pozycję "końcówki" lassa w powietrzu
            Vector3 currentEndPos = Vector3.Lerp(firePoint.position, targetPos, t);
            
            // Rysujemy luźną linę
            DrawBezierCurve(currentEndPos, ropeSag);

            yield return null;
        }

        // Faza 2: Trafienie
        // Lina się napręża (0 zwisu)
        DrawBezierCurve(targetPos, 0f);

        if (targetObject != null)
        {
            if (targetObject.CompareTag("GrapplePoint"))
                StartCoroutine(PullPlayerToTarget(targetPos));
            else if (targetObject.CompareTag("Pickup"))
                StartCoroutine(PullPickupToPlayer(targetObject));
        }
        else
        {
            // Pudło - lasso znika po uderzeniu w pustkę
            lineRenderer.enabled = false;
        }
    }

    // --- RYSOWANIE KRZYWEJ BEZIERA ---
    private void DrawBezierCurve(Vector3 endPos, float currentSag)
    {
        Vector3 p0 = firePoint.position;
        Vector3 p2 = endPos;
        
        // Punkt kontrolny wygięcia liny w dół
        Vector3 p1 = (p0 + p2) / 2f;
        p1.y -= currentSag; 

        for (int i = 0; i < ropeResolution; i++)
        {
            float t = i / (float)(ropeResolution - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, p0, p1, p2);
            lineRenderer.SetPosition(i, position);
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; 
        p += 2 * u * t * p1; 
        p += tt * p2; 
        return p;
    }

    // --- SYSTEM CELOWANIA (SMART TARGETING) ---
    private GameObject FindBestTarget()
    {
        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.transform.position, radius, playerCamera.transform.forward, maxRange, hitLayers);
        List<RaycastHit> points = new List<RaycastHit>();
        List<RaycastHit> pickups = new List<RaycastHit>();

        foreach (var hit in hits)
        {
            if (!IsLineOfSightClear(hit.collider.transform)) continue;

            if (hit.collider.CompareTag("GrapplePoint")) points.Add(hit);
            else if (hit.collider.CompareTag("Pickup")) pickups.Add(hit);
            else if (hit.collider.CompareTag("EnemyGrapple")) return hit.collider.transform.parent.gameObject;
        }

        if (points.Count > 0) return GetClosestToCrosshair(points);
        if (pickups.Count > 0) return GetClosestToCrosshair(pickups);

        return null;
    }

    private bool IsLineOfSightClear(Transform target)
    {
        Vector3 direction = target.position - playerCamera.transform.position;
        float dist = direction.magnitude;
        if (Physics.Raycast(playerCamera.transform.position, direction, dist, obstacleLayers)) return false; 
        return true;
    }

    private GameObject GetClosestToCrosshair(List<RaycastHit> hits)
    {
        GameObject bestObj = null;
        float minAngle = float.MaxValue;

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, dirToTarget);

            if (angle < minAngle)
            {
                minAngle = angle;
                bestObj = hit.collider.gameObject;
            }
        }
        return bestObj;
    }

    // --- LOGIKA PRZYCIĄGANIA (AKCJE) ---
    private IEnumerator PullPlayerToTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        playerMovement.AddImpulse(direction * playerPullForce + Vector3.up * 2f);

        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            // Rysujemy naprężoną linę śledzącą ruch gracza (0 zwisu)
            DrawBezierCurve(targetPos, 0f); 
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    private IEnumerator PullPickupToPlayer(GameObject pickup)
    {
        Rigidbody rb = pickup.GetComponent<Rigidbody>();

        if (rb != null)
        {
            bool wasGravity = rb.useGravity;
            rb.useGravity = false;

            while (Vector3.Distance(pickup.transform.position, transform.position) > 1.5f)
            {
                if (pickup == null) break; 

                // Rysujemy naprężoną linę podążającą za przedmiotem
                DrawBezierCurve(pickup.transform.position, 0f);

                Vector3 direction = (transform.position - pickup.transform.position).normalized;
                rb.linearVelocity = direction * pickupPullSpeed; 

                yield return null;
            }

            if (rb != null)
            {
                rb.useGravity = wasGravity;
                rb.linearVelocity = Vector3.zero;
            }
        }

        lineRenderer.enabled = false;
    }
}
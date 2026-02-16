using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Potrzebne do sortowania list

public class LassoController : MonoBehaviour
{
    [Header("References")]
    public SourceMovement playerMovement;
    public Camera playerCamera;
    public LineRenderer lineRenderer; // Przypisz komponent LineRenderer
    public Transform firePoint;       // Punkt, z którego wylatuje lina (np. d³oñ/broñ)

    [Header("Lasso Settings")]
    public float maxRange = 20f;      // Maksymalny zasiêg
    public float radius = 1.5f;       // "Gruboœæ" celowania (wybaczanie b³êdów)
    public float cooldown = 1f;        // Czas odnowienia lassa
    public LayerMask hitLayers;       // W co mo¿e uderzyæ lasso (wszystko: œciany, pickupy, punkty)
    public LayerMask obstacleLayers;  // Co blokuje lasso (np. œciany, ¿eby nie ³apaæ przez œciany)

    [Header("Pull Settings")]
    public float playerPullForce = 25f; // Si³a przyci¹gania gracza do œciany
    public float pickupPullSpeed = 15f; // Prêdkoœæ przyci¹gania przedmiotu

    // Logika wizualna
    private Coroutine _pullCoroutine;

    bool isOnCooldown = false;
    private void Awake()
    {
        playerMovement = GetComponent<SourceMovement>();
        playerCamera = Camera.main;
            lineRenderer = GetComponent<LineRenderer>();
        
    }
    void Update()
    {
        // Prawy przycisk myszy lub klawisz E
        if (Input.GetKeyDown(KeyCode.F) && isOnCooldown == false)
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
        // 1. ZnajdŸ najlepszy cel
        GameObject bestTarget = FindBestTarget();

        if (bestTarget != null)
        {
            // 2. Rozpoznaj co to jest i dzia³aj
            if (bestTarget.CompareTag("GrapplePoint"))
            {
                StartCoroutine(PullPlayerToTarget(bestTarget.transform.position));
            }
            else if (bestTarget.CompareTag("Pickup"))
            {
                StartCoroutine(PullPickupToPlayer(bestTarget));
            }
        }
        else
        {
            // Opcjonalnie: Strza³ w pustkê (tylko efekt wizualny)
            StartCoroutine(VisualEffectOnly(playerCamera.transform.position + playerCamera.transform.forward * maxRange));
        }
    }

    // --- SYSTEM CELOWANIA (SMART TARGETING) ---

    private GameObject FindBestTarget()
    {
        // Rzucamy "grub¹ rurê" (SphereCastAll) przed siebie
        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.transform.position, radius, playerCamera.transform.forward, maxRange, hitLayers);

        // Listy do segregacji celów
        List<RaycastHit> points = new List<RaycastHit>();
        List<RaycastHit> pickups = new List<RaycastHit>();

        foreach (var hit in hits)
        {
            // SprawdŸ czy widzimy obiekt (czy nie jest za œcian¹)
            if (!IsLineOfSightClear(hit.collider.transform)) continue;

            if (hit.collider.CompareTag("GrapplePoint")) points.Add(hit);
            else if (hit.collider.CompareTag("Pickup")) pickups.Add(hit);
        }

        // HIERARCHIA:
        // 1. Najpierw szukamy Grapple Points blisko celownika
        if (points.Count > 0)
        {
            // Sortujemy: który jest najbli¿ej œrodka ekranu?
            return GetClosestToCrosshair(points);
        }

        // 2. Jeœli nie ma punktów zaczepu, szukamy Pickupów
        if (pickups.Count > 0)
        {
            return GetClosestToCrosshair(pickups);
        }

        // 3. Jeœli nic wa¿nego, sprawdŸmy czy po prostu nie trafiliœmy w œcianê (opcjonalne, do przyci¹gania siê do dowolnej œciany)
        /* RaycastHit wallHit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out wallHit, maxRange, hitLayers))
        {
             return wallHit.collider.gameObject; // Odkomentuj, jeœli chcesz ³apaæ siê wszystkiego
        }
        */

        return null;
    }

    // Sprawdza, czy obiekt jest zas³oniêty przez œcianê
    private bool IsLineOfSightClear(Transform target)
    {
        Vector3 direction = target.position - playerCamera.transform.position;
        float dist = direction.magnitude;

        // Rzucamy cienki promieñ sprawdzaj¹cy tylko przeszkody
        if (Physics.Raycast(playerCamera.transform.position, direction, dist, obstacleLayers))
        {
            return false; // Coœ zas³ania
        }
        return true;
    }

    // Wybiera obiekt z listy, który jest najbardziej na wprost celownika
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

    // --- LOGIKA PRZYCI¥GANIA (AKCJE) ---

    // Mechanika 2: Przyci¹ganie gracza do punktu
    private IEnumerator PullPlayerToTarget(Vector3 targetPos)
    {
        lineRenderer.enabled = true;

        // Obliczamy kierunek wybicia
        Vector3 direction = (targetPos - transform.position).normalized;

        // Dajemy "kopa" w stronê celu (u¿ywaj¹c Twojej nowej funkcji AddImpulse)
        // Mo¿esz dodaæ Vector3.up * 5f, ¿eby lekko podbiæ gracza do góry
        playerMovement.AddImpulse(direction * playerPullForce + Vector3.up * 2f);

        // Efekt wizualny trwa chwilê (np. 0.2s)
        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            UpdateLineRenderer(targetPos);
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    // Mechanika 3: Przyci¹ganie pickupa do gracza
    private IEnumerator PullPickupToPlayer(GameObject pickup)
    {
        lineRenderer.enabled = true;
        Rigidbody rb = pickup.GetComponent<Rigidbody>();

        // Jeœli pickup nie ma fizyki, dodajemy j¹ tymczasowo lub przesuwamy transformem
        // Zak³adam, ¿e pickupy maj¹ Rigidbody
        if (rb != null)
        {
            // Wy³¹czamy grawitacjê na chwilê, ¿eby lecia³ prosto do rêki
            bool wasGravity = rb.useGravity;
            rb.useGravity = false;

            // Pêtla przyci¹gania (dopóki nie jest blisko gracza)
            while (Vector3.Distance(pickup.transform.position, transform.position) > 1.5f)
            {
                if (pickup == null) break; // Zabezpieczenie jakby znikn¹³

                // Odœwie¿ wizualizacjê
                UpdateLineRenderer(pickup.transform.position);

                // Ruch obiektu w stronê gracza
                Vector3 direction = (transform.position - pickup.transform.position).normalized;
                rb.linearVelocity = direction * pickupPullSpeed; // U¿ywamy velocity dla p³ynnoœci

                yield return null;
            }

            // Koniec przyci¹gania
            if (rb != null)
            {
                rb.useGravity = wasGravity;
                rb.linearVelocity = Vector3.zero;
            }
        }

        lineRenderer.enabled = false;
    }

    // Strza³ w pud³o (tylko wizualny)
    private IEnumerator VisualEffectOnly(Vector3 endPos)
    {
        lineRenderer.enabled = true;
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            UpdateLineRenderer(endPos);
            yield return null;
        }
        lineRenderer.enabled = false;
    }

    private void UpdateLineRenderer(Vector3 targetPosition)
    {
        // Pocz¹tek liny w broni/rêce, koniec w celu
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, targetPosition);
    }
}
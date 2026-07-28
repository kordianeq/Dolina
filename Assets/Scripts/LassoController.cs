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
    public Transform firePoint;       // Punkt, z kt�rego wylatuje lina (np. d�o�/bro�)

    [Header("Lasso Settings")]
    public float maxRange = 20f;      // Maksymalny zasi�g
    public float radius = 1.5f;       // "Grubo��" celowania (wybaczanie b��d�w)
    public float cooldown = 1f;        // Czas odnowienia lassa
    public LayerMask hitLayers;       // W co mo�e uderzy� lasso (wszystko: �ciany, pickupy, punkty)
    public LayerMask obstacleLayers;  // Co blokuje lasso (np. �ciany, �eby nie �apa� przez �ciany)

    [Header("Pull Settings")]
    public float playerPullForce = 25f; // Si�a przyci�gania gracza do �ciany
    public float pickupPullSpeed = 15f; // Pr�dko�� przyci�gania przedmiotu

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
        if (Input.GetButtonDown("Lasso") && isOnCooldown == false)
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
        // 1. Znajd� najlepszy cel
        GameObject bestTarget = FindBestTarget();

        if (bestTarget != null)
        {
            // 2. Rozpoznaj co to jest i dzia�aj
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
            // Opcjonalnie: Strza� w pustk� (tylko efekt wizualny)
            StartCoroutine(VisualEffectOnly(playerCamera.transform.position + playerCamera.transform.forward * maxRange));
        }
    }

    // --- SYSTEM CELOWANIA (SMART TARGETING) ---

    private GameObject FindBestTarget()
    {
        // Rzucamy "grub� rur�" (SphereCastAll) przed siebie
        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.transform.position, radius, playerCamera.transform.forward, maxRange, hitLayers);

        // Listy do segregacji cel�w
        List<RaycastHit> points = new List<RaycastHit>();
        List<RaycastHit> pickups = new List<RaycastHit>();

        foreach (var hit in hits)
        {
            // Sprawd� czy widzimy obiekt (czy nie jest za �cian�)
            if (!IsLineOfSightClear(hit.collider.transform)) continue;

            if (hit.collider.CompareTag("GrapplePoint")) points.Add(hit);
            else if (hit.collider.CompareTag("Pickup")) pickups.Add(hit);
            else if (hit.collider.CompareTag("EnemyGrapple")) return hit.collider.transform.parent.gameObject;
        }

        // HIERARCHIA:
        // 1. Najpierw szukamy Grapple Points blisko celownika
        if (points.Count > 0)
        {
            // Sortujemy: kt�ry jest najbli�ej �rodka ekranu?
            return GetClosestToCrosshair(points);
        }

        // 2. Je�li nie ma punkt�w zaczepu, szukamy Pickup�w
        if (pickups.Count > 0)
        {
            return GetClosestToCrosshair(pickups);
        }

        // 3. Je�li nic wa�nego, sprawd�my czy po prostu nie trafili�my w �cian� (opcjonalne, do przyci�gania si� do dowolnej �ciany)
        /* RaycastHit wallHit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out wallHit, maxRange, hitLayers))
        {
             return wallHit.collider.gameObject; // Odkomentuj, je�li chcesz �apa� si� wszystkiego
        }
        */

        return null;
    }

    // Sprawdza, czy obiekt jest zas�oni�ty przez �cian�
    private bool IsLineOfSightClear(Transform target)
    {
        Vector3 direction = target.position - playerCamera.transform.position;
        float dist = direction.magnitude;

        // Rzucamy cienki promie� sprawdzaj�cy tylko przeszkody
        if (Physics.Raycast(playerCamera.transform.position, direction, dist, obstacleLayers))
        {
            return false; // Co� zas�ania
        }
        return true;
    }

    // Wybiera obiekt z listy, kt�ry jest najbardziej na wprost celownika
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

    // --- LOGIKA PRZYCI�GANIA (AKCJE) ---

    // Mechanika 2: Przyci�ganie gracza do punktu
    private IEnumerator PullPlayerToTarget(Vector3 targetPos)
    {
        lineRenderer.enabled = true;

        // Obliczamy kierunek wybicia
        Vector3 direction = (targetPos - transform.position).normalized;

        // Dajemy "kopa" w stron� celu (u�ywaj�c Twojej nowej funkcji AddImpulse)
        // Mo�esz doda� Vector3.up * 5f, �eby lekko podbi� gracza do g�ry
        playerMovement.AddImpulse(direction * playerPullForce + Vector3.up * 2f);

        // Efekt wizualny trwa chwil� (np. 0.2s)
        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            UpdateLineRenderer(targetPos);
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    // Mechanika 3: Przyci�ganie pickupa do gracza
    private IEnumerator PullPickupToPlayer(GameObject pickup)
    {
        lineRenderer.enabled = true;
        Rigidbody rb = pickup.GetComponent<Rigidbody>();

        // Je�li pickup nie ma fizyki, dodajemy j� tymczasowo lub przesuwamy transformem
        // Zak�adam, �e pickupy maj� Rigidbody
        if (rb != null)
        {
            // Wy��czamy grawitacj� na chwil�, �eby lecia� prosto do r�ki
            bool wasGravity = rb.useGravity;
            rb.useGravity = false;

            // P�tla przyci�gania (dop�ki nie jest blisko gracza)
            while (Vector3.Distance(pickup.transform.position, transform.position) > 1.5f)
            {
                if (pickup == null) break; // Zabezpieczenie jakby znikn��

                // Od�wie� wizualizacj�
                UpdateLineRenderer(pickup.transform.position);

                // Ruch obiektu w stron� gracza
                Vector3 direction = (transform.position - pickup.transform.position).normalized;
                rb.linearVelocity = direction * pickupPullSpeed; // U�ywamy velocity dla p�ynno�ci

                yield return null;
            }

            // Koniec przyci�gania
            if (rb != null)
            {
                rb.useGravity = wasGravity;
                rb.linearVelocity = Vector3.zero;
            }
        }

        lineRenderer.enabled = false;
    }

    // Strza� w pud�o (tylko wizualny)
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
        // Pocz�tek liny w broni/r�ce, koniec w celu
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, targetPosition);
    }
}
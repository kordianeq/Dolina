using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RevolverFocus : Ability
{
    [System.Serializable]
    public class FocusTarget
    {
        public EnemyCore enemy;
        public float timeVisible = 0f;
        public float stackProgress = 0f;
        public int Stacks => Mathf.FloorToInt(stackProgress);
        public bool isVisibleNow = false;

        public FocusTarget(EnemyCore enemyCore)
        {
            enemy = enemyCore;
            timeVisible = 0f;
            stackProgress = 0f;
            isVisibleNow = false;
        }
    }

    [Header("Ustawienia Focusa (Deadeye)")]
    [Tooltip("Maksymalny czas trwania skupienia (w sekundach czasu rzeczywistego).")]
    public float focusDuration = 6.0f;

    [Tooltip("Częstotliwość sprawdzania widoczności raycastem (np. co 0.05s = 20 razy na sekundę).")]
    public float checkInterval = 0.05f;

    [Tooltip("Zwolnienie tempa gry podczas skupienia (np. 0.25 = 25% prędkości).")]
    [Range(0.05f, 1f)]
    public float slowMotionScale = 0.25f;

    [Tooltip("Czy wróg musi znajdować się w polu widzenia kamery (na ekranie), aby zacząć naliczać stacki?")]
    public bool requireOnScreen = true;

    [Tooltip("Maska warstw blokujących wzrok (np. ściany, podłogi, przeszkody).")]
    public LayerMask obstacleMask = ~0;

    [Header("Progi Obrażeń (Stacki)")]
    [Tooltip("Prędkość nakładania stacków: ile stacków na sekundę zyskuje wróg podczas widoczności (np. 1 = 1 stack/s, 2 = 2 stacki/s, 3.33 = 10 stacków w 3 sekundy).")]
    public float stacksPerSecond = 2.0f;

    [Tooltip("Minimalna liczba stacków, aby w ogóle oddać strzał do tego wroga.")]
    public int minStacksToShoot = 3;

    [Tooltip("Próg 1: Wymagana liczba stacków (np. 5).")]
    public int tier1MinStacks = 5;
    [Tooltip("Obrażenia zadawane dla Progu 1 (5 - 9 stacków).")]
    public float damageTier1 = 100f;

    [Tooltip("Próg 2: Wymagana liczba stacków (np. 10).")]
    public int tier2MinStacks = 10;
    [Tooltip("Obrażenia zadawane dla Progu 2 (10+ stacków).")]
    public float damageTier2 = 200f;

    [Tooltip("Opcjonalny mnożnik obrażeń za każdy stack, gdy nie osiągnięto progu 1.")]
    public float damagePerStack = 20f;

    [Header("Sekwencja Wystrzału")]
    [Tooltip("Odstęp czasowy (w sekundach) między kolejnymi strzałami (np. 0.07s daje westernowy efekt 'fan-the-hammer').")]
    public float timeBetweenShots = 0.07f;

    [Header("Audio (Opcjonalne)")]
    public AudioClip focusStartSound;
    public AudioClip shootSound;
    public AudioClip targetLockedSound;
    [Header("Visuals")]
    
    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] Color lethalColor = Color.red;

    [SerializeField] VolumeProfile abilityPostProcess;

    [Header("UI Prefab & Kontener")]
    [SerializeField] private FocusMarkerUI markerPrefab;
    [SerializeField] private Transform markersContainer; // Np. panel na Canvasie w grze

    // Pula utworzonych znaczników
    private List<FocusMarkerUI> markerPool = new List<FocusMarkerUI>(); 

    [Header("Aktywne Cele (Podgląd)")]
    public List<FocusTarget> trackedTargets = new List<FocusTarget>();

    [HideInInspector] public bool isFocusActive = false;
    private Coroutine focusRoutine;
    private Coroutine fireSequenceRoutine;

    public override void Start()
    {
        base.Start();

        // Wyklucz warstwy gracza i Ignore Raycast z maski przeszkód
        int playerLayer = LayerMask.NameToLayer("Player");
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        if (playerLayer != -1) obstacleMask &= ~(1 << playerLayer);
        if (ignoreRaycast != -1) obstacleMask &= ~(1 << ignoreRaycast);
    }

    public override void Update()
    {
        base.Update();

        // Jeśli umiejętność trwa i gracz wciśnie PPM lub LPM -> natychmiastowy wcześniejszy wystrzał!
        if (isFocusActive)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
                FireAllLockedShots();
            }
        }
    }

    private void LateUpdate()
    {
        if (!isFocusActive)
        {
            HideAllMarkers();
            return;
        }

        UpdateAllVisuals();
    }

    private void UpdateAllVisuals()
    {
        if (Camera.main == null || markerPrefab == null) return;

        int markerIndex = 0;

        foreach (FocusTarget target in trackedTargets)
        {
            // Sprawdzamy czy cel żyje i jest obecnie widoczny
            if (target.enemy == null || target.enemy.dead || !target.isVisibleNow)
                continue;

            // Rzutujemy pozycję klatki piersiowej wroga (np. + 1.2m w górę) na ekran
            Vector3 worldPos = target.enemy.transform.position + Vector3.up * 1.2f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Jeśli wróg jest za kamerą gracza - nie rysujemy znacznika
            if (screenPos.z <= 0) continue;

            // Obliczamy procent naładowania do progu 200 DMG (tier2MinStacks np. 10 stacków)
            float percentage = Mathf.Clamp01(target.stackProgress / tier2MinStacks);

            // Pobieramy marker z puli
            FocusMarkerUI marker = GetOrCreateMarker(markerIndex);
            if (marker != null)
            {
                marker.Show();
                marker.UpdateMarker(screenPos, percentage, defaultColor, lethalColor);
                markerIndex++;
            }
        }

        // Ukrywamy nadmiarowe markery z puli, które nie są teraz używane
        for (int i = markerIndex; i < markerPool.Count; i++)
        {
            if (markerPool[i] != null) markerPool[i].Hide();
        }
    }

    private FocusMarkerUI GetOrCreateMarker(int index)
    {
        if (index < markerPool.Count)
        {
            if (markerPool[index] != null) return markerPool[index];
        }

        if (markerPrefab == null) return null;

        Transform parent = markersContainer;
        if (parent == null && UiMenager.Instance != null && UiMenager.Instance.gameUi != null)
        {
            parent = UiMenager.Instance.gameUi.transform;
        }
        else if (parent == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null) parent = canvas.transform;
        }

        FocusMarkerUI newMarker = Instantiate(markerPrefab, parent);
        if (index < markerPool.Count)
        {
            markerPool[index] = newMarker;
        }
        else
        {
            markerPool.Add(newMarker);
        }
        return newMarker;
    }

    public void HideAllMarkers()
    {
        foreach (var marker in markerPool)
        {
            if (marker != null) marker.Hide();
        }
    }

    public override void ActivateAbility()
    {
        
        if (isFocusActive) return;
        if(_animator) _animator.SetBool("HatOn",true);
        if(abilityPostProcess) gameManager.ChangePostProcessProfile(abilityPostProcess);
        _isAbilityActive = false;
        isFocusActive = true;

        if (focusStartSound != null)
        {
            AudioManager.Instance.PlaySound(focusStartSound);
        }

        // 1. Zbieramy wszystkich aktywnych wrogów z EnemiesManagera
        trackedTargets.Clear();
        if (EnemiesManager.Instance != null)
        {
            foreach (EnemyCore enemy in EnemiesManager.Instance.activeEnemyCores)
            {
                if (enemy != null && !enemy.dead)
                {
                    trackedTargets.Add(new FocusTarget(enemy));
                }
            }
        }

        // 2. Aktywujemy Bullet-Time (zwolnienie czasu)
        Time.timeScale = slowMotionScale;

        // 3. Uruchamiamy pętlę śledzenia celów
        if (focusRoutine != null) StopCoroutine(focusRoutine);
        focusRoutine = StartCoroutine(FocusTrackingRoutine());
    }

    private IEnumerator FocusTrackingRoutine()
    {
        float elapsedRealTime = 0f;
        float duration = abilityDuration > 0 ? abilityDuration : focusDuration;
        Camera cam = Camera.main;

        while (elapsedRealTime < duration)
        {
            Vector3 eyePos = cam != null ? cam.transform.position : transform.position + Vector3.up * 1.6f;

            for (int i = trackedTargets.Count - 1; i >= 0; i--)
            {
                FocusTarget target = trackedTargets[i];

                if (target.enemy == null || target.enemy.dead)
                {
                    trackedTargets.RemoveAt(i);
                    continue;
                }

                Vector3 enemyChest = target.enemy.transform.position + Vector3.up * 1.2f;
                Vector3 toEnemy = enemyChest - eyePos;
                float distance = toEnemy.magnitude;

                // 1. Sprawdzenie czy wróg jest w polu widzenia kamery (na ekranie)
                if (requireOnScreen && cam != null)
                {
                    Vector3 vp = cam.WorldToViewportPoint(enemyChest);
                    bool onScreen = vp.z > 0 && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;

                    if (!onScreen)
                    {
                        target.isVisibleNow = false;
                        continue;
                    }
                }

                // 2. Raycast sprawdzający czy ściana/przeszkoda nie zasłania wroga
                if (Physics.Raycast(eyePos, toEnemy.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    bool hitEnemy = hit.collider.transform.root == target.enemy.transform.root ||
                                   hit.collider.CompareTag("Enemy") ||
                                   hit.collider.GetComponentInParent<EnemyCore>() == target.enemy;

                    if (hitEnemy)
                    {
                        target.isVisibleNow = true;
                        target.timeVisible += checkInterval;
                        target.stackProgress += checkInterval * stacksPerSecond;
                    }
                    else
                    {
                        target.isVisibleNow = false;
                    }
                }
                else
                {
                    // Brak kolizji na trasie -> wróg widoczny
                    target.isVisibleNow = true;
                    target.timeVisible += checkInterval;
                    target.stackProgress += checkInterval * stacksPerSecond;
                }
            }

            elapsedRealTime += checkInterval;
            yield return new WaitForSecondsRealtime(checkInterval);
        }

        // Czas upłynął automatycznie -> Strzał końcowy!
        FireAllLockedShots();
    }

    public void FireAllLockedShots()
    {
        if (!isFocusActive) return;
        isFocusActive = false;

        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
            focusRoutine = null;
        }

        // Przywracamy normalną prędkość gry
        Time.timeScale = 1.0f;

        // Filtrujemy wrogów, którzy osiągnęli minimalną liczbę stacków
        List<FocusTarget> targetsToShoot = new List<FocusTarget>();
        foreach (FocusTarget target in trackedTargets)
        {
            if (target.enemy != null && !target.enemy.dead && target.Stacks >= minStacksToShoot)
            {
                targetsToShoot.Add(target);
            }
        }

        if (fireSequenceRoutine != null) StopCoroutine(fireSequenceRoutine);
        fireSequenceRoutine = StartCoroutine(FireSequenceRoutine(targetsToShoot));
    }

    private IEnumerator FireSequenceRoutine(List<FocusTarget> targetsToShoot)
    {
        if (targetsToShoot.Count > 0)
        {
            Debug.Log($"[RevolverFocus] Wystrzał serii do {targetsToShoot.Count} namierzonych celów!");

            foreach (FocusTarget target in targetsToShoot)
            {
                if (target.enemy == null || target.enemy.dead) continue;

                ExecuteShot(target);

                if (timeBetweenShots > 0f)
                {
                    yield return new WaitForSeconds(timeBetweenShots);
                }
            }
        }
        else
        {
            Debug.Log("[RevolverFocus] Żaden wróg nie osiągnął wymaganego minimum stacków. Brak strzałów.");
        }

        trackedTargets.Clear();
        fireSequenceRoutine = null;
         if(_animator) _animator.SetBool("HatOn",false);
         if(abilityPostProcess) gameManager.ResetPostProcessProfileToDefault();
        // Rozpoczynamy odliczanie cooldownu
        if (abilityCooldown > 0f)
        {
            Invoke(nameof(ResetAbility), abilityCooldown);
        }
        else
        {
            ResetAbility();
        }
    }

    private void ExecuteShot(FocusTarget target)
    {
        int stacks = target.Stacks;
        float finalDamage = CalculateDamageForStacks(stacks);

        Vector3 hitPoint = target.enemy.transform.position + Vector3.up * 1.2f;
        Vector3 shootDir = (hitPoint - (Camera.main != null ? Camera.main.transform.position : transform.position)).normalized;

        Debug.Log($"[RevolverFocus] Trafiono '{target.enemy.gameObject.name}' za {finalDamage} DMG! (Stacki: {stacks})");

        // Dźwięk strzału
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // Efekt uderzenia kamery
        if (GameManager.Instance != null && GameManager.Instance.PlayerCam != null)
        {
            GameManager.Instance.PlayerCam.ShootEffect();
        }

        // Zadanie obrażeń przez system obrażeń wroga
        if (target.enemy.dmgMannager != null)
        {
            target.enemy.dmgMannager.TakeHp(finalDamage, 15f, shootDir);
        }
        else if (target.enemy.TryGetComponent<IDamagable>(out var damagable))
        {
            damagable.Damaged(finalDamage, shootDir, 15f);
        }
    }

    public float CalculateDamageForStacks(int stacks)
    {
        if (stacks >= tier2MinStacks)
        {
            return damageTier2; // 200 DMG (dla 10+ stacków)
        }
        else if (stacks >= tier1MinStacks)
        {
            return damageTier1; // 100 DMG (dla 5 - 9 stacków)
        }
        else if (stacks >= minStacksToShoot)
        {
            return abilityDamage > 0 ? abilityDamage : (stacks * damagePerStack);
        }

        return 0f;
    }

    public void EnablePostProcess()
    {
        
    }

    public override void ResetAbility()
    {
        base.ResetAbility();
        isFocusActive = false;
        Time.timeScale = 1.0f;
        HideAllMarkers();
        if (_animator) _animator.SetBool("HatOn", false);
        if(abilityPostProcess) gameManager.ResetPostProcessProfileToDefault();
        Debug.Log("[RevolverFocus] Umiejętność gotowa do ponownego użycia.");
    }

    private void OnDisable()
    {
        // Bezpieczeństwo: przywróć normalny czas, ukryj markery i zresetuj animacje, jeśli obiekt zostanie wyłączony
        if (isFocusActive)
        {
            Time.timeScale = 1.0f;
            isFocusActive = false;
        }

        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
            focusRoutine = null;
        }

        if (fireSequenceRoutine != null)
        {
            StopCoroutine(fireSequenceRoutine);
            fireSequenceRoutine = null;
        }

        HideAllMarkers();
        if (_animator) _animator.SetBool("HatOn", false);
        if(abilityPostProcess) gameManager.ResetPostProcessProfileToDefault();
    }

    private void OnDrawGizmos()
    {
        if (!isFocusActive) return;

        Vector3 eyePos = Camera.main != null ? Camera.main.transform.position : transform.position + Vector3.up * 1.6f;

        for (int i = 0; i < trackedTargets.Count; i++)
        {
            FocusTarget target = trackedTargets[i];
            if (target == null || target.enemy == null) continue;

            // Zmiana koloru promienia w zależności od progu stacków:
            if (target.Stacks >= tier2MinStacks)
                Gizmos.color = Color.red;         // Próg 2 (Maksymalny, 200 DMG)
            else if (target.Stacks >= tier1MinStacks)
                Gizmos.color = Color.yellow;      // Próg 1 (Średni, 100 DMG)
            else
                Gizmos.color = Color.green;       // Ładowanie (poniżej progu 1)

            if (target.isVisibleNow)
            {
                Gizmos.DrawLine(eyePos, target.enemy.transform.position + Vector3.up * 1.2f);
                Gizmos.DrawWireSphere(target.enemy.transform.position + Vector3.up * 1.2f, 0.35f);
            }
        }
    }
}

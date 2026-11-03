using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GunSystem : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("Dane i statystyki broni (obrażenia, dźwięki, efekty, magazynek) pobierane z ScriptableObject.")]
    public WeaponData weaponData;

    AnimationController animationController;

    [Header("Runtime Ammo")]
    [Tooltip("Całkowita amunicja zapasowa gracza dla tej broni")]
    public int ammo = 180;
    [HideInInspector] public int bulletsLeft;
    public bool allowShooting = true;

    // Bools stanu
    [HideInInspector] public bool shooting, reloading, isScoped;

    private float nextFireTime = 0f;
    public bool readyToShoot
    {
        get => Time.time >= nextFireTime && !reloading;
        set
        {
            if (value) nextFireTime = 0f;
        }
    }

    [Header("Scene References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public LayerMask whatIsEnemy;
    [HideInInspector] public RaycastHit rayHit;

    [Header("Settings & Visual Tiers")]
    public bool customReload;
    public bool swietoscDependent;
    public List<GameObject> revolverModels;
    public ParticleSystem upgradeParticle;

    [Header("Events")]
    public UnityEvent<Vector3, float> OnTargetHit;

    // Zmienne pomocnicze ukryte przed edytorem
    [HideInInspector] public int bulletsShot;
    [HideInInspector] public Vector3 recoilMovementForce = Vector3.zero;
    [HideInInspector] public float playerSpeed = 0f;
    [HideInInspector] public bool hasStates;

    #region Properties accessing WeaponData
    public string WeaponName => (weaponData != null && !string.IsNullOrEmpty(weaponData.weaponName)) ? weaponData.weaponName : gameObject.name;
    public WeaponType Type => weaponData != null ? weaponData.weaponType : WeaponType.Revolver;
    public float Damage => weaponData != null ? weaponData.damage : 20f;
    public float TimeBetweenShooting => weaponData != null ? weaponData.timeBetweenShooting : 0.3f;
    public float Spread => weaponData != null ? weaponData.spread : 0f;
    public float Range => weaponData != null ? weaponData.range : 100f;
    public float ReloadTime => weaponData != null ? weaponData.reloadTime : 1f;
    public int MagazineSize => weaponData != null ? weaponData.magazineSize : 6;
    public int BulletsPerTap => weaponData != null ? Mathf.Max(1, weaponData.bulletsPerTap) : 1;
    public bool AllowButtonHold => weaponData != null ? weaponData.allowButtonHold : false;
    public bool CanBeScoped => weaponData != null ? weaponData.canBeScoped : false;
    public float RecoilForce => weaponData != null ? weaponData.recoilForce : 0f;
    public float ConeOfFire => weaponData != null ? weaponData.coneOfFire : 0.3f;
    public bool DamageRangeReduction => weaponData != null ? weaponData.damageRangeReduction : false;
    public float FullDamageRange => weaponData != null ? weaponData.fullDamageRange : 10f;
    public bool AllowRicochet => weaponData != null ? weaponData.allowRicochet : false;
    public float RicochetChance => weaponData != null ? weaponData.ricochetChance : 0.05f;
    public float RicochetRange => weaponData != null ? weaponData.ricochetRange : 10f;
    public float RicochetDamageMultiplier => weaponData != null ? weaponData.ricochetDamageMultiplier : 0.5f;
    public float ScopedFov => (weaponData != null && weaponData.scopedFov > 0) ? weaponData.scopedFov : newFov;
    public float TracerSpeed => (weaponData != null && weaponData.tracerSpeed > 0) ? weaponData.tracerSpeed : 100f;

    public AudioClip[] FireSounds => weaponData != null ? weaponData.fireSounds : null;
    public AudioClip ReloadSound => weaponData != null ? weaponData.reloadSound : null;
    public AudioClip EmptySound => weaponData != null ? weaponData.emptyFireSound : null;
    public AudioClip[] RicochetSounds => weaponData != null ? weaponData.ricochetSounds : null;
    public AudioClip[] EnemyHitSounds => weaponData != null ? weaponData.enemyHitSounds : null;
    public GameObject MuzzleFlashGraphic => weaponData != null ? weaponData.muzzleFlash : null;
    public GameObject BulletHoleGraphicPrefab => weaponData != null ? weaponData.bulletHoleGraphic : null;
    public TrailRenderer BulletTrailPrefab => weaponData != null ? weaponData.bulletTrail : null;
    #endregion

    float oldFov = 60f;
    float newFov = 25f;

    GameManager gameManager;
    UiMenager uiMenager;
    AudioManager audioManager;
    CameraControll cameraControll;
    public PlayerStats playerStats;
    SourceMovement playerMovement;
    CinemachineCamera cineCam;

    private int lastKnownAmmo = -1;
    private int lastKnownBulletsLeft = -1;
    private float lastSwietosc = float.MinValue;

    private void Awake()
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"[GunSystem] Obiekt '{gameObject.name}' nie ma przypisanego WeaponData! Przypisz asset WeaponData w Inspektorze.", this);
        }

        gameManager = GameManager.Instance;
        audioManager = AudioManager.Instance;
        uiMenager = UiMenager.Instance != null ? UiMenager.Instance : FindFirstObjectByType<UiMenager>();
        animationController = GetComponentInChildren<AnimationController>();
        fpsCam = Camera.main;

        if (gameManager != null)
        {
            cameraControll = gameManager.PlayerCam;
            if (gameManager.PlayerRef != null)
            {
                playerMovement = gameManager.PlayerRef.GetComponent<SourceMovement>();
                if (playerStats == null)
                    playerStats = gameManager.PlayerStats;
            }
        }

        if (cineCam == null)
        {
            cineCam = GameManager.Instance.PlayerCam.gameObject.GetComponent<CinemachineCamera>();
        }
        if (cineCam != null)
        {
            oldFov = cineCam.Lens.FieldOfView;
        }

        bulletsLeft = MagazineSize;
        allowShooting = true;
        nextFireTime = 0f;
    }

    private void OnEnable()
    {
        reloading = false;
        isScoped = false;
        if (Time.timeScale < 1f) Time.timeScale = 1f;

        UpdateUi();
    }

    private void OnDisable()
    {
        if (isScoped)
        {
            if (cineCam != null) cineCam.Lens.FieldOfView = oldFov;
            if (uiMenager != null && uiMenager.scopePanel != null) uiMenager.scopePanel.SetActive(false);
            Time.timeScale = 1f;
            isScoped = false;
        }
        CancelInvoke();
    }

    private void Start()
    {
        UpdateUi();
    }

    void Update()
    {
        if (swietoscDependent && Type == WeaponType.Revolver)
        {
            CheckSwietosc();
        }

        if (playerMovement != null)
        {
            playerSpeed = playerMovement.playerSpeed;
        }
        else if (gameManager != null && gameManager.PlayerRef != null)
        {
            playerMovement = gameManager.PlayerRef.GetComponent<SourceMovement>();
        }

        MyInput();

        // Wykrycie zmian z zewnątrz (np. podniesienie amunicji w pickUp.cs)
        if (ammo != lastKnownAmmo || bulletsLeft != lastKnownBulletsLeft)
        {
            UpdateUi();
        }
    }

    public void UpdateUi()
    {
        lastKnownAmmo = ammo;
        lastKnownBulletsLeft = bulletsLeft;

        if (uiMenager == null)
        {
            uiMenager = UiMenager.Instance != null ? UiMenager.Instance : FindFirstObjectByType<UiMenager>();
            if (uiMenager == null) return;
        }

        string displayName = WeaponName;

        if (uiMenager.ammoText != null) uiMenager.ammoText.SetText($"{bulletsLeft}/{MagazineSize}");
        if (uiMenager.gunName != null) uiMenager.gunName.SetText(displayName);
        if (uiMenager.totalAmmoText != null) uiMenager.totalAmmoText.SetText(ammo.ToString());
    }

    private void MyInput()
    {
        if (AllowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");

        // Przeładowanie
        if (Input.GetButtonDown("Reload") && bulletsLeft < MagazineSize && !reloading && ammo > 0)
        {
            Reload();
            if (animationController) animationController.Reload();
        }

        // Strzał
        if (readyToShoot && shooting && !reloading && allowShooting)
        {
            if (bulletsLeft <= 0)
            {
                if (audioManager && EmptySound != null)
                {
                    audioManager.PlaySound(EmptySound);
                }
                return;
            }

            if (CanBeScoped)
            {
                if (isScoped)
                {
                    Shoot();
                    if (transform.childCount > 0) transform.GetChild(0).gameObject.SetActive(true);
                    if (cineCam != null) cineCam.Lens.FieldOfView = oldFov;
                    if (uiMenager != null && uiMenager.scopePanel != null) uiMenager.scopePanel.SetActive(false);

                    Time.timeScale = 1f;
                    isScoped = false;
                }
                else
                {
                    if (transform.childCount > 0) transform.GetChild(0).gameObject.SetActive(false);
                    if (cineCam != null) cineCam.Lens.FieldOfView = ScopedFov;
                    if (uiMenager != null && uiMenager.scopePanel != null) uiMenager.scopePanel.SetActive(true);
                    isScoped = true;

                    if (gameManager != null && gameManager.PlayerRef != null && !gameManager.PlayerRef.isGrounded)
                    {
                        Time.timeScale = 0.25f;
                    }
                }
            }
            else
            {
                Shoot();
            }
        }
    }

    void CheckSwietosc()
    {
        if (playerStats == null) return;
        float swietosc = playerStats.swietosc;
        if (Mathf.Approximately(swietosc, lastSwietosc)) return;
        lastSwietosc = swietosc;

        int activeIndex = -1;
        if (swietosc > -100f && swietosc <= -50f) activeIndex = 0;
        else if (swietosc > -50f && swietosc <= 0f) activeIndex = 1;
        else if (swietosc > 0f && swietosc <= 50f) activeIndex = 2;
        else if (swietosc > 50f && swietosc <= 100f) activeIndex = 3;

        for (int i = 0; i < revolverModels.Count; i++)
        {
            if (revolverModels[i] != null)
            {
                revolverModels[i].SetActive(i == activeIndex);
            }
        }
    }

    void CalculateGunForce()
    {
        if (RecoilForce > 0 && gameManager != null && gameManager.PlayerRef != null)
        {
            gameManager.PlayerRef.AddImpulse(-transform.forward * RecoilForce);
        }
    }

    public Vector3 CalculateRecoil()
    {
        float currentSpread = Spread;
        if (playerSpeed > 0.1f)
        {
            currentSpread *= 1.5f;
        }

        float x = Random.Range(-currentSpread, currentSpread);
        float y = Random.Range(-currentSpread, currentSpread);

        if (fpsCam != null)
        {
            return (fpsCam.transform.forward + (fpsCam.transform.right * x) + (fpsCam.transform.up * y)).normalized;
        }

        return transform.forward;
    }

    public void Shoot()
    {
        nextFireTime = Time.time + TimeBetweenShooting;

        CalculateGunForce();

        if (cameraControll != null) cameraControll.ShootEffect();

        if (MuzzleFlashGraphic != null && attackPoint != null)
        {
            Instantiate(MuzzleFlashGraphic, attackPoint.position, attackPoint.rotation);
        }

        if (audioManager != null && FireSounds != null && FireSounds.Length > 0)
        {
            audioManager.PlaySound(FireSounds);
        }

        if (animationController != null)
        {
            animationController.Shot();
        }

        bulletsLeft--;
        UpdateUi();

        int pellets = BulletsPerTap;
        Vector3 shootOrigin = fpsCam != null ? fpsCam.transform.position : transform.position;

        for (int i = 0; i < pellets; i++)
        {
            Vector3 direction = CalculateRecoil();

            if (Physics.SphereCast(shootOrigin, ConeOfFire, direction, out RaycastHit hit, Range, whatIsEnemy))
            {
                rayHit = hit;

                // Mechanika Rykoszetu
                if (AllowRicochet && Random.value <= RicochetChance)
                {
                    HandleRicochet(hit.point, hit.collider);
                    if (audioManager && RicochetSounds != null && RicochetSounds.Length > 0)
                    {
                        audioManager.PlaySound(RicochetSounds[Random.Range(0, RicochetSounds.Length)]);
                    }
                }

                // Wizualizacja pocisku (Trail)
                if (BulletTrailPrefab != null && attackPoint != null)
                {
                    TrailRenderer trail = Instantiate(BulletTrailPrefab, attackPoint.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, hit.point));
                }

                // Zadawanie obrażeń
                ApplyDamage(hit.collider, direction, hit.point, hit.normal, Damage);
            }
            else
            {
                if (BulletTrailPrefab != null && attackPoint != null)
                {
                    TrailRenderer trail = Instantiate(BulletTrailPrefab, attackPoint.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, shootOrigin + direction * Range));
                }
            }
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 endHit)
    {
        if (trail == null) yield break;

        float time = 0f;
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(startPosition, endHit);
        float speed = TracerSpeed > 0 ? TracerSpeed : 100f;
        float duration = distance / speed;

        while (time < 1f)
        {
            if (trail == null) yield break;
            trail.transform.position = Vector3.Lerp(startPosition, endHit, time);
            time += (duration > 0f) ? (Time.deltaTime / duration) : 1f;
            yield return null;
        }

        if (trail != null)
        {
            trail.transform.position = endHit;
            Destroy(trail.gameObject, trail.time);
        }
    }

    #region Damage
    void ApplyDamage(Collider target, Vector3 direction, Vector3 hitPoint, Vector3 hitNormal, float baseDamage)
    {
        if (target == null) return;

        // Bullet hole decal na obiektach niebędących przeciwnikami/pociskami
        if (!target.CompareTag("Enemy") && !target.CompareTag("NPC") && !target.CompareTag("bullet"))
        {
            if (BulletHoleGraphicPrefab != null)
            {
                var hole = Instantiate(BulletHoleGraphicPrefab, hitPoint + (hitNormal * 0.025f), Quaternion.LookRotation(hitNormal));
                hole.transform.SetParent(target.transform, true);
            }
        }

        // Płynna kalkulacja redukcji obrażeń z dystansem (Damage Falloff)
        float finalDamage = baseDamage;
        if (DamageRangeReduction)
        {
            Vector3 origin = fpsCam != null ? fpsCam.transform.position : transform.position;
            float distance = Vector3.Distance(origin, hitPoint);
            if (distance > FullDamageRange)
            {
                float maxRange = Mathf.Max(Range, FullDamageRange + 0.1f);
                float t = Mathf.Clamp01((distance - FullDamageRange) / (maxRange - FullDamageRange));
                finalDamage = Mathf.Lerp(baseDamage, baseDamage * 0.2f, t);
            }
        }

        if (target.gameObject.TryGetComponent<IDamagable>(out IDamagable enemy))
        {
            OnTargetHit?.Invoke(hitPoint, finalDamage);

            // Próba zadania obrażeń z wektorem odrzutu fizycznego
            bool handled = enemy.Damaged(finalDamage, direction.normalized, 1f);
            if (!handled)
            {
                enemy.Damaged(finalDamage);
            }

            if (EnemyHitSounds != null && EnemyHitSounds.Length > 0 && audioManager != null)
            {
                audioManager.PlaySound(EnemyHitSounds);
            }
        }
    }
    #endregion

    public void ResetShot()
    {
        nextFireTime = 0f;
    }

    void HandleRicochet(Vector3 startPoint, Collider ignoredCollider)
    {
        Collider[] colliders = Physics.OverlapSphere(startPoint, RicochetRange, whatIsEnemy);
        Collider nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col == ignoredCollider) continue;
            if (!col.TryGetComponent<IDamagable>(out _)) continue;

            // Sprawdzenie linii wzroku (czy rykoszet nie przenika przez ścianę)
            if (Physics.Linecast(startPoint, col.bounds.center, out RaycastHit wallHit, ~whatIsEnemy))
            {
                if (wallHit.collider != col) continue;
            }

            float dist = Vector3.Distance(startPoint, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestEnemy = col;
            }
        }

        if (nearestEnemy != null)
        {
            if (BulletTrailPrefab != null && attackPoint != null)
            {
                TrailRenderer trail = Instantiate(BulletTrailPrefab, startPoint, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, nearestEnemy.transform.position));
            }

            Vector3 dir = (nearestEnemy.transform.position - startPoint).normalized;
            ApplyDamage(nearestEnemy, dir, nearestEnemy.transform.position, -dir, Damage * RicochetDamageMultiplier);
        }
    }

    public void InstantReload()
    {
        int needed = MagazineSize - bulletsLeft;
        int amountToLoad = Mathf.Min(needed, ammo);

        bulletsLeft += amountToLoad;
        ammo -= amountToLoad;
        reloading = false;
        UpdateUi();
    }

    public void Reload()
    {
        if (reloading || bulletsLeft >= MagazineSize || ammo <= 0) return;

        if (customReload)
        {
            if (TryGetComponent<IReload>(out var custom))
            {
                custom.Reloaded();
                reloading = true;
            }
        }
        else
        {
            reloading = true;
            Invoke(nameof(ReloadFinished), ReloadTime);
            if (ReloadSound != null && audioManager) audioManager.PlaySound(ReloadSound);
        }
    }

    public void ReloadFinished()
    {
        int needed = MagazineSize - bulletsLeft;
        int amountToLoad = Mathf.Min(needed, ammo);

        bulletsLeft += amountToLoad;
        ammo -= amountToLoad;
        reloading = false;
        UpdateUi();
    }

    private void OnDrawGizmos()
    {
        if (fpsCam == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(fpsCam.transform.position + fpsCam.transform.forward * Range, ConeOfFire);
    }
}

[System.Serializable]
public struct GunSaveData
{
    public List<int> ammo;
}

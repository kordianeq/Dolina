using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class GunSystem : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("Jeśli przypisane, statystyki broni są pobierane z tego ScriptableObject. W przeciwnym razie używane są poniższe wartości domyślne.")]
    public WeaponData weaponData;

    AnimationController animationController;

    [Header("Gun Stats (Fallback if WeaponData is null)")]
    public float damage = 20f;
    public float timeBetweenShooting = 0.3f;
    public float spread = 0f;
    public float range = 100f;
    public float reloadTime = 1f;
    public float timeBetweenShots = 0f;
    public int magazineSize = 6;
    public int bulletsPerTap = 1;
    public int ammo = 180;
    public bool allowButtonHold = false;
    public bool allowShooting = true;
    public bool canBeScoped = false;
    public bool damageRangeRedduction = false;
    public float fullDamageRange = 10f;
    public float recoilForce = 0f;
    public float force = 3f;
    public float coneOfFire = 0.30f;

    [HideInInspector] public int bulletsLeft, bulletsShot;
    public Vector3 recoilMovementForce = Vector3.zero;
    public float playerSpeed = 0f;

    // Bools stanu
    [HideInInspector] public bool shooting, readyToShoot, reloading, isScoped;

    [Header("Other Settings")]
    public bool hasStates;
    public bool customReload;
    public bool swietoscDependent;

    [Header("Ricochet System (Fallback)")]
    public bool allowRicochet = false;
    [Range(0f, 1f)] public float ricochetChance = 0.05f;
    public float ricochetRange = 10f;
    public float ricochetDamageMultiplier = 0.5f;

    #region Properties accessing WeaponData or fallback
    public float Damage => weaponData != null ? weaponData.damage : damage;
    public float TimeBetweenShooting => weaponData != null ? weaponData.timeBetweenShooting : timeBetweenShooting;
    public float Spread => weaponData != null ? weaponData.spread : spread;
    public float Range => weaponData != null ? weaponData.range : range;
    public float ReloadTime => weaponData != null ? weaponData.reloadTime : reloadTime;
    public float TimeBetweenShots => weaponData != null ? weaponData.timeBetweenShots : timeBetweenShots;
    public int MagazineSize => weaponData != null ? weaponData.magazineSize : magazineSize;
    public int BulletsPerTap => weaponData != null ? Mathf.Max(1, weaponData.bulletsPerTap) : Mathf.Max(1, bulletsPerTap);
    public bool AllowButtonHold => weaponData != null ? weaponData.allowButtonHold : allowButtonHold;
    public bool CanBeScoped => weaponData != null ? weaponData.canBeScoped : canBeScoped;
    public float RecoilForce => weaponData != null ? weaponData.recoilForce : recoilForce;
    public float ConeOfFire => weaponData != null ? weaponData.coneOfFire : coneOfFire;
    public bool DamageRangeReduction => weaponData != null ? weaponData.damageRangeReduction : damageRangeRedduction;
    public float FullDamageRange => weaponData != null ? weaponData.fullDamageRange : fullDamageRange;
    public bool AllowRicochet => weaponData != null ? weaponData.allowRicochet : allowRicochet;
    public float RicochetChance => weaponData != null ? weaponData.ricochetChance : ricochetChance;
    public float RicochetRange => weaponData != null ? weaponData.ricochetRange : ricochetRange;
    public float RicochetDamageMultiplier => weaponData != null ? weaponData.ricochetDamageMultiplier : ricochetDamageMultiplier;
    public float ScopedFov => (weaponData != null && weaponData.scopedFov > 0) ? weaponData.scopedFov : newFov;

    public AudioClip[] FireSounds => (weaponData != null && weaponData.fireSounds != null && weaponData.fireSounds.Length > 0) ? weaponData.fireSounds : fire;
    public AudioClip ReloadSound => (weaponData != null && weaponData.reloadSound != null) ? weaponData.reloadSound : reload;
    public AudioClip EmptySound => (weaponData != null && weaponData.emptyFireSound != null) ? weaponData.emptyFireSound : emptyMag;
    public AudioClip[] RicochetSounds => (weaponData != null && weaponData.ricochetSounds != null && weaponData.ricochetSounds.Length > 0) ? weaponData.ricochetSounds : ricochetSounds;
    public AudioClip[] EnemyHitSounds => (weaponData != null && weaponData.enemyHitSounds != null && weaponData.enemyHitSounds.Length > 0) ? weaponData.enemyHitSounds : enemyHit;
    public GameObject MuzzleFlashGraphic => (weaponData != null && weaponData.muzzleFlash != null) ? weaponData.muzzleFlash : muzzleFlash;
    public GameObject BulletHoleGraphicPrefab => (weaponData != null && weaponData.bulletHoleGraphic != null) ? weaponData.bulletHoleGraphic : bulletHoleGraphic;
    public TrailRenderer BulletTrailPrefab => (weaponData != null && weaponData.bulletTrail != null) ? weaponData.bulletTrail : BulletTrail;
    #endregion

    float oldFov = 60f;
    float newFov = 25f;

    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    UiMenager uiMenager;
    AudioManager audioManager;
    CameraControll cameraControll;
    public PlayerStats playerStats;
    SourceMovement playerMovement;

    [Header("SwietoscLevels")]
    public List<GameObject> revolverModels;
    public ParticleSystem upgradeParticle;

    [Header("Visuals and Sfx (Fallback)")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public AudioClip[] fire;
    public AudioClip reload;
    public AudioClip pullUp;
    public AudioClip pullDown;
    public AudioClip[] ricochetSounds;
    public AudioClip[] enemyHit;
    public AudioClip emptyMag;

    private bool isHitEffectRunning = false;
    GameManager gameManager;

    [SerializeField] private TrailRenderer BulletTrail;
    CinemachineCamera cineCam;

    //=== Events ===
    public UnityEvent<Vector3, float> OnTargetHit;

    private int lastKnownAmmo = -1;
    private int lastKnownBulletsLeft = -1;
    private float lastSwietosc = float.MinValue;

    private void Awake()
    {
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
            cineCam = FindFirstObjectByType<CinemachineCamera>();
        }
        if (cineCam != null)
        {
            oldFov = cineCam.Lens.FieldOfView;
        }

        bulletsLeft = MagazineSize;
        readyToShoot = true;
        allowShooting = true;
    }

    private void OnEnable()
    {
        readyToShoot = true;
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
        if (swietoscDependent)
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

        string displayName = (weaponData != null && !string.IsNullOrEmpty(weaponData.weaponName)) ? weaponData.weaponName : gameObject.name;

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
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && allowShooting)
        {
            if (CanBeScoped)
            {
                if (isScoped)
                {
                    Shoot();
                    if (transform.childCount > 0) transform.GetChild(0).gameObject.SetActive(true);
                    if (audioManager && FireSounds != null && FireSounds.Length > 0) audioManager.PlaySound(FireSounds);
                    if (animationController) animationController.Shot();

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
                bulletsShot = BulletsPerTap;
                Shoot();

                if (animationController != null && animationController.animator != null)
                {
                    animationController.animator.Play("Shoot");
                }

                if (audioManager && FireSounds != null && FireSounds.Length > 0)
                {
                    audioManager.PlaySound(FireSounds);
                }
            }
        }
        else if (readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            if (audioManager && EmptySound != null)
            {
                audioManager.PlaySound(EmptySound);
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
        readyToShoot = false;

        CalculateGunForce();
        Vector3 direction = CalculateRecoil();
        Vector3 shootOrigin = fpsCam != null ? fpsCam.transform.position : transform.position;

        // RayCast / SphereCast
        if (Physics.SphereCast(shootOrigin, ConeOfFire, direction, out rayHit, Range, whatIsEnemy))
        {
            // Mechanika Rykoszetu
            if (AllowRicochet && Random.value <= RicochetChance)
            {
                HandleRicochet(rayHit.point, rayHit.collider);
                if (audioManager && RicochetSounds != null && RicochetSounds.Length > 0)
                {
                    audioManager.PlaySound(RicochetSounds[Random.Range(0, RicochetSounds.Length)]);
                }
            }

            // Wizualizacja pocisku (Trail)
            if (BulletTrailPrefab != null && attackPoint != null)
            {
                TrailRenderer trail = Instantiate(BulletTrailPrefab, attackPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, rayHit));
            }

            // Zadawanie obrażeń i dziura po kuli
            ApplyDamage(rayHit.collider, direction, rayHit.point, Damage);
        }
        else
        {
            if (BulletTrailPrefab != null && attackPoint != null)
            {
                TrailRenderer trail = Instantiate(BulletTrailPrefab, attackPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, shootOrigin + direction * Range));
            }
        }

        if (cameraControll != null) cameraControll.ShootEffect();
        if (MuzzleFlashGraphic != null && attackPoint != null)
        {
            Instantiate(MuzzleFlashGraphic, attackPoint.position, attackPoint.rotation);
        }

        bulletsLeft--;
        bulletsShot--;
        UpdateUi();

        Invoke(nameof(ResetShot), TimeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke(nameof(Shoot), TimeBetweenShots);
        }
    }

    #region Damage
    void ApplyDamage(Collider target, Vector3 direction, Vector3 hitPoint, float baseDamage)
    {
        if (target == null) return;

        // Bullet hole decal na obiektach niebędących przeciwnikami/pociskami
        if (!target.CompareTag("Enemy") && !target.CompareTag("NPC") && !target.CompareTag("bullet"))
        {
            if (BulletHoleGraphicPrefab != null)
            {
                var hole = Instantiate(BulletHoleGraphicPrefab, hitPoint + (rayHit.normal * 0.025f), Quaternion.LookRotation(rayHit.normal));
                hole.transform.SetParent(target.transform, true);
            }
        }

        // Kalkulacja redukcji obrażeń z dystansem (Damage Falloff)
        float finalDamage = baseDamage;
        if (DamageRangeReduction)
        {
            Vector3 origin = fpsCam != null ? fpsCam.transform.position : transform.position;
            float distance = Vector3.Distance(origin, target.transform.position);
            if (distance > FullDamageRange)
            {
                finalDamage = baseDamage / Mathf.Clamp(distance - FullDamageRange, 1f, 100f);
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
        readyToShoot = true;
    }

    void PostProcessVisuals()
    {
        if (isHitEffectRunning || gameManager == null) return;

        if (gameManager.TryGetComponent<UnityEngine.Rendering.Volume>(out var volume) && volume.profile != null)
        {
            if (volume.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var colorAdjustments))
            {
                StartCoroutine(PulseEffectCoroutine(0.5f, 50, (currentValue) =>
                {
                    colorAdjustments.saturation.value = currentValue;
                    colorAdjustments.contrast.value = currentValue / 2;
                }));
            }
        }
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
            if (BulletTrailPrefab != null)
            {
                TrailRenderer trail = Instantiate(BulletTrailPrefab, startPoint, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, nearestEnemy.transform.position));
            }

            Vector3 dir = (nearestEnemy.transform.position - startPoint).normalized;
            ApplyDamage(nearestEnemy, dir, nearestEnemy.transform.position, Damage * RicochetDamageMultiplier);
        }
    }

    private IEnumerator PulseEffectCoroutine(float duration, float peakValue, System.Action<float> onUpdate)
    {
        isHitEffectRunning = true;
        float halfDuration = duration / 2.0f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            float currentValue = Mathf.Lerp(0f, peakValue, timer / halfDuration);
            onUpdate(currentValue);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            float currentValue = Mathf.Lerp(peakValue, 0f, timer / halfDuration);
            onUpdate(currentValue);
            timer += Time.deltaTime;
            yield return null;
        }

        onUpdate(0f);
        isHitEffectRunning = false;
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

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        if (trail == null) yield break;

        float time = 0f;
        Vector3 startPosition = trail.transform.position;

        while (time < 1f)
        {
            if (trail == null) yield break;
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        if (trail != null)
        {
            trail.transform.position = hit.point;
            Destroy(trail.gameObject, trail.time);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 endHit)
    {
        if (trail == null) yield break;

        float time = 0f;
        Vector3 startPosition = trail.transform.position;

        while (time < 1f)
        {
            if (trail == null) yield break;
            trail.transform.position = Vector3.Lerp(startPosition, endHit, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        if (trail != null)
        {
            trail.transform.position = endHit;
            Destroy(trail.gameObject, trail.time);
        }
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

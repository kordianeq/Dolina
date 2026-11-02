using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General Info")]
    public string weaponName = "Gun";
    public Sprite weaponIcon;

    [Header("Shooting Stats")]
    public float damage = 20f;
    [Tooltip("Time in seconds between consecutive shots/taps")]
    public float timeBetweenShooting = 0.3f;
    public float range = 100f;
    public int magazineSize = 6;
    public float reloadTime = 1f;
    public bool allowButtonHold = false;
    public float spread = 0f;
    public int bulletsPerTap = 1;
    [Tooltip("Delay between pellets/shots in burst fire (when bulletsPerTap > 1)")]
    public float timeBetweenShots = 0.05f;
    public float coneOfFire = 0.3f;
    public float force = 3f;
    public float recoilForce = 0f;

    [Header("Aim / Scope")]
    public bool canBeScoped = false;
    public float scopedFov = 25f;

    [Header("Damage Falloff")]
    public bool damageRangeReduction = false;
    public float fullDamageRange = 10f;

    [Header("Ricochet Settings")]
    public bool allowRicochet = false;
    [Range(0f, 1f)] public float ricochetChance = 0.05f;
    public float ricochetRange = 10f;
    public float ricochetDamageMultiplier = 0.5f;

    [Header("Audio Clips")]
    public AudioClip[] fireSounds;
    public AudioClip emptyFireSound;
    public AudioClip reloadSound;
    public AudioClip[] ricochetSounds;
    public AudioClip[] enemyHitSounds;

    [Header("Visual Effects")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public TrailRenderer bulletTrail;

    // Backward-compatibility aliases for earlier drafts
    public float fireRate => timeBetweenShooting;
    public bool isAutomatic => allowButtonHold;
    public float spreadAmount => spread;
    public int bulletsPerShot => bulletsPerTap;
    public bool damageFalloff => damageRangeReduction;
    public float falloffStartDistance => fullDamageRange;
}

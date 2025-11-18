using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    AnimationController animationController;
    //Gun stats
    [Header("Gun Stats")]
    public float damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap, ammo;
    public bool allowButtonHold, allowShooting, canBeScoped, damageRangeRedduction;
    public float fullDamageRange;
    public float recoilForce = 0;
    public float force;
    public float coneOfFire = 0.30f;
    [HideInInspector] public int bulletsLeft, bulletsShot;
   

    //bools 
    [HideInInspector] public bool shooting, readyToShoot, reloading, isScoped;

    [Header("Other Settings")]
    public bool hasStates;
    public bool customReload;
    public bool swietoscDependent;
    float oldFov;
    float NewFov = 25;

    //Reference
    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    UiMenager uiMenager;
    AudioManager audioManager;
    public PlayerStats playerStats;

    [Header("SwietoscLevels")]
    public List<GameObject> revolverModels;
    public ParticleSystem upgradeParticle;
    //Graphics
    [Header("Visuals and Sfx")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public AudioClip[] fire;
    public AudioClip reload;
    public AudioClip pullUp;
    public AudioClip pullDown;

    private bool isHitEffectRunning = false;

    GameManager gameManager;

    [SerializeField] private TrailRenderer BulletTrail;



    private void Awake()
    {
        gameManager = GameManager.Instance;
        bulletsLeft = magazineSize;
        readyToShoot = true;
        allowShooting = true;
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        animationController = GetComponentInChildren<AnimationController>();
        audioManager = GameObject.FindWithTag("audioManager").GetComponent<AudioManager>();
        fpsCam = Camera.main;
        oldFov = fpsCam.fieldOfView;
    }

    private void Start()
    {
        //trrzeba ogarnac
        gameManager = GameManager.Instance;
        bulletsLeft = magazineSize;
        readyToShoot = true;
        allowShooting = true;
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        animationController = GetComponentInChildren<AnimationController>();
        audioManager = GameObject.FindWithTag("audioManager").GetComponent<AudioManager>();
        fpsCam = Camera.main;
        oldFov = fpsCam.fieldOfView;
    }
    void Update()
    {
        if (swietoscDependent)
        {
            ChceckSwietosc();
        }

        MyInput();

        //SetText
        uiMenager.ammoText.SetText(bulletsLeft + " / " + magazineSize);
        uiMenager.gunName.SetText(gameObject.name);
        uiMenager.totalAmmoText.SetText(ammo.ToString());
    }
    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");


        if (Input.GetButtonDown("Reload") && bulletsLeft < magazineSize && !reloading)
        {
            Reload();

            if (animationController) animationController.Reload();
            //animationController.animator.SetBool("Reload", true);
            //audioManager.PlaySound(reload);
        }


        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && allowShooting)
        {
            if (canBeScoped)
            {
                if (isScoped)
                {
                    Shoot();
                    transform.GetChild(0).gameObject.SetActive(true);
                    if (audioManager) audioManager.PlaySound(fire);
                    if (animationController) animationController.Shot();

                    fpsCam.fieldOfView = oldFov;
                    uiMenager.scopePanel.SetActive(false);
                    //animationController.animator.SetBool(("None"), false);

                    Time.timeScale = 1f;
                    isScoped = false;
                }
                else
                {
                    transform.GetChild(0).gameObject.SetActive(false);
                    fpsCam.fieldOfView = NewFov;
                    uiMenager.scopePanel.SetActive(true);
                    isScoped = true;

                    Time.timeScale = 0.25f;
                    //animationController.animator.SetBool(("None"), true);
                }
            }
            else
            {

                bulletsShot = bulletsPerTap;

                Shoot();
                if (animationController)
                {
                    animationController.animator.Play("Shoot");
                }

                if (audioManager) audioManager.PlaySound(fire);
            }

        }
    }

    void ChceckSwietosc()
    {
        var swietosc = playerStats.swietosc;
        if (swietosc > -100 && swietosc <= -50)
        {
            int i = 0;
            foreach (GameObject model in revolverModels)
            {
                if (i == 0) model.SetActive(true);
                else model.SetActive(false);
                i++;
            }
        }
        else if (swietosc > -50 && swietosc <= 0)
        {
            int i = 0;
            foreach (GameObject model in revolverModels)
            {
                if (i == 1) model.SetActive(true);
                else model.SetActive(false);
                i++;
            }
        }
        else if (swietosc > 0 && swietosc <= 50)
        {
            int i = 0;
            foreach (GameObject model in revolverModels)
            {
                if (i == 2) model.SetActive(true);
                else model.SetActive(false);
                i++;
            }
        }
        else if (swietosc > 50 && swietosc <= 100)
        {
            int i = 0;
            foreach (GameObject model in revolverModels)
            {
                if (i == 3) model.SetActive(true);
                else model.SetActive(false);
                i++;
            }
        }
    }

    void CalculateGunForce()
    {
        if (recoilForce > 0)
            gameManager.PlayerRef.Launch(-transform.forward, recoilForce);
    }
    public void Shoot()
    {

        readyToShoot = false;

        CalculateGunForce();

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);


        //Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);



        //RayCast 
        if (Physics.SphereCast(fpsCam.transform.position, coneOfFire, direction, out rayHit, range, whatIsEnemy))
        {
            //Debug.Log(rayHit.collider.name);

            //graphics
            TrailRenderer trail = Instantiate(BulletTrail, attackPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, rayHit));

            //Spawn bullet hole graphic
            if (rayHit.collider.CompareTag("Enemy") || rayHit.collider.CompareTag("NPC") || rayHit.collider.CompareTag("bullet"))
            {

            }
            else
            {
                //Transform parent = transform.position, transform.rotation, transform.localScale);
                var hole = Instantiate(bulletHoleGraphic, rayHit.point + (rayHit.normal * 0.025f), Quaternion.LookRotation(rayHit.normal));
                hole.transform.SetParent(rayHit.collider.transform, true);
            }

            #region NewDamageSystemDisabled
            //New damage system
            //DISABLED
            /*
                            if (rayHit.transform.gameObject.TryGetComponent<Iidmgeable>(out Iidmgeable tryDmg))
                            {

                                if (damageRangeRedduction)
                                {
                                    float distance;
                                    distance = Vector3.Distance(fpsCam.transform.position, rayHit.collider.transform.position);
                                    if (fullDamageRange > distance)
                                    {
                                        // full damage

                                        tryDmg.TakeDmg(transform.forward, force, damage);
                                    }
                                    else
                                    {

                                        float reducedDamage = damage / Mathf.Clamp(distance - fullDamageRange, 1, 100);
                                        tryDmg.TakeDmg(transform.forward, force, damage);


                                        Debug.Log("Damage: " + reducedDamage + "Per pellet");


                                    }
                                }
                                else
                                {
                                    tryDmg.TakeDmg(transform.forward, force, damage);
                                }
                            }*/

            //Old damage system
            //lol, random update that i will not explain
            #endregion

            if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable enemy))
            {

                if (!enemy.Damaged(damage, direction.normalized, 1f))
                {

                    if (damageRangeRedduction)
                    {
                        float distance;
                        distance = Vector3.Distance(fpsCam.transform.position, rayHit.collider.transform.position);
                        if (fullDamageRange > distance)
                        {
                            // full damage
                            Debug.Log("Full damage applied");
                            enemy.Damaged(damage);
                        }
                        else
                        {

                            float reducedDamage = damage / Mathf.Clamp(distance - fullDamageRange, 1, 100);
                            enemy.Damaged(damage);


                            Debug.Log("Damage: " + reducedDamage + "Per pellet");
                        }
                    }
                    else
                    {
                        enemy.Damaged(damage);
                    }
                }
            }

        }
        else
        {
            //graphics
            TrailRenderer trail = Instantiate(BulletTrail, attackPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, fpsCam.transform.position + fpsCam.transform.forward * range));
        }



        //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);


        if (bulletsPerTap > 1)
        {
            if (bulletsShot == bulletsPerTap)
            {
                bulletsLeft--;

            }
            bulletsShot--;
        }
        else
        {
            bulletsLeft--;
            bulletsShot--;
        }



        Invoke("ResetShot", timeBetweenShooting);


        if (bulletsShot > 0 && bulletsLeft > 0) Invoke(nameof(Shoot), timeBetweenShots);

    }
    public void ResetShot()
    {
        readyToShoot = true;
    }

    void PostProcessVisuals()
    {
        if (isHitEffectRunning)
        {
            return; // Ignoruj to trafienie
        }

        if (GameObject.Find("Post-process Volume").GetComponent<UnityEngine.Rendering.Volume>().profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var colorAdjustments))
        {

            StartCoroutine(PulseEffectCoroutine(0.5f, 50, (currentValue) =>
            {
                // Ten kod jest wywoływany przez korutynę w każdej klatce animacji
                colorAdjustments.saturation.value = currentValue;
                colorAdjustments.contrast.value = currentValue / 2;
                //Debug.Log(currentValue);
            }));
        }
    }

    /// <summary>
    /// KORUTYNA: Animuje wartość od 0 do 8 i z powrotem do 0.
    /// </summary>
    /// <param name="duration">Całkowity czas trwania animacji</param>
    /// <param name="onUpdate">Akcja (funkcja) wywoływana co klatkę z aktualną wartością (od 0 do 8 i z powrotem)</param>
    private IEnumerator PulseEffectCoroutine(float duration, float peakValue, System.Action<float> onUpdate)
    {
        isHitEffectRunning = true;


        float halfDuration = duration / 2.0f;
        float timer = 0f;

        // --- Faza 1: Animacja z 0 do 8 ---
        while (timer < halfDuration)
        {
            // Mathf.Lerp liczy wartość pośrednią. timer / halfDuration daje nam postęp od 0.0 do 1.0
            float currentValue = Mathf.Lerp(0f, peakValue, timer / halfDuration);

            // Wywołujemy akcję (funkcję), którą podaliśmy w StartCoroutine, przekazując jej obliczoną wartość
            onUpdate(currentValue);

            // Zwiększamy timer o czas, jaki upłynął od ostatniej klatki
            timer += Time.deltaTime;

            // Kończymy tę klatkę i wracamy tu w następnej
            yield return null;
        }

        // --- Faza 2: Animacja z 8 do 0 ---
        timer = 0f; // Resetujemy timer
        while (timer < halfDuration)
        {
            float currentValue = Mathf.Lerp(peakValue, 0f, timer / halfDuration);
            onUpdate(currentValue);

            timer += Time.deltaTime;
            yield return null;
        }

        // --- Zakończenie ---
        // Upewniamy się, że na końcu wartość to DOKŁADNIE 0
        onUpdate(0f);

        isHitEffectRunning = false;
    }

    public void InstantReload()
    {
        bulletsLeft = magazineSize;
        ammo = ammo - magazineSize + bulletsLeft;

    }
    public void Reload()
    {
        if (customReload)
        {
            GetComponent<IReload>().Reloaded();
            reloading = true;
        }
        else
        {
            reloading = true;
            Invoke("ReloadFinished", reloadTime);

        }
        ammo = ammo - magazineSize + bulletsLeft;

    }
    public void ReloadFinished()
    {
        //animationController.animator.SetBool("Reload", false);
        bulletsLeft = magazineSize;
        reloading = false;
    }


    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0f;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1f)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }

        Trail.transform.position = Hit.point;

        Destroy(Trail.gameObject, Trail.time);
    }
    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 endHit)
    {
        float time = 0f;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1f)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, endHit, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }

        Trail.transform.position = endHit;

        Destroy(Trail.gameObject, Trail.time);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(fpsCam.transform.position + fpsCam.transform.forward * range, coneOfFire);
    }

    //public void Save(ref GunSaveData saveData, int gunId)
    //{
    //    saveData.ammo[gunId] = bulletsLeft;
    //}

    //public void Load(GunSaveData saveData, int gunId)
    //{
    //    bulletsLeft = saveData.ammo[gunId];

    //}
}
[System.Serializable]

public struct GunSaveData
{
    public List<int> ammo;
}



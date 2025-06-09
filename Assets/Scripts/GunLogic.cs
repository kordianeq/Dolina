using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GunSystem : MonoBehaviour
{
    AnimationController animationController;
    //Gun stats
    [Header("Gun Stats")]
    public float damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold, damageRangeRedduction, allowShooting, canBeScoped;
    public float fullDamageRange;

    public float force;
    [HideInInspector] public int bulletsLeft, bulletsShot;

    //bools 
    [HideInInspector] public bool shooting, readyToShoot, reloading, isScoped;

    public bool hasStates;
    public bool customReload;
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

    
    [SerializeField] private TrailRenderer BulletTrail;



    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        allowShooting = true;
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
    }

    private void Start()
    {
        animationController = GetComponentInChildren<AnimationController>();
        audioManager = GameObject.FindWithTag("audioManager").GetComponent<AudioManager>();
        fpsCam = Camera.main;
        oldFov = fpsCam.fieldOfView;

        
    }
    void Update()
    {
        
        ChceckSwietosc();

        MyInput();
        //SetText
        uiMenager.ammoText.SetText(bulletsLeft + " / " + magazineSize);
        uiMenager.gunName.SetText(gameObject.name);
    }
    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");


        if (Input.GetButtonDown("Reload") && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
            animationController.Reload();
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
                    audioManager.PlaySound(fire);
                    Shoot();
                    animationController.Shot();
                    fpsCam.fieldOfView = oldFov;
                    uiMenager.scopePanel.SetActive(false);
                    //animationController.animator.SetBool(("None"), false);
                    isScoped = false;
                }
                else
                {
                    fpsCam.fieldOfView = NewFov;
                    uiMenager.scopePanel.SetActive(true);
                    isScoped = true;
                    //animationController.animator.SetBool(("None"), true);
                    

                }
            }
            else
            {
                bulletsShot = bulletsPerTap;

                Shoot();
                animationController.Shot();
               // audioManager.PlaySound(fire);
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
    public void Shoot()
    {
        
        readyToShoot = false;


        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);


        //Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);



        //RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range))
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
                Instantiate(bulletHoleGraphic, rayHit.point + (rayHit.normal * 0.025f), Quaternion.LookRotation(rayHit.normal));
            }
                

            if (rayHit.collider.CompareTag("Enemy"))
            {
                //New damage system

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
                }

                //Old damage system
                if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable enemy))
                {
                    Debug.Log("Using old damage system");
                    if (damageRangeRedduction)
                    {
                        float distance;
                        distance = Vector3.Distance(fpsCam.transform.position, rayHit.collider.transform.position);
                        if (fullDamageRange > distance)
                        {
                            // full damage

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
            if (rayHit.collider.CompareTag("NPC"))
            {
                if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable npc))
                {
                    npc.Damaged(0);
                }
                else
                {
                    Debug.Log("NPC nie ma przypisanego dialogu po strzale");
                }
            }
        }




        
        

        //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);


        bulletsLeft--;
        bulletsShot--;


        Invoke("ResetShot", timeBetweenShooting);


        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }
    public void ResetShot()
    {
        readyToShoot = true;
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
            time += Time.deltaTime/Trail.time;

            yield return null;
        }

        Trail.transform.position = Hit.point;

        Destroy(Trail.gameObject, Trail.time);
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



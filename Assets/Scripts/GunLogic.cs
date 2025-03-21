using TMPro;
using UnityEngine;



public class GunSystem : MonoBehaviour
{
    AnimationController animationController; 
    //Gun stats
    public float damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold, damageRangeRedduction, allowShooting, canBeScoped;
    public float fullDamageRange;

    public bool customReload;
    public float force;
    [HideInInspector] public int bulletsLeft, bulletsShot;

    //bools 
    [HideInInspector] public bool shooting, readyToShoot, reloading, isScoped;

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

    //Graphics
    [Header("Visuals and Sfx")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public AudioClip[] fire;
    public AudioClip reload;
    public AudioClip pullUp;
    public AudioClip pullDown;






    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        allowShooting = true;
    }

    private void Start()
    {
        animationController = GameObject.Find("FpsAnim").GetComponent<AnimationController>();
        fpsCam = Camera.main;
        oldFov = fpsCam.fieldOfView;
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        audioManager = GameObject.FindWithTag("audioManager").GetComponent<AudioManager>();
    }
    void Update()
    {
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
            animationController.animator.SetBool("Reload", true);
            audioManager.PlaySound(reload);
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
                    animationController.animator.SetBool(("None"), false);
                    isScoped = false;
                }
                else
                {
                    fpsCam.fieldOfView = NewFov;
                    uiMenager.scopePanel.SetActive(true);
                    isScoped = true;
                    animationController.animator.SetBool(("None"), true);
                    

                }
            }
            else
            {
                bulletsShot = bulletsPerTap;

                Shoot();
                animationController.Shot();
                audioManager.PlaySound(fire);
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



            if (rayHit.collider.CompareTag("Enemy"))
            {
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

                if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable enemy))
                {

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




        //Graphics
        //Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
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
        animationController.animator.SetBool("Reload", false);
        bulletsLeft = magazineSize;
        reloading = false;
    }
}


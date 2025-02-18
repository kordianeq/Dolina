using TMPro;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public float damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold, damageRangeRedduction, allowShooting;
    public float fullDamageRange;
    int bulletsLeft, bulletsShot;
    public float force;


    //bools 
    bool shooting, readyToShoot, reloading;


    //Reference
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;


    //Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;


    TextMeshProUGUI text;


    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        allowShooting = true;
    }

    private void Start()
    {
        text = GameObject.Find("AmmoText").GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        MyInput();


        //SetText
        text.SetText(bulletsLeft + " / " + magazineSize);
    }
    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");


        if (Input.GetButtonDown("Reload") && bulletsLeft < magazineSize && !reloading) Reload();


        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && allowShooting)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
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
    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnpierRiffle : MonoBehaviour
{
    bool isScoped;
    Camera cam;
    UiMenager uiMenager;
    float oldFov;
    public float NewFov = 25f;
    public float damage = 10f, reloadTime, timeBetweenShooting, timeBetweenShots, range;

    bool readyToShoot, reloading;
    int bulletsLeft, bulletsShot;
    public int magazineSize, spread;

    public KeyCode fire = KeyCode.Mouse0;

    public RaycastHit rayHit;

    TextMeshProUGUI text;
    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    private void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        text = GameObject.Find("AmmoText").GetComponent<TextMeshProUGUI>();

        oldFov = cam.fieldOfView;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        if (Input.GetKeyDown(fire) && readyToShoot &&   !reloading && bulletsLeft > 0)
        {
            if (isScoped)
            {
                Shoot();
                cam.fieldOfView = oldFov;
                uiMenager.scopePanel.SetActive(false);
                isScoped = false;
            }
            else
            {
                cam.fieldOfView = NewFov;
                uiMenager.scopePanel.SetActive(true);
                isScoped = true;


            }

        }

        text.SetText(bulletsLeft + " / " + magazineSize);

    }

    private void Shoot()
    {
        readyToShoot = false;


        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);


        //Calculate Direction with Spread
        Vector3 direction = cam.transform.forward + new Vector3(x, y, 0);



        //RayCast
        if (Physics.Raycast(cam.transform.position, direction, out rayHit, range))
        {
            //Debug.Log(rayHit.collider.name);


            if (rayHit.collider.CompareTag("Enemy"))
            {
                if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable enemy))
                {
                     enemy.Damaged(damage);
                }
                //Damage

            }
            if (rayHit.collider.CompareTag("NPC"))
            {
                if (rayHit.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable npc))
                {
                    npc.Damaged(0);
                }
            }
        }

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

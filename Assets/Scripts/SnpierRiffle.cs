using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnpierRiffle : GunSystem
{
    bool isScoped;
    Camera cam;
    UiMenager uiMenager;
    float oldFov;
    public float NewFov = 25f;
    

    bool readyToShoot, reloading;
    int bulletsLeft, bulletsShot;
    

    public KeyCode fire = KeyCode.Mouse0;

    

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

   
}

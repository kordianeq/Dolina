using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class WeaponCore : MonoBehaviour
{
    //https://x.com/shitpost_2077/status/1839044567879799067
    public float weaponRange;
    public float minRange;
    public float maxRange;

    public float fireRate;
    public float fTime;
    public GameObject bullet;
    public Transform barrel;
    public float life;
    public void Shoot()
    {
        fTime+=Time.deltaTime;
        if(fTime>=fireRate)
        {
            fTime=0;
            GameObject b =Instantiate(bullet,barrel);
            Destroy(b,life);
        }

    }
}

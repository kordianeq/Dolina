using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buuuulet : MonoBehaviour, IKickeable
{
    // dont use this shit
    //only placeholder
    // Start is called before the first frame update
    public float flyspeed, damage;
    public Vector3 forw;
    bool good;
    void Start()
    {
        forw = transform.forward;
        good = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= forw * Time.deltaTime * flyspeed;
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    GetComponentInChildren<IDamagable>().Damaged(damage);

        //}
        //Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (good)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.GetComponentInParent<IDamagable>().Damaged(damage);

            }
        }
        else
        { 
            if (other.gameObject.CompareTag("Enemy"))
            {
                other.GetComponentInParent<IDamagable>().Damaged(damage);

            }
        }
        //Destroy(gameObject);
        }

    public bool KickHandleButMorePrecize(Vector3 from)
    {
        forw = from - transform.position;
        good = true;
        return true;
    }

    public void KickHandle()
    {
        Destroy(gameObject);
    }
}

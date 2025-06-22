using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buuuulet : MonoBehaviour, IKickeable
{
    [SerializeField] GameObject particleEffect;
    // dont use this shit
    //only placeholder
    // Start is called before the first frame update
    public float flyspeed, damage;
    public Vector3 forw;
    public bool good;
    bool hitDetect;
    RaycastHit hit;
    public float radiu, rang;
    public LayerMask HitMask;
    void Start()
    {
        forw = transform.forward;
        Destroy(gameObject, 5f);
        //good = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= forw * Time.deltaTime * flyspeed;

        transform.rotation = Quaternion.LookRotation(forw);


        // i dont know how to codeeeee 
        // soryy
        hitDetect = Physics.SphereCast(transform.position, radiu, transform.forward, out hit, rang, HitMask);

        if (hitDetect)
        {
            //Output the name of the Collider your Box hit
            Debug.Log("Hit : " + hit.collider.name+ hit.collider.tag);

            if (good)
            {
                Debug.Log("BulletIsGood");
                if (!hit.collider.transform.gameObject.CompareTag("Enemy"))
                {
                    if (hit.transform.TryGetComponent<IDamagable>(out IDamagable trydamage))
                    {
                        trydamage.Damaged(damage);
                    }
                    //hit.transform.GetComponentInParent<IDamagable>().Damaged(damage);
                    if (particleEffect != null)
                    {
                        GameObject p = Instantiate(particleEffect, transform.position, transform.rotation);
                        p.transform.parent = null;
                        Destroy(p, 1f);
                    }

                    Destroy(gameObject);

                }
            }
            else
            {
                if (hit.transform.TryGetComponent<IDamagable>(out IDamagable trydamage))
                {
                    trydamage.Damaged(damage);
                }

                //hit.transform.GetComponentInParent<IDamagable>().Damaged(damage);

                if (particleEffect != null)
                {
                    GameObject p = Instantiate(particleEffect, transform.position, transform.rotation);
                    p.transform.parent = null;
                    Destroy(p, 1f);
                }
                Destroy(gameObject);




            }
            /*if (good)
            {
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    hit.transform.GetComponentInParent<IDamagable>().Damaged(damage);
                    Destroy(gameObject);
                }
            }
            else
            {
                if (hit.transform.gameObject.CompareTag("Enemy"))
                {
                    hit.transform.GetComponentInParent<IDamagable>().Damaged(damage);
                    Destroy(gameObject);

                }
            }*/
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    GetComponentInChildren<IDamagable>().Damaged(damage);

        //}
        //Destroy(gameObject);
    }

    void FixedUpdate()
    {
        //Raycast i guess

    }

    /*private void OnTriggerEnter(Collider other)
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
    }*/

    public bool kickHandle(Vector3 from, float force)
    {
        Debug.Log("Kicken");
        forw = from - transform.position;
        forw = Vector3.ProjectOnPlane(forw, Vector3.up).normalized;
        //transform.LookAt(forw);
        good = false;
        return true;
    }

    public void KickHandle()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radiu);



    }
}

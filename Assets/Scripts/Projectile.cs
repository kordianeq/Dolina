using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    public GameObject afterExplosion;
    SphereCollider colision;
    HandPowers handPowersRef;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        colision = GetComponent<SphereCollider>();
        handPowersRef = FindAnyObjectByType<HandPowers>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        rb.isKinematic= true;
        //rb.velocity = Vector3.zero;
        colision.isTrigger = true;
        colision.radius = 10f;


        //

        Invoke(nameof(KillParticle), 0.4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyLogic>().ApplyDamage(handPowersRef.explosionDamage);
        }
    }
    void KillParticle()
    {
        Instantiate(afterExplosion, rb.position, rb.rotation);
        Destroy(gameObject);
    }
}

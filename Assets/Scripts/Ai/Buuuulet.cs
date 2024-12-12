using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buuuulet : MonoBehaviour
{
    // dont use this shit
    //only placeholder
    // Start is called before the first frame update
    public float flyspeed,damage;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= transform.forward * Time.deltaTime * flyspeed;
        Destroy(gameObject,5f);
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
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponentInParent<IDamagable>().Damaged(damage);

        }
        //Destroy(gameObject);
    }
}

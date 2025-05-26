using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugBreakDoor : MonoBehaviour,  IKickeable
{
    Rigidbody rigidb;
    bool Unhinged = false;
    public float UnhingeForce;

    public float lethalVelocity;
    public float flyTime;
    float flyTimer = 0;

    private void Start() {
        rigidb = GetComponent<Rigidbody>();
        rigidb.isKinematic = true;
    }

    private void FixedUpdate() {
        
    }

    public void KickHandle()
    {
       Destroy(this.gameObject);
    }

    public bool KickHandleButMorePrecize(Vector3 from)
    {
        if(Unhinged)
        {
            return false;
        }else
        {
            Unhinged = true;
            rigidb.isKinematic = false;
            StartCoroutine(Fly());
            rigidb.AddForce(Vector3.ProjectOnPlane( transform.position - from,Vector3.up).normalized * UnhingeForce,ForceMode.Impulse);
            

            return true;
        }
    }

    IEnumerator Fly()
    {
        rigidb.useGravity = false;
        yield return new WaitForSeconds(flyTime);
        rigidb.useGravity = true;
        yield return null;
    }


    private void OnTriggerEnter(Collider other) {
        
    }
}

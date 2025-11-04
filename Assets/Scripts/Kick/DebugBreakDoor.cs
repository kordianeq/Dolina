using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;


public class DebugBreakDoor : MonoBehaviour,  IKickeable, IDamagable
{
    
    Rigidbody rigidb;
    bool Unhinged = false;
    public float UnhingeForce;

    public float lethalVelocity;
    public float doorDmg;
    public float flyTime;
    float flyTimer = 0;
    [SerializeField]GameObject OnDestroyChunk;

    private void Start()
    {
        rigidb = GetComponent<Rigidbody>();
        rigidb.isKinematic = true;
    }

    private void FixedUpdate() {
        
    }

 
    public void KickHandle()
    {
        Break();

    }
    void Break()
    { 
        var chunk = Instantiate(OnDestroyChunk,transform.position,transform.rotation);
        chunk.GetComponent<ChunkMaker>().GoAndBreak(GetComponent<Rigidbody>());

        Destroy(this.gameObject);
    }
    public void Damaged(float dmg)
    {
        Break();
    }

    public bool kickHandle(Vector3 from, float kickForce)
    {
        if (Unhinged)
        {
            return false;
        }
        else
        {
            Unhinged = true;
            rigidb.isKinematic = false;
            StartCoroutine(Fly());
            rigidb.AddForce(Vector3.ProjectOnPlane(transform.position - from, Vector3.up).normalized * UnhingeForce, ForceMode.Impulse);


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
        if (other.tag != "Default")
        {
            if (rigidb.linearVelocity.magnitude > lethalVelocity)
            {
                if (other.gameObject.TryGetComponent<IDamagable>(out IDamagable tryDmg))
                {
                    tryDmg.Damaged(doorDmg);
                    
                }
                
                Break();
            }
        }
    }
}

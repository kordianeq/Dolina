
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Breakeable : MonoBehaviour, Iidmgeable, IiBoomeable, IDamagable, IKickeable
{
    public float KickForces;
    public bool NotBr;
    public bool BulletBreakeable;
    public float hp;
    public float breakpower;
    public bool fuse;
    public float fuseTime;
    public float breakVelo;
    public ParticleSystem part;
    public bool hasfuse;
    public bool invc = false;
    public GameObject doWhenBroken;// wip, will make this an abstact class or smth idk
    public GameObject chunkSpwn;

    Rigidbody rb;
    // Start is called before the first frame update

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //OLD DMG SYSTEM, WAIT NO, NEW ENEMY SYSTEM BUT OLD SYSTEM IS NOW THE DEFAULT ONE
    public void TakeDmg(Vector3 dmgDir, float dmgPower, float dmg)
    {
        //Debug.Log("CALL AN AMBULANCE");
        if (BulletBreakeable)
        {
            hp -= dmg;
            if (hp <= 0)
            { Break(); }
        }
        //Vector3 dmgdir = transform.position-dmgDir;
        //dmgdir.y = 0;       
        //moveBrain.AddDirectionalTorque(dmgDir.normalized,dmgPower*0.1f,ForceMode.Impulse);
    }

    public void Damaged(float dmg)
    {
        if (BulletBreakeable)
        {
            hp -= dmg;
            if (hp <= 0)
            { Break(); }
        }
    }

    public void KickHandle()
    {
        //FUSE

        if (hasfuse)
        {
            Fused();
        }else
        {
            Break();
        }
        


    }

    public bool KickHandleButMorePrecize(Vector3 from)
    {
        Vector3 flattened = Vector3.ProjectOnPlane(transform.position - from, Vector3.up);
        rb.AddForce(flattened*KickForces,ForceMode.Impulse);
        rb.AddForce(Vector3.up*2,ForceMode.Impulse);
        if (hasfuse)
        {
            Fused();
        }
        else if (NotBr)
        {

        }
        else
        {
            Break();
        }

        return true;
        
/*  
                if (hasfuse)
                {
                    Fused();
                }
                else
                {
                    Break();
                }*/
    }

    public void MakeBoom(float dmg)
    {
        Break();
    }

    void Fused()
    {
        if (fuse && invc)
        {
            Break();
        }
        else
        {
            fuse = true;
            StartCoroutine(SetFuse());
        }
        
    }
    IEnumerator SetFuse()
    {
        part.Play();
        Debug.Log("aaaaa");
        yield return new WaitForSeconds(0.25f);
        invc = true;
        yield return new WaitForSeconds(fuseTime - 0.25f);
        Debug.Log("bbbb");
        Break();
        yield return null;
    }

    public void Break()
    {
        if (doWhenBroken != null)
        {
            Instantiate(doWhenBroken, transform.position, transform.rotation);
        }

        if (chunkSpwn != null)
        {
            GameObject Chk = Instantiate(chunkSpwn, transform.position, transform.rotation);
            var allchk = Chk.GetComponentsInChildren<Rigidbody>();
            Destroy(Chk, Random.Range(3f, 5f));
            foreach (var rb in allchk)
            {
                //Debug.Log("sigma rizz");
                rb.AddForce(Random.insideUnitSphere * breakpower, ForceMode.Impulse);
                //rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            }
        }
        Destroy(gameObject);
    }


    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(GetComponent<Rigidbody>().velocity.magnitude);
        if (rb.velocity.magnitude > breakVelo)
        {
            Break();
        }
    }
}

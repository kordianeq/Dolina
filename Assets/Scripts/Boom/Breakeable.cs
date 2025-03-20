
using Unity.VisualScripting;
using UnityEngine;

public class Breakeable : MonoBehaviour, Iidmgeable, IiBoomeable
{
    public bool BulletBreakeable;
    public float hp;
    public float breakpower;

    public GameObject doWhenBroken;// wip, will make this an abstact class or smth idk
    public GameObject chunkSpwn;

    // Start is called before the first frame update
    public void TakeDmg(Vector3 dmgDir, float dmgPower, float dmg)
    {
        
        //Debug.Log("CALL AN AMBULANCE");
        if(BulletBreakeable)
        {
            
            
            hp -= dmg;
            if(hp<= 0)
            {Break();}

        }
        //Vector3 dmgdir = transform.position-dmgDir;
        //dmgdir.y = 0;
        
        //moveBrain.AddDirectionalTorque(dmgDir.normalized,dmgPower*0.1f,ForceMode.Impulse);
    }

    public void MakeBoom(float dmg)
    {
       Break();
    }

    public void Break()
    {
        if(doWhenBroken!= null)
        {
            Instantiate(doWhenBroken,transform.position, transform.rotation);
        }

        if(chunkSpwn!= null)
        {
            GameObject Chk = Instantiate(chunkSpwn,transform.position, transform.rotation);
            var allchk = Chk.GetComponentsInChildren<Rigidbody>();
            foreach(var rb in allchk)
            {
                //Debug.Log("sigma rizz");
                rb.AddForce(Random.insideUnitSphere * breakpower,ForceMode.Impulse);
                //rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            }
        }
        Destroy(gameObject);   
    }
}

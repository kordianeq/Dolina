using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TableFlipper : MonoBehaviour, IKickeable, IDamagable
{
    [SerializeField] Transform adjust;
    [SerializeField] Transform animAdjust;
    [SerializeField] Transform boneAdjust;
    [SerializeField] Animator anim;
    [SerializeField]Rigidbody rb;
    [SerializeField] float kickForce;
    [SerializeField] float hp,dmg;
    [SerializeField] GameObject destr;
    bool oneTime = true;

    // Start is called before the first frame update
    void Start()
    {

        anim.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void KickHandle()
    {

    }
    public bool kickHandle(Vector3 from, float spd)
    {
        if (oneTime == true)
        {
            oneTime = false;
            anim.speed = 1;
            //Debug.Log("Kicked from angle: " + Vector3.Angle(Vector3.ProjectOnPlane(from, Vector3.up)-Vector3.ProjectOnPlane(transform.position,Vector3.up),transform.forward));
            float kickAngle = Vector3.SignedAngle((Vector3.ProjectOnPlane(adjust.position, Vector3.up) - new Vector3(from.x, 0, from.z)).normalized, adjust.transform.forward, Vector3.up);
            Debug.Log("Kicked from angle: " + kickAngle);
            if (math.abs(kickAngle) <= 45)
            {

                Debug.Log("Kick Forward");
                anim.SetTrigger("F");
                animAdjust.eulerAngles = new Vector3(-90, 0f, 0);
                boneAdjust.eulerAngles = new Vector3(0, 0, 0);

            }
            else if (math.abs(kickAngle) > 45 && math.abs(kickAngle) <= 135)
            {
                if (kickAngle < 0)
                {
                    Debug.Log("Kick Left");
                    anim.SetTrigger("L");
                    animAdjust.eulerAngles = new Vector3(-90, 0f, 0);
                    boneAdjust.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    Debug.Log("Kick Right");

                    anim.SetTrigger("L");
                    animAdjust.eulerAngles = new Vector3(-90, 180f, 0);
                    boneAdjust.eulerAngles = new Vector3(0, 0, 0);
                }
            }
            else
            {
                Debug.Log("Kick BACK");
                anim.SetTrigger("F");
                animAdjust.eulerAngles = new Vector3(-90, 180f, 0);
                boneAdjust.eulerAngles = new Vector3(0, 180, 0);
            }
        }
        else
        {
            rb.AddForce(Vector3.ProjectOnPlane(adjust.transform.position - from, Vector3.up).normalized * kickForce, ForceMode.Impulse);
            rb.AddForce(Vector3.up * 4,ForceMode.Impulse);
        }
        return true;
    }
    
    void Break()
    { 
        var chunk = Instantiate(destr,transform.position,transform.rotation);
        chunk.GetComponent<ChunkMaker>().GoAndBreak(rb);

        Destroy(animAdjust.gameObject);
    }

    public void Damaged(float dmg)
    {
        
            hp -= dmg;
            if (hp <= 0)
            { Break(); }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Default")
        {
            if (rb.linearVelocity.magnitude > 5)
            {
                if (other.gameObject.TryGetComponent<IDamagable>(out IDamagable tryDmg))
                {
                    tryDmg.Damaged(dmg);

                }
                hp -= 25;
                //Break();
            }
        }
    }
    
}

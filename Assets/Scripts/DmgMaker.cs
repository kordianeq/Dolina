using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DmgMaker : MonoBehaviour
{
    // Start is called before the first frame update
   RaycastHit hit;
   public LayerMask shootmask;
   public float dmg, force;
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.DrawRay(transform.position, transform.forward*100,Color.red,5f);
            //Debug.Log("pew pew");
            //Debug.Log(Physics.Raycast(transform.position, Vector3.forward, out hit, Mathf.Infinity, shootmask));
            if(Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, shootmask))
            {

                //Debug.Log("hit");
                if(hit.transform.gameObject!=null)
                {
                    hit.transform.gameObject.TryGetComponent<Iidmgeable>(out Iidmgeable tryDmg);
                    tryDmg.TakeDmg(transform.forward,force,dmg);
                }
                
                
                
            }else
            {
                //Debug.Log("Not hit");
            }
        }
    }

    //TryGetComponent<ITakeable>(out ITakeable pickup))
}

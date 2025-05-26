using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugKick : MonoBehaviour, IKickeable
{
    // Start is called before the first frame update

    public bool eject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void KickHandle()
    {


        Debug.Log("uuu sigma");
        GetComponent<Rigidbody>().AddForce(Vector3.up*10f,ForceMode.Impulse);
    }

    public bool KickHandleButMorePrecize()
    {
        if(eject)
        {
            return true;
        }else
        {
            return false;
        }
    }
}

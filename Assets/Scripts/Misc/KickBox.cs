using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickBox : HurtBox, IKickeable
{

    public void KickHandle()
    {

    }

    public bool KickHandleButMorePrecize()
    {
        ParentRecreceiver.GetComponent<DmgMannager>();
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKickeable
{
    void KickHandle();

    public bool kickHandle(Vector3 from,float kickForce)
    {
        return false;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKickeable
{
    void KickHandle();

    public bool KickHandleButMorePrecize(Vector3 from)
    {
        return false;
    }

}

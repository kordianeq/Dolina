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
    public bool kickHandle(Vector3 from, float kickForce, float dmg)
    {
        return false;
    }
    public bool kickHandle(Vector3 from, float kickForce, float dmg, bool inflictBleed)
    {
        return false;
    }
}

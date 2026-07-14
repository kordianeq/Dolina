using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void Damaged(float damage);

    public bool Damaged(float damage,Vector3 dir,float force)
    {
        return false;
    }
}

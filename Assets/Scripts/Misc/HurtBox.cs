using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class HurtBox : MonoBehaviour
{
    [SerializeField] bool critical;
    [SerializeField] protected GameObject ParentRecreceiver;
    // Start is called before the first frame update
    void OnDrawGizmosSelected()
    {
        if (critical)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}

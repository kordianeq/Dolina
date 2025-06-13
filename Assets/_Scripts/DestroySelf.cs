using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour, IInteracted
{
    // Start is called before the first frame update
    public void NewInteraction()
    {
        Destroy(gameObject);
    }
}

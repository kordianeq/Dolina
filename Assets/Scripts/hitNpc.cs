using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitNpc : MonoBehaviour, IDamagable
{

    public GameObject npcDialogue;
    public void Damaged(float damage)
    {
        npcDialogue.GetComponent<IInteracted>().NewInteraction();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healingItem : MonoBehaviour
{
   
    public float healAmount = 20f; // Amount of health to restore
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerStats>().healingItems++;
            Destroy(gameObject); // Destroy the healing item after pickup
        }
        
    }

}

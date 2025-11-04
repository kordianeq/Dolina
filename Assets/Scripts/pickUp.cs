using UnityEngine;

public enum ItemType
{
    Health,
    Ammo,
    PowerUp
}
public class pickUp : MonoBehaviour
{
    public ItemType itemType;
    public float ammount;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add logic for what happens when the player picks up the item
            Debug.Log("Item picked up!");
            switch(itemType) // Example: Change ItemType as needed
            {
                case ItemType.Health:
                    // Increase player's health
                    Debug.Log("Health picked up!");
                    other.GetComponentInParent<PlayerStats>().playerHp += ammount;
                    break;
                case ItemType.Ammo:
                    // Increase player's ammo
                    
                    
                    break;
                case ItemType.PowerUp:
                    // Grant player a power-up
                    break;
            }
            // Destroy the pickup object after being collected
            Destroy(gameObject);
        }
    }
}

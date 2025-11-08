using UnityEngine;

public enum ItemType
{
    Health,
    Ammo,
    Throwable,
    PowerUp
}

public enum AmmoType
{
    Revolver,
    Shotgun,
    Sniper
}
public class pickUp : MonoBehaviour
{
    public ItemType itemType;


    public float ammount;

    [Header("Ammo Type (Only for Ammo pickups)")]
    public AmmoType ammoType;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add logic for what happens when the player picks up the item
            Debug.Log("Item picked up!");
            switch (itemType) // Example: Change ItemType as needed
            {
                case ItemType.Health:
                    // Increase player's health
                    Debug.Log("Health picked up!");
                    other.GetComponentInParent<PlayerStats>().playerHp += ammount;
                    break;

                case ItemType.Ammo:
                    // Increase player's ammo
                    switch (ammoType)
                    {
                        case AmmoType.Revolver:
                            other.GetComponentInParent<GunSystem>().ammo += (int)ammount;
                            break;
                        case AmmoType.Shotgun:
                            other.GetComponentInParent<GunSystem>().ammo += (int)ammount;
                            break;
                        case AmmoType.Sniper:
                            other.GetComponentInParent<GunSystem>().ammo += (int)ammount;
                            break;
                        default:
                            break;
                    }

                    break;
                case ItemType.PowerUp:
                    // Grant player a power-up
                    break;
                case ItemType.Throwable:
                    // Increase player's throwable count
                    other.GetComponentInParent<PlayerStats>().throwablesCount += (int)ammount;
                    GameManager.Instance.UpdateThrowablesCount();
                    break;
            }
            // Destroy the pickup object after being collected
            Destroy(gameObject);
        }
    }
}

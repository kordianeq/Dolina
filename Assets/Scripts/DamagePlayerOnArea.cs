using UnityEngine;

public class DamagePlayerOnArea : MonoBehaviour
{

    public int damage;
    public float tick;
    bool repeat = true;


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && repeat)
        {
            IDamagable playerStats = other.gameObject.GetComponentInParent<IDamagable>();

            repeat = false;
            playerStats.Damaged(damage);

            Invoke(nameof(ResetTimer), tick);




        }

    }

    void ResetTimer()
    {
        repeat = true;
    }
}

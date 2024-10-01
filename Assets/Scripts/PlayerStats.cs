using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // Start is called before the first frame update
    float dmgreduction;
    public float playerHp;
    public float maxPlayerHp;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayerDamaged(int damage)
    {
        if (dmgreduction > 0)
        {
            damage -= (int)(damage * dmgreduction);
        }
        playerHp -= damage;
    }

    public void DamageReduction(float reductionProcentage)
    {
        dmgreduction = reductionProcentage;
    }
}

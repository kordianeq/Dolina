using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour,IDamagable
{
    // Start is called before the first frame update
    float dmgreduction;
    public float playerHp;
    public float maxPlayerHp;
    sliderScript swietoscSlid;
    sliderScript hpSlid;
    TextMeshProUGUI hpText;
    public float swietosc;
    void Start()
    {
        swietoscSlid = GameObject.Find("SwietoscSlider").GetComponent<sliderScript>();
        hpSlid = GameObject.Find("HpSlider").GetComponent<sliderScript>();
        hpText = GameObject.Find("HpText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        swietoscSlid.value = swietosc;
        hpSlid.value = playerHp;
        hpText.text = playerHp.ToString();
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

    public void Damaged(float damage)
    {
        playerHp -= damage;
    }
}

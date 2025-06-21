using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamagable
{
    // Start is called before the first frame update

    float dmgreduction;
    //public Transform playerTransform;
    public float playerHp;
    public float maxPlayerHp;
    public float swietosc;

    [Header("Healing")]
    public int healingItems = 0;
    public float healAmmout = 20f; // Amount of health restored by healing items


    public bool isDead = false;
    sliderScript swietoscSlid;
    sliderScript hpSlid;
    TextMeshProUGUI hpText;
    TextMeshProUGUI healingItemsText;
    void Start()
    {
        //playerTransform = GetComponent<Transform>();
        swietoscSlid = GameObject.Find("SwietoscSlider").GetComponent<sliderScript>();
        hpSlid = GameObject.Find("HpSlider").GetComponent<sliderScript>();
        hpText = GameObject.Find("HpText").GetComponent<TextMeshProUGUI>();
        healingItemsText = GameObject.Find("HealingItemsText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Heal"))
        {
            HealPlayer(healAmmout); // Example heal amount, can be changed or made dynamic
        }
        swietoscSlid.value = swietosc;
        hpSlid.value = playerHp;
        hpText.text = playerHp.ToString();
        healingItemsText.text = healingItems.ToString();    
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
        Debug.Log("damage" + damage);
        playerHp -= damage;
    }

    public void HealPlayer(float healAmmout)
    {
        if(healingItems > 0)
        {
            
            //Play animation or sound for healing

            if (playerHp + healAmmout > maxPlayerHp)
            {
                healAmmout = maxPlayerHp - playerHp;
            }
            if( playerHp == maxPlayerHp)
            {
                Debug.Log("Player already at max health");
                return;
            }
            playerHp += healAmmout;

            healingItems--;
        }
        else
        {
            Debug.Log("No healing items available");
        }

    }
    public void Save(ref PlayerSaveData saveData)
    {
        saveData.position = transform.position;
        saveData.playerHp = playerHp;
    }

    public void Load(PlayerSaveData saveData)
    {
        transform.position = saveData.position;
        playerHp = saveData.playerHp;
    }
}
[System.Serializable]

public struct PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float playerHp;
}
using TMPro;
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour,IDamagable
{
    // Start is called before the first frame update

    float dmgreduction;
    //public Transform playerTransform;
    public float playerHp;
    public float maxPlayerHp;
    public float swietosc;
    public bool infiniteThrows;
    public int throwablesCount;
    public bool isDead = false;
    sliderScript swietoscSlid;
    sliderScript hpSlid;
    TextMeshProUGUI hpText;
    UiMenager uiMenager;

    public event Action OnPlayerDeath;

    void Start()
    {
        
    }

    void Awake()
    {
        //playerTransform = GetComponent<Transform>();
        swietoscSlid = GameObject.Find("SwietoscSlider").GetComponent<sliderScript>();
        hpSlid = GameObject.Find("HpSlider").GetComponent<sliderScript>();
        hpText = GameObject.Find("HpText").GetComponent<TextMeshProUGUI>();
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        uiMenager.UpdateThrowableCount(throwablesCount);
        playerHp = maxPlayerHp;
    }

    // Update is called once per frame
    void Update()
    {
        swietoscSlid.value = swietosc;
        hpSlid.value = playerHp;
        hpText.text = playerHp.ToString();
    }

    public void DamageReduction(float reductionProcentage)
    {
        dmgreduction = reductionProcentage;
    }

    public void Damaged(float damage)
    {
        if (isDead) return;

        if (dmgreduction > 0)
        {
            damage -= (int)(damage * dmgreduction);
        }

        playerHp -= damage;
       

        if (playerHp <= 0)
        {
            
            isDead = true;
            Death();
        }
        
        uiMenager.damageOverlayScript.Damaged();
    }

    public void Death()
    {
        isDead = true;
        OnPlayerDeath?.Invoke();
        Debug.Log("Player has died.");
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
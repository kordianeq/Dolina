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
    public bool godMode = false;
    sliderScript swietoscSlid;
    sliderScript hpSlid;
    TextMeshProUGUI hpText;
    UiMenager uiMenager;

    public Ability abilitySlot;

    public event Action OnPlayerDeath;

    void Start()
    {
        
    }

    void Awake()
    {
        //playerTransform = GetComponent<Transform>();
        swietoscSlid = GameObject.Find("SwietoscSlider").GetComponent<sliderScript>();
        //hpSlid = GameObject.Find("HpSlider").GetComponent<sliderScript>();
        hpText = GameObject.Find("HpText").GetComponent<TextMeshProUGUI>();
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        uiMenager.UpdateThrowableCount(throwablesCount);
        playerHp = maxPlayerHp;
    }

    // Update is called once per frame
    void Update()
    {
        swietoscSlid.value = swietosc;
        //hpSlid.value = playerHp;
        hpText.text = playerHp.ToString();
    }

    public void DamageReduction(float reductionProcentage)
    {
        dmgreduction = reductionProcentage;
    }

    public void Damaged(float damage)
    {
        if (isDead) return;
        if(godMode) return;
        
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
        if(abilitySlot.GetType() == typeof(UndyingTotem) && abilitySlot._isAbilityActive)
        {
            Debug.Log("Undying Totem activated. Player revived.");
            isDead = false;
            abilitySlot.ActivateAbility();
            return;
        }
       
        isDead = true;
        OnPlayerDeath?.Invoke();
        Debug.Log("Player has died.");
    }

    public void Save(ref PlayerSaveData saveData)
    {
        saveData.position = transform.position;
        saveData.rotation = transform.rotation;
        saveData.playerHp = playerHp;
    }

    public void Load(PlayerSaveData saveData)
    {
        transform.position = saveData.position;
        if (saveData.rotation != Quaternion.identity)
            transform.rotation = saveData.rotation;

        playerHp = saveData.playerHp > 0 ? saveData.playerHp : maxPlayerHp;
        isDead = false;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerStatus(PlayerState.Normal);
            if (GameManager.Instance.UiMenager != null && GameManager.Instance.UiMenager.deathPanel != null)
            {
                GameManager.Instance.UiMenager.deathPanel.SetActive(false);
            }
        }
    }
}
[System.Serializable]

public struct PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public float playerHp;
}
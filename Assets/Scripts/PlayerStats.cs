using TMPro;
using System;
using UnityEngine;
using UnityEngine.Events;

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
    [Header("Sounds")]
    AudioManager audioManager;
    public AudioClip[] damageSounds;
    public AudioClip[] deathSounds;


    public Ability abilitySlot;

    public event Action OnPlayerDeath;

    void Start()
    {
        audioManager = AudioManager.Instance;
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

    public void AddSwietosc(float amount)
    {
        swietosc = Mathf.Clamp(swietosc + amount, -100f, 100f);
        var lookCtrl = FindFirstObjectByType<LookController>();
        if (lookCtrl != null)
        {
            lookCtrl.swietosc = swietosc;
        }
        if (swietoscSlid != null)
        {
            swietoscSlid.value = swietosc;
        }
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

        if (audioManager != null) audioManager.PlaySound(damageSounds);

        if (playerHp <= 0)
        {
            isDead = true;
            Death();
            return;
        }
        
        if (uiMenager != null && uiMenager.damageOverlayScript != null)
        {
            uiMenager.damageOverlayScript.Damaged();
        }
    }

    public void Death()
    {
        // Sprawdzamy czy totem istnieje, czy komponent jest włączony w inspektorze i aktywny w grze
        if (abilitySlot != null && 
            abilitySlot.enabled && 
            abilitySlot.gameObject.activeInHierarchy && 
            abilitySlot._isAbilityActive && 
            abilitySlot is UndyingTotem totem)
        {
            Debug.Log("Undying Totem activated. Player revived.");
            isDead = false;
            totem.ActivateAbility();
            return;
        }

        isDead = true;

        if (OnPlayerDeath != null)
        {
            OnPlayerDeath.Invoke();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePlayerDeath();
        }

        Debug.Log("Player has died.");
        if (deathSounds != null && audioManager != null) audioManager.PlaySound(deathSounds);
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
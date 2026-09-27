using NUnit.Framework;
using UnityEngine;
[RequireComponent(typeof(PlayerStats))]
public abstract class Ability : MonoBehaviour
{
    public GameManager gameManager;
    public UiMenager uiMenager;
    public Animator _animator;
    private WeaponSwap weaponController;
    public PlayerStats _playerStats;
    [Header("Ability Settings")]
    public bool isPassive;
    public bool isWeaponDependent;
    [ShowIf("isWeaponDependent")]
    public int weaponSlotId;
    
    public float abilityCooldown;
    public float abilityDuration;
    public float abilityDamage;
    public string abilityName;

    [Header("Visuals")]
    public Sprite abilityIcon;
    public bool _isAbilityActive = true;
    
    public virtual void Awake()
    {
        gameManager = GameManager.Instance;
    }

    public virtual void Start()
    {   
        _playerStats = GetComponent<PlayerStats>();
        _playerStats.abilitySlot = this;
        uiMenager = GameManager.Instance.UiMenager;
        Debug.Log("Ability assigned to player: " + abilityName);
        if(isPassive && _isAbilityActive)
        {
            // Aktywuj umiejętność pasywną od razu po starcie gry
        }
        weaponController = GameManager.Instance.WeaponParrent.GetComponent<WeaponSwap>();
    }
    public virtual void Update()
    {
        if(isWeaponDependent == true && weaponSlotId != weaponController.selectedWeapon) return;
        if(isWeaponDependent == true && weaponController.isWeaponSlotUnlocked == false) return;
        if (Input.GetButtonDown("Ability") && _isAbilityActive && !isPassive)
        {
            _isAbilityActive = false;
            ActivateAbility();
        }
    }

    /// <summary>
    /// Metoda aktywująca umiejętność. Powinna być nadpisana w klasach dziedziczących, aby dodać logikę specyficzną dla danej umiejętności.
    /// </summary>
    public abstract void ActivateAbility();

    /// <summary>
    /// Metoda kiedy umiejętność zostanie zmieniona, np. w przypadku zmiany umiejętności w ekwipunku.
    /// Można ją nadpisać w klasach dziedziczących, aby
    /// </summary>
    public virtual void OnAbilityChanged()
    {
        
    }

    /// <summary>
    /// Metoda resetująca umiejętność po upływie czasu cooldown. Można ją nadpisać w klasach dziedziczących, aby dodać dodatkowe efekty wizualne lub logikę.
    /// </summary>
    public virtual void ResetAbility()
    {
        _isAbilityActive = true;
    }
}

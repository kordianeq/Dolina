using UnityEngine;
[RequireComponent(typeof(PlayerStats))]
public abstract class Ability : MonoBehaviour
{
    public AnimationController _animationController;
    public PlayerStats _playerStats;
    [Header("Ability Settings")]
    public bool isPassive;
    public float abilityCooldown;
    public float abilityDuration;
    public float abilityDamage;
    public string abilityName;

    [Header("Visuals")]
    public Sprite abilityIcon;
    public bool _isAbilityActive = true;
    
    void Start()
    {   
        _playerStats = GetComponent<PlayerStats>();
        _playerStats.abilitySlot = this;
        Debug.Log("Ability assigned to player: " + abilityName);
    }
    void Update()
    {
        if (Input.GetButtonDown("Ability") && _isAbilityActive && !isPassive)
        {
            _isAbilityActive = false;
            ActivateAbility();
        }
    }
    public abstract void ActivateAbility();

    public void ResetAbility()
    {
        _isAbilityActive = true;
    }
}

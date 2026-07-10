using UnityEngine;

public class UndyingTotem : Ability
{
    [SerializeField] private GameObject ace;
   
  
    
    public override void ActivateAbility()
    {
        _playerStats.playerHp = _playerStats.maxPlayerHp * 0.5f; 
        ace.GetComponentInChildren<Animator>().Play("UndyingTotem");
        _isAbilityActive = false;
        Invoke(nameof(ResetAbility), abilityCooldown);
    }
}

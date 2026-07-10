using UnityEngine;

public class UndyingTotem : Ability
{
    [SerializeField] private GameObject ace;
   
  
    
    public override void ActivateAbility()
    {
        _playerStats.playerHp = _playerStats.maxPlayerHp;
        ace.GetComponentInChildren<Animator>().Play("UndyingTotem");
    }
}

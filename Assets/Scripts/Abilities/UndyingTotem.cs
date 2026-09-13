using UnityEngine;

public class UndyingTotem : Ability
{
    [SerializeField] private GameObject ace;

    public override void Start()
    {
        base.Start();
        if(_isAbilityActive)
        {
            uiMenager.UndyingTotemSlot.SetActive(true);
        }
        else
        {
            uiMenager.UndyingTotemSlot.SetActive(false);
        }
    }
    public override void ActivateAbility()
    {
        _playerStats.playerHp = _playerStats.maxPlayerHp * 0.5f; 

        // Uruchom animację umiejętności
        ace.GetComponentInChildren<Animator>().Play("UndyingTotem");

        _isAbilityActive = false;

        // Ukryj ikonę umiejętności w UI
        uiMenager.UndyingTotemSlot.SetActive(false);

        // Wywołaj ResetAbility po upływie czasu cooldown
        Invoke(nameof(ResetAbility), abilityCooldown);
    }

    // Resetuje umiejętność po upływie czasu cooldown + przywraca ikonę umiejętności w UI
    public override void ResetAbility()
    {
        base.ResetAbility();
        uiMenager.UndyingTotemSlot.SetActive(true);
    }
}

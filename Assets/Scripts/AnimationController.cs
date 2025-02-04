using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Animator animator;
    WeaponSwap weaponInfo;

    void Start()
    {
        weaponInfo = GameObject.Find("GunSlot").GetComponent<WeaponSwap>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        animator.SetInteger("WeaponIndex", weaponInfo.selectedWeapon);
        
        switch (weaponInfo.selectedWeapon)
        {
            case 0:

                return;
            default:
                break;
        }
    }
    public void Reload()
    {
        animator.SetTrigger("Reload");
    }
}
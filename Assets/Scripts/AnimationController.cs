using UnityEngine;

public class AnimationController : MonoBehaviour
{
   public Animator animator;
    //WeaponSwap weaponInfo;

    void Start()
    {
        //weaponInfo = GameObject.Find("GunSlot").GetComponent<WeaponSwap>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
       
    }
    public void Reload()
    {
        animator.SetTrigger("Reload");
    }
    public void Shot()
    {
        animator.SetTrigger("Shoot");
    }
   
}
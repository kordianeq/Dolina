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
    private void Awake()
    {
      ;
    }
    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("speed", GameManager.Instance.playerRef.rb.linearVelocity.magnitude);
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
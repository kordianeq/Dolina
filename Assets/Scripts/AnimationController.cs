using UnityEngine;

public class AnimationController : MonoBehaviour
{
   public Animator animator;
   PlayerMovement playerMovement;
    //WeaponSwap weaponInfo;

    void Start()
    {
        //weaponInfo = GameObject.Find("GunSlot").GetComponent<WeaponSwap>();
        animator = GetComponent<Animator>();
    }
    private void Awake()
    {
      playerMovement =  GameManager.Instance.playerRef;
    }
    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("speed",playerMovement.rb.linearVelocity.magnitude);
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
using UnityEngine;

public class NpcHurtBox : HurtBox, IDamagable
{
    [SerializeField] private PropPart bodyPart;
    [SerializeField] private float dmgMultiplier = 1;
    private NpcDmgManager dmgMannager;

    void Awake()
    {
        dmgMannager = ParentRecreceiver.GetComponent<NpcDmgManager>();
    }
    public void Damaged(float dmg)
    {
        Debug.Log("Bullet hit: " + gameObject.name);
        dmgMannager.Receive(bodyPart,dmg * dmgMultiplier,-1f,Vector3.zero);
    }

    public bool Damaged(float dmg, Vector3 dir, float force)
    {
        Debug.Log("Bullet hit with koncback: " + gameObject.name);   
        dmgMannager.Receive(bodyPart,dmg * dmgMultiplier,force,dir); 
        return true;
    }
}

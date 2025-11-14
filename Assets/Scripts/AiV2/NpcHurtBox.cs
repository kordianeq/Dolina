using UnityEngine;

public class NpcHurtBox : HurtBox, IDamagable
{
    
    public NpcDmgManager dmgMannager;
    void Awake()
    {
        dmgMannager = ParentRecreceiver.GetComponent<NpcDmgManager>();
    }
    public void Damaged(float dmg)
    {
        Debug.Log("Bullet hit: " + gameObject.name);
    }

    public bool Damaged(float dmg, Vector3 dir, float force)
    {
        Debug.Log("Bullet hit with koncback: " + gameObject.name);    
        return true;
    }
}

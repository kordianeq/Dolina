using Unity.VisualScripting;
using UnityEngine;

public class NpcDmgManager : MonoBehaviour, IKickeable, IReceiver, IiBoomeable
{
    [SerializeField] NpcCoreBase eCore;
    [SerializeField] RigMiscCore rigMiscCore;
    [SerializeField] BodyPartMannager bodyPartMannager;
    public DmgWobble dmgWooble;

    public float EnemyHp;
    public float overkillHp;
    public float dmgFromKick;
    public float DmgFromExpl;
    
    public bool isDead {get; private set;} = false;
    // Start is called before the first frame update
    void Start()
    {
        //rigMiscCore.GetProp(PropPart.Head);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*public void Damaged(float dmg)
    {
        dmgWooble.MakeWobble();
    }

    public bool Damaged(float dmg, Vector3 dir, float f)
    {
        dmgWooble.MakeWobble();
        return true;
    }*/

    public void KickHandle()
    {
        dmgWooble.MakeWobble();
    }

    public bool kickHandle(Vector3 from, float kickForce)
    {
        dmgWooble.MakeWobble();

        Debug.Log("Kickekekekek");
        eCore.HandleKick(from, kickForce);
        TakeHp(dmgFromKick, 0,from);

        return false;
    }

    public void MakeBoom(float hp)
    { 
        TakeHp(DmgFromExpl, 0,Vector3.zero);
    }

    // here mannager recieves dmg data from limbs
    public void Receive(PropPart bodyPart, float dmg, float force, Vector3 direction)
    {
        dmgWooble.MakeWobble();
        TakeHp(10,0,Vector3.zero);
       /* if (bodyPartMannager)
        {
            //try to find valid bodyPart to damage
            bool found = false;
            foreach (BodyPart bPart in bodyPartMannager.bodyParts)
            {
                if (bPart.partName == bodyPart)
                {
                    found = true;
                    TakeHp(bPart.CalculatePartDmg(dmg), force, direction);
                    if (bPart.partHp <= 0 && !bPart.broken && !bPart.unbreakeable)
                    {
                        Debug.Log("Try to Modify bodyPart Rendering");
                        bPart.broken = true;
                        PropControll selectedProp = rigMiscCore.GetProp(bodyPart);
                        if (selectedProp != null)
                        {
                            switch (selectedProp.part)
                            {
                                case PropPart.Head:
                                    selectedProp.SetProp(1);
                                    selectedProp.ForceParticle();
                                    rigMiscCore.animPart[1].ForceOne(-1);
                                    rigMiscCore.animPart[2].ForceOne(-1);
                                    break;
                                case PropPart.HandL:
                                    selectedProp.SetProp(1);
                                    break;
                                case PropPart.HandR:

                                    break;
                                case PropPart.Bottle:

                                    break;
                                case PropPart.Torso:
                                    selectedProp.SetProp(1);
                                    selectedProp.ForceParticle();
                                    rigMiscCore.animPart[1].ForceOne(-1);
                                    rigMiscCore.animPart[2].ForceOne(-1);
                                break;
                            }
                        }
                        else
                        {
                            Debug.Log("Part CANT BE BROKEN");
                        }

                    }




                    break;
                }

            }
            if (!found)
            {
                Debug.Log("No valid body part to damage!");
                TakeHp(dmg, force, direction);
            }
        }*/

    }



    public void TakeHp(float _hp, float _knoc, Vector3 dir)
    {
        EnemyHp -= _hp;
        EvaluateHp();
        if (_knoc != 0)
        {
            eCore.HandleKnockBack(dir,_knoc);
        }
    }

    //handlesDEATH
    public void EvaluateHp()
    {
        eCore.HandleHpLoss(isDead);
        if(EnemyHp<=0 && !isDead)
        {
            isDead = true;
            eCore.HandleDeath();
        }else if(isDead && EnemyHp<=-overkillHp)
        {
            eCore.HandleOverkill();
        }

        //resurected wtf
        if(EnemyHp > 0 && isDead)
        {
            eCore.HandleResurection();
        }

    }

    
}


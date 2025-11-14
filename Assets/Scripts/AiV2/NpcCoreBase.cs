using System;
using UnityEngine;

public class NpcCoreBase : MonoBehaviour
{
    // ---> update, not true---> ok, so this will be the most fucking important script for enemies, like all the global stats, hp etc. logic itself is containded inside "brains"
    // responisble for comunication between unity components and custom scripts.
    // icludes basic stats
    //will need to change it into abstract class tho... not now... im lazy

    [SerializeField] public NpcMovementBase move;
    
    [SerializeField] public Animator animator;
    
    [SerializeField] public NpcDmgManager dmgMannager;



    void FixedUpdate()
    {
        //npcMove.AddDirectionalForce(Vector3.forward,1f,ForceMode.Force);
    }

    public void HandleKick(Vector3 from, float kickForce)
    {
        move.AddDirectionalForce(Vector3.up,10f,ForceMode.Impulse);
    }

    public void EvaluateHp()
    {

    }

    public void HandleKnockBack(Vector3 dir,float _knoc)
    {
        
    }


    /*
        [SerializeField] public NpcMovementBrain moveBrain;
        [SerializeField] public NpcBehaviorBrain behaviorBrain;

        [SerializeField] public DmgMannager dmgMannager;
        [SerializeField] public Animator animator;

        [SerializeField] public Transform corectionb;
        [SerializeField] public Transform corectionT;
        public Transform ChunkSpw;
        public bool dead = false;

        [SerializeField] bool incapacitated = false;
        [SerializeField] bool stunned = false;
        [SerializeField] bool inAgro = false;
        [SerializeField] Vector3 moveVector;
        [SerializeField] GameObject visualPart;


        //timers
        [SerializeField] float defaultKnockBackForce;
        [SerializeField] float defaultKickBackForce;





        void Start()
        {

        }

        // Update is called once per frame you moron
        void Update()
        {
            if (!incapacitated)
            {
                if (moveRotatiomOverride)
                {
                    visualPart.transform.localRotation = desiredOVERRIDENBodyOrientation;
                }
                else
                {
                    visualPart.transform.localRotation = desiredBodyOrientation;
                }
                moveRotatiomOverride = false;
            }


            if (moveVector == Vector3.zero)
            {

                animator.SetFloat("WBlend", 0);
            }
            else
            {
                //Debug.Log("Skibidi");
                animator.SetFloat("WBlend", 1);
            }

            if (OVERKILL)
            {
                OverkILL();
            }
        }

        private void LateUpdate()
        {
            if (!dead)
            {
                corectionb.rotation = corectionT.rotation;
                visualPart.transform.localPosition = new Vector3(0, 0.1f, 0);
            }
            else
            {
                visualPart.transform.localPosition = corectionT.localPosition;
            }
        }
        /*
        public void TakeDmg(Vector3 dmgDir, float dmgPower, float dmg)
        {
            //Debug.Log("CALL AN AMBULANCE");
            hp -= dmg;
            EvaluateHp();
            //Vector3 dmgdir = transform.position-dmgDir;
            //dmgdir.y = 0;
            moveBrain.AddDirectionalForce(dmgDir.normalized, dmgPower, ForceMode.Impulse);
            //moveBrain.AddDirectionalTorque(dmgDir.normalized,dmgPower*0.1f,ForceMode.Impulse);
        }*/
    /*
        public void Damaged(float dmg)
        {
            hp -= dmg;
            EvaluateHp();
            //Vector3 dmgdir = transform.position-dmgDir;
            //dmgdir.y = 0;
            moveBrain.AddDirectionalForce(Vector3.zero, 0f, ForceMode.Impulse);
        }*/


    /*
    public void EvaluateHp()
    {
        if (dmgMannager.EnemyHp <= 0 && !dead)
        {
            if (dmgMannager.EnemyHp > dmgMannager.overkillHp)
            {
                moveBrain.ForceDeadState();
                behaviorBrain.ForceDeadState();
                dead = true;
            }
            else
            {
                OverkillTry();
            }

        }
        else if (dead && dmgMannager.EnemyHp < dmgMannager.overkillHp)
        {
            OverkillTry();
        }
        else if (dead && dmgMannager.EnemyHp > 0)
        {
            moveBrain.ForceResurectState();
            behaviorBrain.ForceResurectState();
            dead = false;
        }
    }

    public void OverkillTry()
    {
        OVERKILL = true;
        
    }
    void OverkILL()
    { 
        Instantiate(Corpse, ChunkSpw.position, transform.rotation);
        Destroy(gameObject);
    }

    public void CalculateDesiredRotation(Vector3 _vect, float _speed, bool _rotationOverride)
    {

        if (_rotationOverride)
        {
            //Debug.Log("overriddee");
            moveRotatiomOverride = true;
            desiredOVERRIDENBodyOrientation = Quaternion.Lerp(visualPart.transform.rotation, Quaternion.Euler(0, (float)Math.Atan2(_vect.x, _vect.z) * Mathf.Rad2Deg, 0), _speed * Time.deltaTime);
        }
        else
        {
            desiredBodyOrientation = Quaternion.Lerp(visualPart.transform.rotation, Quaternion.Euler(0, (float)Math.Atan2(_vect.x, _vect.z) * Mathf.Rad2Deg, 0), _speed * Time.deltaTime);
        }
    }


    public void SetMoveVector(Vector3 moveInput)
    {
        moveVector = new Vector3(moveInput.x, 0, moveInput.z).normalized;
    }
    public Vector3 GetMoveVector()
    {
        return moveVector.normalized;
    }





    //                                  ------------GET SET--------------- IDK how to program
    public bool CheckDead()
    {
        return dead;
    }

    public bool CheckIncapacitated()
    {
        return incapacitated;
    }

    public bool CheckStunned()
    {
        return stunned;
    }

    public float CheckIncapacitionTime()
    {
        return incapacitionTime;
    }

    public float CheckStunnTime()
    {
        return stunnTime;
    }

    public void SetDead(bool _set)
    {
        dead = _set;
    }

    public void SetIncapacitated(bool _set)
    {
        incapacitated = _set;
    }

    public void SetStunned(bool _set)
    {
        stunned = _set;
    }

    public void SetMoveOverrideRotation(bool _set)
    {
        moveRotatiomOverride = _set;
    }

    public void HandleKick(Vector3 from, float _f)
    {
        Debug.Log("Kicken");
        Vector3 flattened = Vector3.ProjectOnPlane(transform.position - from, Vector3.up);
        moveBrain.AddDirectionalForce(flattened,defaultKickBackForce , ForceMode.Impulse);
        moveBrain.AddDirectionalForce(Vector3.up,defaultKickBackForce , ForceMode.Impulse);
        //moveBrain.AddDirectionalTorque(from.normalized,_f,ForceMode.Impulse);
    }
    
    public void HandleKnockBack(Vector3 from,float _f)
    { 

    }
*/
}

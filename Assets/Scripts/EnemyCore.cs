using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyCore : MonoBehaviour, Iidmgeable, IDamagable
{
    // ok, so this will be the most fucking important script for enemy, like all the global stats, hp etc... logic itself is containded inside "brains"
    //will need to change it into abstract class tho... not now
    [SerializeField]public NpcMovementBrain moveBrain;
    [SerializeField]public NpcBehaviorBrain behaviorBrain;
    [SerializeField]public Animator animator;

    [SerializeField] public Transform corectionb;
    [SerializeField] public Transform corectionT;
    public Transform ChunkSpw;
    public float hp, overkillHp;
    public bool dead = false;

    [SerializeField] bool incapacitated = false;
    [SerializeField] bool stunned = false;
    [SerializeField] bool inAgro = false;
    [SerializeField] Vector3 moveVector;
    [SerializeField] GameObject visualPart;

    //timers
    [SerializeField] float incapacitionTime;
    [SerializeField] float stunnTime;

    [SerializeField] bool moveRotatiomOverride;

    [SerializeField] Quaternion desiredBodyOrientation;
    [SerializeField] Quaternion desiredOVERRIDENBodyOrientation;
    public GameObject Corpse;
    void Start()
    {

    }

    // Update is called once per frame you moron
    void Update()
    {
        if(!incapacitated)
        {
            if(moveRotatiomOverride)
            {
                visualPart.transform.localRotation = desiredOVERRIDENBodyOrientation;
            }else
            {
                visualPart.transform.localRotation = desiredBodyOrientation;
            }
            moveRotatiomOverride = false;
        }
        


        

    }

    private void LateUpdate() {
        if(!dead)
        {corectionb.rotation = corectionT.rotation;
        visualPart.transform.localPosition = new Vector3(0,0.1f,0);
        }
        else
        {
            visualPart.transform.localPosition = corectionT.localPosition;
        }
    }
    public void TakeDmg(Vector3 dmgDir, float dmgPower, float dmg)
    {
        //Debug.Log("CALL AN AMBULANCE");
        hp -= dmg;
        EvaluateHp();
        //Vector3 dmgdir = transform.position-dmgDir;
        //dmgdir.y = 0;
        moveBrain.AddDirectionalForce(dmgDir.normalized, dmgPower, ForceMode.Impulse);
        //moveBrain.AddDirectionalTorque(dmgDir.normalized,dmgPower*0.1f,ForceMode.Impulse);
    }

    public void Damaged(float dmg)
    {
        hp -= dmg;
        EvaluateHp();
        //Vector3 dmgdir = transform.position-dmgDir;
        //dmgdir.y = 0;
        moveBrain.AddDirectionalForce(Vector3.zero, 0f, ForceMode.Impulse);
    }

    public void EvaluateHp()
    {
        if (hp <= 0 && !dead)
        {
            if (hp > overkillHp)
            {
                moveBrain.ForceDeadState();
                behaviorBrain.ForceDeadState();
                dead = true;
            }
            else
            {
                Overkill();
            }

        }
        else if (dead && hp < overkillHp)
        {
            Overkill();
        }
        else if (dead && hp > 0)
        {
            moveBrain.ForceResurectState();
            behaviorBrain.ForceResurectState();
            dead = false;
        }
    }

    public void Overkill()
    {
        Instantiate(Corpse, ChunkSpw.position, transform.rotation);
        Destroy(gameObject);
    }


    public void CalculateDesiredRotation(Vector3 _vect, float _speed,bool _rotationOverride)
    {
        
        if(_rotationOverride) 
        {
            //Debug.Log("overriddee");
            moveRotatiomOverride = true;
            desiredOVERRIDENBodyOrientation = Quaternion.Lerp(visualPart.transform.rotation, Quaternion.Euler(0, (float)Math.Atan2(_vect.x, _vect.z) * Mathf.Rad2Deg, 0), _speed * Time.deltaTime);
        }else
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

}
//[System.Serializable]
//public struct EnemiesSaveData
//{
//    public EnemySaveData[] enemies;
//    public int enemyCount;
//}
//public struct EnemySaveData
//{
//    public float Hp;
//    public bool Dead;
//    public Transform enemyPosition;
//}


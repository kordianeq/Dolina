using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBurstShot : NpcBehaviorStateOvveride
{
    [Header("weypointDetection Range")]

    bool oneT;
    [SerializeField] private float hitready;
    [SerializeField] private float hitTime;
    [SerializeField] ParticleSystem HitEffect;
    [SerializeField] string ShootAnimName;
    [SerializeField] private float rotationSpeed;
    [SerializeField] GameObject hurtBox;

    [Header("same level states")]
    public State AfterHit;


    [Header("childState")]
    public State chill;

    //[Header("do when passive")]   
    //public State patrolType;
    private void Start()
    {

    }
    private void Awake()
    {
        hurtBox.SetActive(false);
    }
    public override void Enter()
    {
        oneT = true;
        ForceStateAnim();
        //hurtBox.SetActive(true);
        //if (patrolType != null) { SetChild(patrolType); }
        Debug.Log("Shootin");
        //if(chill!=null){SetChild(chill);}

    }
    public override void Do()
    {
        if (time >= hitready)
        {
            if (oneT)
            {
                //ForceStateAnim(ShootAnimName);
                HitEffect.Play();
                oneT = false;
                hurtBox.SetActive(true);
            }
            if (time >= hitTime)
                {
                    isComplete = true;
                    //Debug.Log("aaaaaa");
                    Change(AfterHit);
                    //Debug.Log("waitStop");
                }
        }


        //Debug.Log("i am moving yo");
        //Change(DoAfter);            
        //brain.moveBrain.rotationOverrid = true;
        brain.mainCore.CalculateDesiredRotation((brain.target.position - core.transform.position).normalized, rotationSpeed, true);
        //brain.moveBrain.SetDesiredMovementRotation((moveTarget.position-core.transform.position).normalized, rotationSpeed);
    }
    public override void FixedDo()
    {
        brain.mainCore.SetMoveVector(new Vector3(0, 0, 0));
        // Debug.Log("inrange");
        //Change(AfterHit);


    }
    public override void Exit()
    {
        oneT = true;
        hurtBox.SetActive(false);
        //brain.moveBrain.rotationOverrid = false;
    }
}

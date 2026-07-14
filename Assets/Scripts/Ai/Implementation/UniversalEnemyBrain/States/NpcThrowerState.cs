using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcThrowerState : NpcBehaviorStateOvveride
{
    [Header("Move stats, make plyer go brrrr")]
    [SerializeField] private List<GameObject> bulletType;
    public Vector3 offset;
  
    [SerializeField] private Transform target;

    [SerializeField] float waitTime;
    [SerializeField] float throwForce;
    [SerializeField] float throwForceVertcal;
    [SerializeField] Transform throwPoint;
    [SerializeField] float rotationSpeed;

    public State DoAfter;
    private void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    public override void Enter()
    {
        ForceStateAnim();
        //brain.mainCore.animator.SetTrigger("Draw");
    }
    public override void Do()
    {

        brain.mainCore.CalculateDesiredRotation((brain.target.position - core.transform.position).normalized, rotationSpeed, true);
    }
    public override void FixedDo()
    {
        
        if (time >= waitTime)
        {
            int rando = Random.Range(0, bulletType.Count);
            GameObject Throw = Instantiate(bulletType[rando], throwPoint.position, throwPoint.rotation);
            Rigidbody rb = Throw.GetComponent<Rigidbody>();
            rb.AddForce(throwPoint.forward * throwForce,ForceMode.Impulse);
            rb.AddForce(Vector3.up * throwForceVertcal,ForceMode.Impulse);
            isComplete = true;

            Change(DoAfter);
            //Debug.Log("waitStop");
        }

    
         brain.mainCore.SetMoveVector(new Vector3(0,0,0));
        
    }
    public override void Exit()
    {

    }
}

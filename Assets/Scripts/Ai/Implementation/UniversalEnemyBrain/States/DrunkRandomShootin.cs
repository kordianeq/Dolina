using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DrunkRandomShootin : NpcBehaviorStateOvveride
{
    [Header("Move stats, make plyer go brrrr")]
    [SerializeField] private GameObject bulletType;
    public Vector3 offset;
  
    [SerializeField] private Transform target;

    [SerializeField] float waitTime;
    [SerializeField] float throwForce;
    [SerializeField] float throwForceVertcal;
    [SerializeField] Transform throwPoint;
    [SerializeField] float rotationSpeed;

    [SerializeField] int MagazineSize;
    
    [SerializeField] float firereate;
    float fireTimer;

    int bulletC;

    [SerializeField] float INaccuracyMultiplier;

    public State DoAfter;

    
    public string ReloadAnim;

 
    

    private void Awake()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    public override void Enter()
    {
        fireTimer = firereate;
        bulletC = MagazineSize;
        
        ForceStateAnim();
        //brain.mainCore.animator.SetTrigger("Draw");
    }
    public override void Do()
    {

        brain.mainCore.CalculateDesiredRotation((brain.target.position - core.transform.position).normalized, rotationSpeed, true);
    }
    public override void FixedDo()
    {
        if (bulletC > 0)
        {
            if (fireTimer > 0)
            {
                fireTimer -= Time.deltaTime;

            }
            else
            {
                fireTimer = firereate;
                bulletC--;

                //shoot
                //random innacuracy
                float xRotation = Random.Range(-INaccuracyMultiplier, INaccuracyMultiplier);
                float yRotation = Random.Range(-INaccuracyMultiplier, INaccuracyMultiplier);
                Vector3 a = transform.position + offset;
                Quaternion rototo = Quaternion.LookRotation(a - target.position);
                rototo *= Quaternion.Euler(Vector3.up * yRotation);
                rototo *= Quaternion.Euler(Vector3.right * xRotation);

                Instantiate(bulletType, throwPoint.position, rototo);

                
                
            }
        }
        else
        {
            //reload
            isComplete = true;
            bulletC = MagazineSize;
            Change(DoAfter);         
        }
        
        /*if (time >= waitTime)
        {
            int rando = Random.Range(0, bulletType.Count);
            GameObject Shoot = Instantiate(bulletType[rando], throwPoint.position, throwPoint.rotation);

            isComplete = true;

            Change(DoAfter);
            //Debug.Log("waitStop");
        }*/

    
         brain.mainCore.SetMoveVector(new Vector3(0,0,0));
        
    }
    public override void Exit()
    {

    }
}

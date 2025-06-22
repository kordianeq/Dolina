using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMovementBrain : NpcBrainMaster
{
    [Header("set up stuff")]
    public State startState;
    public State deathState;
    [SerializeField] Rigidbody rb;
    public RaycastHit hit;

    

    [Header("Ray cast set up stuff")]
    [SerializeField] public float groundRayLenght;
    [SerializeField] private float groundRayHeight;
    [SerializeField] private float groundRayRadius;
    [SerializeField] private LayerMask groundLayer;

    //   [Header("Movement stats (the important part)")]


    [Header("Movement Data")]
    [SerializeField] public float velocityMultplier;
    [SerializeField] public float dragMultiplier;
    [SerializeField] public bool groundCheck { get; private set; }
    //[SerializeField] public bool rotationOverrid;
    [SerializeField] public Quaternion desiredBodyOrientation;
    [SerializeField] GameObject visualPart;
    //[SerializeField] Vector3 visualPartDefaultOffset;

    // Start is called before the first frame update
    void Awake()
    {
        //visualPartDefaultOffset = visualPart.transform.localPosition;
        SetupInstances();
        sMachine.Set(startState, sMachine);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.DoBranch();
    }

    private void FixedUpdate()
    {
        groundCheck = Grounded();
        currentState.FixedDoBranch();
    }


    // General stuff, mostly used by other scripts i guess

    public bool Grounded()
    {
        return Physics.SphereCast(transform.position + new Vector3(0, groundRayHeight + groundRayRadius, 0), groundRayRadius, Vector3.down, out hit, groundRayLenght, groundLayer);

    }

    public void Bounce(float _hoverHeight, float _spring, float _damp)
    {

        Vector3 vel = rb.velocity;
        //Vector3 dir = transform.TransformDirection(Vector3.down);
        Vector3 dir = Vector3.down;
        float dirVel = Vector3.Dot(dir, vel);
        float x = hit.distance - _hoverHeight;
        float springForce = (x * _spring) - (dirVel * _damp);

        rb.AddForce(dir * springForce);
    }

    public void FallAccelerate(float _maxFallSpeed, float _gravityMult)
    {
        if (rb.velocity.y > -_maxFallSpeed)
        {
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime * _gravityMult;
        }
    }


    

    public void MoveCharacter(float _speed, float _drag)
    {
        Vector3 velo = rb.velocity;
        velo = new Vector3(velo.x, 0f, velo.z);

        //APPLY FORCES
        rb.AddForce(((mainCore.GetMoveVector() * _speed * velocityMultplier) - velo) * _drag * dragMultiplier / Time.fixedDeltaTime);
    }

    public void DeAccelerate(float decelerationRate)
    {
        Vector3 velocity = rb.velocity;
        velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
        //to avoid unnecessary calculations i guess
        if (velocity.magnitude > 0.01f)
        {
           
            Vector3 decelerationForce = -velocity.normalized * decelerationRate;

            rb.AddForce(decelerationForce, ForceMode.Acceleration);
        }
    }

    public void SetGravity(bool g)
    {
        rb.useGravity = g;
    }

    public void AddDirectionalForce(Vector3 _direction, float _force,ForceMode forceMode)
    {
        rb.AddForce(_direction * _force,forceMode);
    }

    public void AddDirectionalTorque(Vector3 _direction, float _force,ForceMode forceMode)
    {
        rb.AddTorque(_direction * _force,forceMode);
    }

    public void ClearVerticalVelo()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }

    
    

    // move stats
    public void MoveStatsOverride(float _speed, float _drag)
    {
        velocityMultplier = _speed;
        dragMultiplier = _drag;
    }

    public void MoveStatsOverride(float _speed)
    {
        velocityMultplier = _speed;
        
    }

    public void MoveStatsClear()
    {
        velocityMultplier = 1;
        dragMultiplier = 1;
    }


    //cursed workarounds
    public void ForceMasterState(State forceSt)
    {
        sMachine.Set(forceSt, sMachine);
    }
 

    public void RbRotationConstraints(bool lockmode)
    {
        if(lockmode)
        {
            rb.gameObject.transform.rotation = new Quaternion(0, 0, 0,0);
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }else
        {
            
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    public Transform GetMainTransform()
    {
        return rb.transform;
    }

    // ONLY FOR DEBUG, DOES NOTHING IN GAME
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + new Vector3(0, groundRayHeight, 0), Vector2.down * groundRayLenght, Color.red);
        if (Physics.SphereCast(transform.position + new Vector3(0, groundRayHeight + groundRayRadius, 0), groundRayRadius, Vector3.down, out hit, groundRayLenght, groundLayer))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hit.point, groundRayRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, groundRayHeight - groundRayLenght, 0), groundRayRadius);

        }
    }



    // special force states
    public override void ForceDeadState()
    {
        Debug.Log("i am dead now");
        sMachine.Set(deathState, sMachine,true);
    }

    public override void ForceResurectState()
    {
        Debug.Log("wtf, i resurected");
        sMachine.Set(startState, sMachine,true);
    }


}


/*  Trash bin


public void RotateTowardsVector(Vector3 _vect, float _speed)
    {
        //Debug.Log(Math.Atan2(_vect.x,_vect.z) *Mathf.Rad2Deg);

        visualPart.transform.rotation = Quaternion.Lerp(visualPart.transform.rotation, Quaternion.Euler(0, (float)Math.Atan2(_vect.x, _vect.z) * Mathf.Rad2Deg, 0), _speed * Time.deltaTime);

        Debug.Log("rotating at");
             Vector3 targetLook = _vect - transform.position;
             targetLook = new Vector3(targetLook.x, 0, targetLook.z);
             Vector3 lookDir = Vector3.RotateTowards(visualPart.transform.forward, targetLook, _speed * Time.deltaTime, 0.0f);
             //lookDir = new Vector3(lookDir.x,0,lookDir.y);
             visualPart.transform.rotation = Quaternion.LookRotation(lookDir);
    }


*/
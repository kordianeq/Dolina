using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class MovementBrain : MachineCore
{
    [Header("set up stuff")]
    public State startState;
    [SerializeField] Rigidbody rb;
    public RaycastHit hit ;

    [Header("Ray cast set up stuff")]
    [SerializeField] public float groundRayLenght ;
    [SerializeField] private float groundRayHeight;
    [SerializeField] private float groundRayRadius;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement stats (the important part)")]


    [SerializeField] private float minJumpTime;
    [SerializeField] float jumpTimer;

    [Header("Movement Data")]
    [SerializeField] public Vector3 moveVector;
    [SerializeField] public bool groundCheck{get; private set;}
    [SerializeField] public bool wantJump;
    [SerializeField] GameObject visualPart;
    [SerializeField] Vector3 visualPartDefault;

    // Start is called before the first frame update
    void Awake()
    {
        visualPartDefault = visualPart.transform.localPosition;
        SetupInstances();
        sMachine.Set(startState,sMachine);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.DoBranch();
    }

    private void FixedUpdate() 
    {
        groundCheck=Grounded();

        currentState.FixedDoBranch();
    }

    public bool Grounded()
    {
        return Physics.SphereCast(transform.position + new Vector3(0, groundRayHeight + groundRayRadius, 0), groundRayRadius, Vector3.down, out hit, groundRayLenght, groundLayer);
        
    }

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

    public void Bounce(float _hoverHeight,float _spring,float _damp)
    {

        Vector3 vel = rb.velocity;
        //Vector3 dir = transform.TransformDirection(Vector3.down);
        Vector3 dir = Vector3.down;
        float dirVel = Vector3.Dot(dir, vel);
        float x = hit.distance - _hoverHeight;
        float springForce = (x * _spring) - (dirVel * _damp);

        rb.AddForce(dir * springForce);
    }

    public void FallAccelerate(float _maxFallSpeed,float _gravityMult)
    {
        if(rb.velocity.y >-_maxFallSpeed)
        {
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime * _gravityMult;
        }
    }

    public void ClearMove()
    {
        moveVector = Vector3.zero;
    }

    public void SetMoveVector(Vector3 moveInput)
    {
        moveVector = new Vector3(moveInput.x,0,moveInput.z).normalized; 
    }

    public void MoveCharacter(float _speed,float _drag)
    {
        Vector3 velo = rb.velocity;
        velo = new Vector3(velo.x, 0f, velo.z);

        //APPLY FORCES
        rb.AddForce(((moveVector * _speed) - velo) * _drag / Time.fixedDeltaTime);
    }

    public void SetGravity(bool g)
    {
        rb.useGravity=g;
    }

    public void AddDirectionalForce(Vector3 v,float f)
    {
        rb.AddForce(v*f);
    }

    public void AddDirectionalImpulseForce(Vector3 v,float f)
    {
        rb.AddForce(v*f,ForceMode.Impulse);
    }

    public void ClearVerticalVelo()
    {
        rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
    }

    //cursed workaround
    public void ForceMasterState(State forceSt)
    {
        sMachine.Set(forceSt,sMachine);                
    }

    public void SetHeight(Vector3 _offset)
    {
        visualPart.transform.localPosition = visualPartDefault + _offset;
    }
    
}

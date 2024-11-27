using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcMovementBrain : MachineCore
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

 //   [Header("Movement stats (the important part)")]
 

    [Header("Movement Data")]
    [SerializeField] public Vector3 moveVector;
    [SerializeField] public float velocityMultplier;
    [SerializeField] public float dragMultiplier;
    [SerializeField] public bool groundCheck{get; private set;}
    [SerializeField] public bool wantJump;
    [SerializeField] public bool rotationOverrid;
    [SerializeField] GameObject visualPart;
    [SerializeField] Vector3 visualPartDefaultOffset;

    // Start is called before the first frame update
    void Awake()
    {
        visualPartDefaultOffset = visualPart.transform.localPosition;
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
        visualPart.transform.localPosition = visualPartDefaultOffset + _offset;
    }

    public void RotateTowardsVector(Vector3 _vect, float _speed)
    {
        //Debug.Log(Math.Atan2(_vect.x,_vect.z) *Mathf.Rad2Deg);

        visualPart.transform.rotation = Quaternion.Lerp(visualPart.transform.rotation, Quaternion.Euler(0, (float)Math.Atan2(_vect.x,_vect.z) *Mathf.Rad2Deg, 0), _speed * Time.deltaTime);

       /* Debug.Log("rotating at");
            Vector3 targetLook = _vect - transform.position;
            targetLook = new Vector3(targetLook.x, 0, targetLook.z);
            Vector3 lookDir = Vector3.RotateTowards(visualPart.transform.forward, targetLook, _speed * Time.deltaTime, 0.0f);
            //lookDir = new Vector3(lookDir.x,0,lookDir.y);
            visualPart.transform.rotation = Quaternion.LookRotation(lookDir);*/
    }

    public void OverrideMoveStats(float _speed,float _drag)
    {
        velocityMultplier = _speed;
        dragMultiplier = _drag;
    }
    public void ClearMoveOverride()
    {
        velocityMultplier = 1;
        dragMultiplier = 1;
    }

}

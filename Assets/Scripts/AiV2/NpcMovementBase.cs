using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor.Callbacks;
using UnityEngine;


public class NpcMovementBase : MonoBehaviour
{
    [Header("set up stuff")]
    [SerializeField] Rigidbody rb;
    public RaycastHit hit;

    public NpcRotationModes npcLookMode {get;private set; }

    [Header("Ray cast set up stuff")]
    [SerializeField] public float groundRayLenght;
    [SerializeField] private float groundRayHeight;
    [SerializeField] private float groundRayRadius;
    [SerializeField] private LayerMask groundLayer;


    //   [Header("Movement stats (the important part)")]
    [Header("Movement Data")]
    [SerializeField] public bool groundCheck { get; private set; }
    //[SerializeField] public bool rotationOverrid;
    [SerializeField] public Quaternion desiredBodyOrientation;
    [SerializeField] GameObject visualPart;
    //[SerializeField] Vector3 visualPartDefaultOffset;

    [SerializeField] float defaultHoverHeight,defaultSpring,defaultDamping,maxFallSpeed;

    [SerializeField] float baseSpeed,baseDrag,deAcceleration,baseRotationSpeed;
    
    float stunnSpeedValue = 1f;
    float stunnDragValue = 1f;
    float stunnDeAccelerationValue = 1f;
    
    

    float finalSpeed;
    [SerializeField] Vector3 moveVector = new(0,0,0);


    [Header("Movement Data")]
    [SerializeField] Material[] bullshitDebug;
    [SerializeField] GameObject debDetector;
    public Vector3 rbDirection;
    [SerializeField] Vector3 lastSavedRbPosition;

    bool inStunn = false;
    float stuntimer = 0;

    // Start is called before the first frame update
    void Awake()
    {
       npcLookMode = NpcRotationModes.LookAtVelocityReversed;
       rbDirection = visualPart.transform.forward;
       lastSavedRbPosition = visualPart.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        switch(npcLookMode)
        {
            case NpcRotationModes.disabled:

            break;
            case NpcRotationModes.LookAtMoveDirection:
                CalculateDesiredRotation(moveVector,baseRotationSpeed);
                visualPart.transform.localRotation = desiredBodyOrientation;
            break;
            case NpcRotationModes.LookAtTarget:
                visualPart.transform.localRotation = desiredBodyOrientation;
            break;
            case NpcRotationModes.LookAtVelocity:
                
                if(rb.linearVelocity.magnitude > 0.1f)
                {
                    //lastNonZeroRotation = Vector3.Normalize(rb.linearVelocity);  
                    lastSavedRbPosition = rbDirection;                
                }
                CalculateDesiredRotation(lastSavedRbPosition,baseRotationSpeed);
                visualPart.transform.localRotation = desiredBodyOrientation;
            break;
            case NpcRotationModes.LookAtVelocityReversed:
                
                Vector3 rbdir = rb.linearVelocity;
                rbdir = Vector3.ProjectOnPlane(rbdir,Vector3.up);
                if(rbdir.magnitude > 0.1f)
                {
                    lastSavedRbPosition= Quaternion.AngleAxis(180, Vector3.up) *rbDirection;
                    //lastNonZeroRotation = rb.linearVelocity
                    //lastNonZeroRotation = Vector3.Normalize(Quaternion.AngleAxis(180, Vector3.up) * rb.linearVelocity);                  
                }
                CalculateDesiredRotation(lastSavedRbPosition,baseRotationSpeed);
                visualPart.transform.localRotation = desiredBodyOrientation;
            break;
        }
    }

    IEnumerator SoftStunn(float _stunnTime)
    {  
        inStunn = true;
        stuntimer = _stunnTime;
        StunnValues(0f,0.2f,0.75f);
        while(stuntimer > 0)
        {
            stuntimer -= Time.deltaTime;
            
            yield return null;
        }
        ResetStunnValues();
        inStunn = false;
        yield return null;
    }

    public void EnterSoftStunn(float stunTime)
    {
        if(!inStunn)
        {
            StartCoroutine(SoftStunn(stunTime));
        }else
        {
            stuntimer+= stunTime;
        }

    }

    void ResetStunnValues()
    {
        stunnSpeedValue = 1;
        stunnDragValue = 1;
        stunnDeAccelerationValue = 1;
    }
    void StunnValues(float _speed,float _drag, float _deAcceleration)
    {
        stunnSpeedValue = _speed;
        stunnDragValue = _drag;
        stunnDeAccelerationValue = _deAcceleration;
    }

    private void FixedUpdate()
    {
        groundCheck = Grounded();
        
        //Hover Above Ground
        if(groundCheck)
        {
            SetGravity(false);
            Bounce(defaultHoverHeight,defaultSpring,defaultDamping);
        }
        else
        {
            SetGravity(true);
            FallAccelerate(maxFallSpeed,2f);

        }

        if(moveVector.magnitude <= 0.05f || inStunn)
        {
            debDetector.GetComponent<Renderer>().material = bullshitDebug[0];
            
 
            DeAccelerate(deAcceleration);
            

        }
        else
        {
            ApplyMoveVector();
            debDetector.GetComponent<Renderer>().material = bullshitDebug[1];
        }

        rbDirection = rb.linearVelocity.normalized;
        rbDirection = new Vector3(rbDirection.x,0,rbDirection.z);
    }



   public void CalculateDesiredRotation(Vector3 _vect, float _speed)
    {
        //_vect = Vector3.ProjectOnPlane(_vect,Vector3.up);
        if(_vect != Vector3.zero)
        {
            //_vect = _vect.normalized;
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


    // General stuff, mostly used by other scripts i guess

    public bool Grounded()
    {
        return Physics.SphereCast(transform.position + new Vector3(0, groundRayHeight + groundRayRadius, 0), groundRayRadius, Vector3.down, out hit, groundRayLenght, groundLayer);
    }

    public void Bounce(float _hoverHeight, float _spring, float _damp)
    {
        Vector3 vel = rb.linearVelocity;
        //Vector3 dir = transform.TransformDirection(Vector3.down);
        Vector3 dir = Vector3.down;
        float dirVel = Vector3.Dot(dir, vel);
        float x = hit.distance - _hoverHeight;
        float springForce = (x * _spring) - (dirVel * _damp);

        rb.AddForce(dir * springForce);
    }

    public void FallAccelerate(float _maxFallSpeed, float _gravityMult)
    {
        if (rb.linearVelocity.y > -_maxFallSpeed)
        {
            rb.linearVelocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime * _gravityMult;
        }
    }

    public void MoveCharacter(float _speedMultiplier, float _dragMultiplier)
    {
        Vector3 velo = rb.linearVelocity;
        velo = new Vector3(velo.x, 0f, velo.z);

        //APPLY FORCES
        rb.AddForce(_dragMultiplier * baseDrag * ((_speedMultiplier * baseSpeed * GetMoveVector() ) - velo) / Time.fixedDeltaTime);       
    }

    public void ApplyMoveVector()
    {
        Vector3 velo = rb.linearVelocity;
        velo = new Vector3(velo.x, 0f, velo.z);

        //APPLY FORCES
        rb.AddForce( baseDrag * stunnDragValue * ((baseSpeed * stunnSpeedValue * GetMoveVector() ) - velo) / Time.fixedDeltaTime);
    }

    public void DeAccelerate(float decelerationRate)
    {
        Vector3 velocity = new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z);
        
        //to avoid unnecessary calculations i guess
        if (velocity.magnitude > 0.01f)
        {
           
            Vector3 decelerationForce = decelerationRate * stunnDeAccelerationValue * -velocity.normalized;

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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    // move stats
    /*
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
    }*/


    //cursed workarounds

    public void ResetMoveVector()
    {
        moveVector = Vector3.zero;
    }

    public void SetLookState(NpcRotationModes _lookMode)
    {
        npcLookMode = _lookMode;
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


}

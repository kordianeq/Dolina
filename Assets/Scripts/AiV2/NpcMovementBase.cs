using UnityEngine;

public class NpcMovementBase : MonoBehaviour
{
    [Header("set up stuff")]
    [SerializeField] Rigidbody rb;
    public RaycastHit hit;

    

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

    [SerializeField] float baseSpeed,baseDrag,deAcceleration;
    [SerializeField] Vector3 moveVector = new(0,0,0);


    [Header("Movement Data")]
    [SerializeField] Material[] bullshitDebug;
    [SerializeField] GameObject debDetector;
    // Start is called before the first frame update
    void Awake()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    private void FixedUpdate()
    {
        groundCheck = Grounded();
        //Bounce(1f,);

        //Hover Above Ground
        if(groundCheck)
        {
            SetGravity(false);
            Bounce(defaultHoverHeight,defaultSpring,defaultDamping);
        }
        else
        {
            SetGravity(true);
        }


        if(moveVector.magnitude <= 0)
        {
            debDetector.GetComponent<Renderer>().material = bullshitDebug[0];
            DeAccelerate(deAcceleration);
        }else
        {
            ApplyMoveVector();
            debDetector.GetComponent<Renderer>().material = bullshitDebug[1];
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
        rb.AddForce( baseDrag * ((baseSpeed * GetMoveVector() ) - velo) / Time.fixedDeltaTime);
    }

    public void DeAccelerate(float decelerationRate)
    {
        Vector3 velocity = rb.linearVelocity;
        velocity = new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z);
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

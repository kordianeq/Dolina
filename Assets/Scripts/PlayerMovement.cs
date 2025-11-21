using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]


    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown, kickCooldown;
    public float airMultiplier;

    public float defaultSpeed;
    float newLocalSpeed;
    float moveSpeed;
    bool inAir;

    bool readyToJump, readyToKick;
    bool speedOverride;
    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    public Collider playerCollider;

    float horizontalInput;
    float verticalInput;

    public bool movementLocked;
    Vector3 moveDirection;

    public Rigidbody rb;

    bool lastGrounded;

    public bool mounted;

    private PlayerStats myStats;
    [SerializeField] private CameraControll playerCamera;
    private void Awake()
    {
        myStats = GetComponent<PlayerStats>();

        if (GameManager.Instance != null)
        {
            Debug.Log("Movement Awake");
            GameManager.Instance.RegisterPlayer(this, myStats, playerCamera);
        }
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;

        readyToKick = true;
        readyToJump = true;
        speedOverride = false;
    }

    void SwitchMountState()
    {
        if (mounted)
        {
            if (playerCollider.enabled) playerCollider.enabled = false;
            if (rb) rb.isKinematic = true;

        }
        else
        {
            if (!playerCollider.enabled) playerCollider.enabled = true;
            if (rb) rb.isKinematic = false;
        }
    }
    bool pervMountedState = false;
    private void Update()
    {
        if (mounted != pervMountedState) SwitchMountState();
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        if (speedOverride)
        {
            moveSpeed = newLocalSpeed;
        }
        else
        {
            moveSpeed = defaultSpeed;
        }

        MyInput();
        SpeedControl();

        if (!movementLocked)
        {
            //Jump Input
            if (Input.GetButtonDown("Jump") && readyToJump && grounded)
            {

                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }

            //Kicking Input
            if (Input.GetButtonDown("Kick") && readyToKick)
            {
                Kick();

                readyToKick = false;

                Invoke(nameof(ResetKick), kickCooldown);
            }
        }

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (grounded != lastGrounded && grounded == true)
        {
            //Debug.Log("Landed");
            GetComponentInChildren<Audio_Footsteps>().PlayLanding();
        }
        lastGrounded = grounded;
        pervMountedState = mounted;
    }

    private void FixedUpdate()
    {
        CalculateGravity();
        if (!movementLocked)
        {
            MovePlayer();
        }

    }

    void CalculateGravity()
    {
        if (!grounded)
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.down * Time.deltaTime * 2f);
                //rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y*1.25f*Time.deltaTime, rb.linearVelocity.z);
            }
        }
    }
    public void Launch(Vector3 launchVelocity, float multiplier)
    {
        rb.AddForce(launchVelocity * multiplier, ForceMode.Impulse);
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump

    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);


    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    public void SpeedReduction(float newSpeed)
    {
        speedOverride = true;
        newLocalSpeed = newSpeed;
    }

    void Kick()
    {
        Debug.Log("Kick");
        //Kiciking logic
    }
    void ResetKick()
    {
        readyToKick = true;
    }
}
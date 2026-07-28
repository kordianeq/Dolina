using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SourceMovement : MonoBehaviour
{
    [Header("General Settings")]
    public float mass = 1.0f;           // Masa gracza (wp³ywa na to, jak mocno odrzucaj¹ go wybuchy)
    public float gravity = 20.0f;
    public float jumpForce = 8.0f;
    public float friction = 6.0f;
    public bool movementLocked = false;
    public bool mounted = false;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float crouchTransitionSpeed = 10.0f;
    public float crouchMoveSpeed = 4.0f;

    [Header("Ground Movement")]
    public float maxGroundSpeed = 10.0f; 
    public float groundAccel = 10.0f;    
    public float stopSpeed = 2.0f;       

    [Header("Air Movement")]
    public float maxAirSpeed = 1.0f;     
    public float airAccel = 50.0f;
    public bool autoBhop = true;

    [Header("Jumping & Queue")]
    public float jumpQueueWindow = 0.1f;
    private float _jumpQueueTimer = 0f;

    // Wewnêtrzne zmienne
    private CharacterController _cc;
    private Transform _mainCameraTransform; 
    private Vector3 _playerVelocity = Vector3.zero;
    private Vector3 _wishDir = Vector3.zero;
    private bool _isGrounded;
    private bool _isCrouching;

    //Ukryte w inspektorze, ale nadal dostêpne publicznie dla innych skryptów
    [HideInInspector] public bool isGrounded => _isGrounded;

    [SerializeField] private CameraControll playerCamera;
    private PlayerStats myStats;
    void Awake()
    {
        myStats = GetComponent<PlayerStats>();

        if (GameManager.Instance != null)
        {
            Debug.Log("Movement Awake");
            GameManager.Instance.RegisterPlayer(this, myStats, playerCamera);
        }

        _cc = GetComponent<CharacterController>();
        _cc.height = standHeight;

        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Brak MainCamera w scenie! Skrypt nie bêdzie wiedzia³ gdzie jest przód.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(movementLocked) {
            return;
        }
        Movement();
    }

    void Movement()
    {
        RotatePlayerToCameraDirection();

        _isGrounded = _cc.isGrounded;

        HandleCrouch();

        // Input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        _wishDir = (transform.right * x + transform.forward * z);
        _wishDir.Normalize();

        // Skok (Queue + AutoBhop)
        if (Input.GetButtonDown("Jump"))
        {
            _jumpQueueTimer = jumpQueueWindow;
        }
        if (_jumpQueueTimer > 0)
        {
            _jumpQueueTimer -= Time.deltaTime;
        }

        bool jumpRequested = autoBhop ? Input.GetButton("Jump") : (_jumpQueueTimer > 0);

        if (_isGrounded && jumpRequested && !_isCrouching)
        {
            _playerVelocity.y = jumpForce;
            _jumpQueueTimer = 0f;
            _isGrounded = false;
        }

        // Ruch
        if (_isGrounded)
        {
            ApplyGroundMove();
        }
        else
        {
            ApplyAirMove();
        }

        // Grawitacja
        if (!_isGrounded)
        {
            _playerVelocity.y -= gravity * Time.deltaTime;
        }

        _cc.Move(_playerVelocity * Time.deltaTime);
    }

    // --- NOWA FUNKCJA: DODAWANIE SI£Y (IMPULS) ---
    /// <summary>
    /// Dodaje natychmiastow¹ si³ê do gracza (np. odrzut broni, wybuch).
    /// </summary>
    /// <param name="force">Wektor si³y (kierunek * moc)</param>
    public void AddImpulse(Vector3 force)
    {
        // a = F / m
        Vector3 acceleration = force / mass;
        _playerVelocity += acceleration;

        // Jeœli si³a wypycha nas w górê, musimy "odkleiæ" siê od ziemi,
        // w przeciwnym razie ApplyGroundMove w nastêpnej klatce wyzeruje nam prêdkoœæ Y.
        if (_playerVelocity.y > 0)
        {
            _isGrounded = false;
        }
    }



    // --- FUNKCJE POMOCNICZE ---

    private void RotatePlayerToCameraDirection()
    {
        if (_mainCameraTransform == null) return;
        Vector3 cameraEuler = _mainCameraTransform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, cameraEuler.y, 0);
    }

    private void HandleCrouch()
    {
        bool wishCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        _isCrouching = wishCrouch;
        float targetHeight = wishCrouch ? crouchHeight : standHeight;
        _cc.height = Mathf.Lerp(_cc.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        _cc.center = new Vector3(0,0,0);
    }

    private void ApplyGroundMove()
    {
        if (_playerVelocity.y < 0) _playerVelocity.y = -2f;
        ApplyFriction();
        float targetSpeed = _isCrouching ? crouchMoveSpeed : maxGroundSpeed;
        ApplyAccelerate(_wishDir, targetSpeed, groundAccel);
    }

    private void ApplyAirMove()
    {
        ApplyAccelerate(_wishDir, maxAirSpeed, airAccel);
    }

    private void ApplyAccelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(new Vector3(_playerVelocity.x, 0, _playerVelocity.z), wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0) return;
        float accelSpeed = accel * Time.deltaTime * wishSpeed;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;
        _playerVelocity.x += wishDir.x * accelSpeed;
        _playerVelocity.z += wishDir.z * accelSpeed;
    }

    private void ApplyFriction()
    {
        Vector3 vec = _playerVelocity;
        vec.y = 0;
        float speed = vec.magnitude;
        float drop = 0;

        if (speed < 0.01f)
        {
            _playerVelocity.x = 0;
            _playerVelocity.z = 0;
            return;
        }

        float control = (speed < stopSpeed) ? stopSpeed : speed;
        drop += control * friction * Time.deltaTime;

        float newSpeed = speed - drop;
        if (newSpeed < 0) newSpeed = 0;
        if (newSpeed > 0) newSpeed /= speed;

        _playerVelocity.x *= newSpeed;
        _playerVelocity.z *= newSpeed;
    }

    public float GetHorizontalSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(_playerVelocity.x, 0, _playerVelocity.z);
        return horizontalVelocity.magnitude;
    }

    public Vector3 GetHorizontalSpeedVector()
    {
        Vector3 horizontalVelocity = new Vector3(_playerVelocity.x, 0, _playerVelocity.z);
        return horizontalVelocity;
    }
}
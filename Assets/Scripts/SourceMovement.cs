using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SourceMovement : MonoBehaviour
{
    [Header("General Settings")]
    public float mass = 1.0f;           // Masa gracza (wpływa na to, jak mocno odrzucają go wybuchy)
    public float gravity = 20.0f;
    public float jumpForce = 8.0f;
    public float friction = 6.0f;
    public bool movementLocked = false;
    public bool mounted = false;

    [Header("Noclip Settings")]
    public bool isNoclip = false;
    public float noclipSpeed = 16f;
    public float noclipFastMultiplier = 2.5f;

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

    // Wewnętrzne zmienne
    private CharacterController _cc;
    private Transform _mainCameraTransform; 
    private Vector3 _playerVelocity = Vector3.zero;
    private Vector3 _wishDir = Vector3.zero;
    private bool _isGrounded;
    private bool _isCrouching;

    //Ukryte w inspektorze, ale nadal dostępne publicznie dla innych skryptów
    [HideInInspector] public bool isGrounded => isNoclip ? false : _isGrounded;

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
            Debug.LogError("Brak MainCamera w scenie! Skrypt nie będzie wiedział gdzie jest przód.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (movementLocked) {
            return;
        }
        Movement();
    }

    void Movement()
    {
        RotatePlayerToCameraDirection();

        // Obsługa noclip (latanie przez ściany)
        if (isNoclip)
        {
            HandleNoclip();
            return;
        }

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

    private void HandleNoclip()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = Vector3.zero;
        if (_mainCameraTransform != null)
        {
            moveDir += _mainCameraTransform.forward * z;
            moveDir += _mainCameraTransform.right * x;
        }
        else
        {
            moveDir += transform.forward * z + transform.right * x;
        }

        // Lot w górę / w dół
        if (Input.GetKey(KeyCode.Space))
        {
            moveDir += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
        {
            moveDir += Vector3.down;
        }

        float speed = noclipSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= noclipFastMultiplier;
        }

        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();
        }

        transform.position += moveDir * (speed * Time.deltaTime);
    }

    /// <summary>
    /// Przełącza tryb noclip (latanie przez ściany bez kolizji).
    /// </summary>
    public bool ToggleNoclip()
    {
        isNoclip = !isNoclip;
        if (isNoclip)
        {
            if (_cc != null) _cc.enabled = false;
            _playerVelocity = Vector3.zero;
        }
        else
        {
            if (_cc != null) _cc.enabled = true;
            _playerVelocity = Vector3.zero;
        }
        return isNoclip;
    }

    public void SetNoclip(bool state)
    {
        isNoclip = state;
        if (isNoclip)
        {
            if (_cc != null) _cc.enabled = false;
            _playerVelocity = Vector3.zero;
        }
        else
        {
            if (_cc != null) _cc.enabled = true;
            _playerVelocity = Vector3.zero;
        }
    }

    // --- FUNKCJE DODAWANIA I KONTROLI SIŁY/PRĘDKOŚCI ---

    /// <summary>
    /// Dodaje natychmiastową siłę do gracza (np. odrzut broni, wybuch).
    /// </summary>
    /// <param name="force">Wektor siły (kierunek * moc)</param>
    public void AddImpulse(Vector3 force)
    {
        // a = F / m
        Vector3 acceleration = force / mass;
        _playerVelocity += acceleration;

        // Jeśli siła wypycha nas w górę, musimy "odkleić" się od ziemi,
        // w przeciwnym razie ApplyGroundMove w następnej klatce wyzeruje nam prędkość Y.
        if (_playerVelocity.y > 0)
        {
            _isGrounded = false;
        }
    }

    /// <summary>
    /// Pobiera lub ustawia bieżącą prędkość gracza.
    /// </summary>
    public Vector3 PlayerVelocity
    {
        get => _playerVelocity;
        set => _playerVelocity = value;
    }

    /// <summary>
    /// Ustawia bezpośrednio prędkość gracza (np. lina/lasso, kontrolowany slingshot).
    /// </summary>
    public void SetVelocity(Vector3 velocity)
    {
        _playerVelocity = velocity;
        _isGrounded = false;
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
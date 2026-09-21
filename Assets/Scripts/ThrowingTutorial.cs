using UnityEngine;

public class ThrowingTutorial : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public PlayerStats playerStats;
    [Header("Settings")]
    
    //public int totalThrows;
    public float throwCooldown;

    [Header("Throwing")]
    
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    [SerializeField] private bool _isThrowingActive = true;

    private void Awake()
    {   
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
        {
            playerStats = GameManager.Instance.PlayerStats;
        }
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }
    void OnEnable()
    {
        LeftHandChanger.OnItemChanged += ActivateThrowing;
    }

    void OnDisable()
    {
        LeftHandChanger.OnItemChanged -= ActivateThrowing;
    }

    private void Start()
    {
        readyToThrow = true;
        if (playerStats == null)
        {
            playerStats = GameManager.Instance != null && GameManager.Instance.PlayerStats != null
                ? GameManager.Instance.PlayerStats
                : FindFirstObjectByType<PlayerStats>();
        }
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("LeftHand") && readyToThrow && playerStats.throwablesCount > 0 && _isThrowingActive)
        {
            Throw();
            GameManager.Instance.UpdateThrowablesCount();
        }
    }


    void ActivateThrowing(UltilityItemType leftHandItem)
    {
        if (leftHandItem == UltilityItemType.Dynamite)
        {
            _isThrowingActive = true;
        }
        else
        {
            _isThrowingActive = false;
        }
    }
    private void Throw()
    {
        readyToThrow = false;

       
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        // calculate direction bazując bezpośrednio na wektorze patrzenia kamery
        Vector3 aimDirection = cam != null ? cam.forward : transform.forward;

        if (cam != null && Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f))
        {
            if (hit.distance > 1.5f && attackPoint != null)
            {
                aimDirection = (hit.point - attackPoint.position).normalized;
            }
        }

        Vector3 moveDirection = Vector3.zero;
        if (GameManager.Instance != null && GameManager.Instance.PlayerRef != null)
        {
            moveDirection = GameManager.Instance.PlayerRef.GetHorizontalSpeedVector();
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                moveDirection = moveDirection.normalized;
            }
        }

        // Pełny wektor 3D kierunku patrzenia kamery (góra / dół / wprost)
        Vector3 throwDirection = aimDirection;

        // Opcjonalne subtelne dodanie pędu ruchu gracza (np. przy biegu w przód lub na boki)
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            throwDirection = Vector3.Lerp(throwDirection, (throwDirection + moveDirection * 0.35f).normalized, 0.25f).normalized;
        }

        // add force wzdłuż kierunku patrzenia kamery + lekki łuk do góry
        Vector3 forceToAdd = throwDirection * throwForce + Vector3.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        if (!playerStats.infiniteThrows)
            playerStats.throwablesCount--;

        // implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);
    }

   
    private void ResetThrow()
    {
        readyToThrow = true;
    }
}
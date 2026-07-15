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

    private void Start()
    {
        
        readyToThrow = true;
    }
    private void Awake()
    {   
        Debug.Log("Awake ThrowingTutorial");
        playerStats = GameManager.Instance.PlayerStats;
        cam = Camera.main.gameObject.transform;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Throw") && readyToThrow && playerStats.throwablesCount > 0)
        {
            Throw();
            GameManager.Instance.UpdateThrowablesCount();
        }
    }


    private void Throw()
    {
        readyToThrow = false;

       
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 aimDirection = cam.forward;
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

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            aimDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forwardDirection = Vector3.ProjectOnPlane(aimDirection, Vector3.up).normalized;
        if (forwardDirection.sqrMagnitude < 0.0001f)
        {
            forwardDirection = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        }

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            forwardDirection = Vector3.Lerp(forwardDirection, moveDirection, 0.35f).normalized;
        }

        // add force
        Vector3 forceToAdd = forwardDirection * throwForce + Vector3.up * throwUpwardForce;

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
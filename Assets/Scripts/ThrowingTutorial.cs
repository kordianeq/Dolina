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
        Vector3 forceDirection = cam.forward;

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + Vector3.up * throwUpwardForce + new Vector3(GameManager.Instance.PlayerRef.GetHorizontalSpeed(), 0,GameManager.Instance.PlayerRef.GetHorizontalSpeed());

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
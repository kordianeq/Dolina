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
    public KeyCode throwKey = KeyCode.Mouse0;
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
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();

    }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && playerStats.throwablesCount > 0)
        {
            Throw();
            GameManager.Instance.UpdateThrowablesCount();
        }
    }


    private void Throw()
    {
        readyToThrow = false;

        // instantiate object to throw
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + Vector3.up * throwUpwardForce + GameManager.Instance.PlayerRef.rb.linearVelocity.normalized;

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
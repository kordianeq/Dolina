using TMPro;
using UnityEngine;

public class HandPowers : MonoBehaviour
{

    [Header("References")]
    public GameObject player;

    PlayerStats playerStats;
    PlayerMovement playerMovement;

    public Camera cam;
     TextMeshProUGUI ammoText;

    [Header("RayShoot")]

    public int damage;
    public float delay;
    public float maxDistance;
    bool shooting;
    bool readyToShoot;

    [Header("Throwing")]

    public KeyCode throwing = KeyCode.Mouse1;

    public float throwForce, throwUpwardForce, throwCooldown;
    public GameObject objectToThrow;
    public Transform attackPoint;
    bool readyToThrow;

    public int explosionDamage, stationaryDamage;
    public float tickSpeed, timeOnGround;

    [Header("Shield")]
    public KeyCode shield = KeyCode.E;

    public GameObject shieldObj;
    public float shieldSpeed;
    public float damageReduction, shieldCooldown;
    bool readyToShield;

    
    
    // Start is called before the first frame update
    void Start()
    {
        playerStats = player.GetComponent<PlayerStats>();
        playerMovement = player.GetComponent<PlayerMovement>();
        ammoText = GameObject.Find("AmmoText").GetComponent<TextMeshProUGUI>();
        shieldObj.gameObject.SetActive(false);
    }
    private void Awake()
    {
        readyToThrow = true;
        readyToShoot = true;
        readyToShield = true;
    }
    // Update is called once per frame
    void Update()
    {
        
        ammoText.text = "Infinite ammo";
        MyInput();
    }

    void MyInput()
    {
        shooting = Input.GetKey(KeyCode.Mouse0);
        if (readyToShoot && shooting)
        {
            RayShoot();
        }
        //
        if( Input.GetKey(throwing) && readyToThrow)
        {
            Throw();
        }

        Shield();
    }
    void Shield()
    {
        //When button pressed and cooldown ready
        if (Input.GetKeyDown(shield) && readyToShield)
        {
            

            //logika tarczy do playera
            playerStats.DamageReduction(damageReduction);
            playerMovement.SpeedReduction(shieldSpeed);

            //Shield object appears in scene (visual effect)
            shieldObj.gameObject.SetActive(true);

           

        }
        else if(Input.GetKeyUp(shield) && readyToShield) //When button is no longer held
        {
            readyToShield = false;
            //default values
            
            playerStats.DamageReduction(0);
            playerMovement.SpeedReduction(playerMovement.defaultSpeed);

            shieldObj.gameObject.SetActive(false);

            //Cooldown
            Invoke(nameof(ShieldReset), shieldCooldown);
        }
        else if(Input.GetKeyDown(shield) && readyToShield == false)
        { 
            Debug.Log("ShieldNotReady"); 
        }
    }
    //Resets Shield Ability
    void ShieldReset()
    {
        readyToShield = true;
    }
    void RayShoot()
    {
        readyToShoot = false;
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxDistance))
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.gameObject.GetComponent<EnemyLogic>().ApplyDamage(damage);
            }
        }

        Invoke(nameof(TimeBetweenShooting), delay);
    }

    //Resets Shooting ability
    void TimeBetweenShooting()
    {
        readyToShoot = true;
        
    }

    private void Throw()
    {
        readyToThrow = false;

        // instantiate object to throw
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.transform.rotation);

        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        

        // implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);
    }
    
    //Resets throw ability
    private void ResetThrow()
    {
        readyToThrow = true;
    }
}

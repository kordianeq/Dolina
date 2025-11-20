using UnityEngine;
using UnityEngine.AI;

public class HorseAi : MonoBehaviour, IKickeable
{
    public float gunSightDistance;
    public int damage;

    public Animator horseAnimator;
    public NavMeshAgent agent;
    public Breakeable breakableScript;
    public Transform player;
    public NavMeshAgent navMeshAgent;
    public LayerMask whatIsGround, whatIsPlayer;

    RaycastHit hit;
    Rigidbody rb;
    //patrolling
    public Vector3 walkPoint;
    bool walkPointSet = false;
    public float walkPointRange;

    //Atacking
    public float timeBetweenAtacks;
    bool alreadyAttacked;
    bool kicked = false;
    public bool mounted = false;
    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    [Header("Kick Settings")]
    public float localKickForce = 10f;
    public float localUpKickForce = 5f;

    Horse horse;
    Animator animator;
    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        breakableScript = GetComponent<Breakeable>();
        horse = GetComponent<Horse>();
        animator = GetComponentInChildren<Animator>();
    }



    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        animator.SetFloat("speed", rb.linearVelocity.magnitude);


        if (!kicked)
        {
            if (horse.isDead)
            {
                navMeshAgent.enabled = false;
                breakableScript.enabled = false;
            }

            if (!mounted)
            {
                if (navMeshAgent.enabled == false && horse.isDead == false) navMeshAgent.enabled = true;
                if (breakableScript.enabled == true && horse.isDead == false) breakableScript.enabled = false;

                if (navMeshAgent.isOnNavMesh)
                {
                    if (!playerInSightRange && !playerInAttackRange)
                    {
                        // Debug.Log("Patrol");
                        Patroling();
                    }
                    else if (playerInSightRange && !playerInAttackRange)
                    {
                        //Debug.Log("Chase");
                        ChasePlayer();
                    }
                    else
                        if (playerInSightRange && playerInAttackRange)
                    {
                        AttackPlayer();
                        //Debug.Log("Attack");
                    }
                }
                else
                {

                }
            }
            else
            {
                navMeshAgent.enabled = false;
                breakableScript.enabled = false;
            }


        }
        else if (horse.isDead)
        {
            navMeshAgent.enabled = false;
            breakableScript.enabled = false;
        }
        else
        {

            breakableScript.enabled = true;
            navMeshAgent.enabled = false;
        }



    }

    public void KickHandle()
    {
        //Debug.Log("The horse has been kicked.");

    }

    public bool kickHandle(Vector3 from, float kickForc)
    {
        //Debug.Log("The horse has been kicked.");
        kicked = true;
        Invoke(nameof(KickReset), 5f);
        Vector3 flattened = Vector3.ProjectOnPlane(transform.position - from, Vector3.up);
        rb.AddForce(flattened * localKickForce, ForceMode.Impulse);
        rb.AddForce(Vector3.up * localUpKickForce, ForceMode.Impulse);
        horse.Damaged(horse.kickDamage);

        return true;
    }

    public void KickReset()
    {
        kicked = false;
    }
    public void AnimationAttack()
    {
        Debug.Log("DoDmg");
        if (playerInAttackRange)
        {
            //player.GetComponent<Stats>().TakeDamage(damage, 0);
        }
        else
        {
            Debug.Log("Damnn! I missed");
        }

    }
    void Patroling()
    {

        if (walkPointSet == false)
        {
            SearchWalkPoint();
        }

        if (walkPointSet == true)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            Eat();
        }

    }

    void Eat()
    {
        horseAnimator.SetBool("Eat", true);
        Invoke(nameof(EatingFinished), 3f);
    }
    public void EatingFinished()
    {
        horseAnimator.SetBool("Eat", false);
        walkPointSet = false;
    }

    void SearchWalkPoint()
    {

        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;

        }
        else
        {
            //Debug.LogWarning("Możliwe że nie ustawiono podłogi jako layer Ground");
        }

        //if(Physics.Raycast(transform.position, walkPoint, out hit))
        //{
        //    if(hit.collider != null)
        //    {
        //        Debug.Log("Terrain");
        //    }
        //}
    }
    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    void Stunned()
    {
        agent.SetDestination(transform.position);
    }
    void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        //transform.LookAt(player);
        if (!alreadyAttacked)
        {
            //animController.Attack();
            ///Attack code here


            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAtacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, sightRange);
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawRay(transform.position, player.position - transform.position);

    //}

    void DistanceToPlayer()
    {
        //Debug.Log(player.position);

        if (Physics.Raycast(transform.position, player.position, gunSightDistance, whatIsGround))
        {

            //jeœli œciana
            //Debug.Log("Wall in between");
        }
        else
        {
            if (Vector3.Distance(transform.position, player.position) <= gunSightDistance)
            {
                //Check rotation
            }
        }
    }


}

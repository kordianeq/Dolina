using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class KickConroller : MonoBehaviour
{
    public LayerMask KickMask;
    public LayerMask KickBlockMask;
    [SerializeField] private Transform KickStartPoint;
    [SerializeField] ParticleSystem particleEffect;
    [SerializeField] private float KickRadius;
    [SerializeField] private float kickRange;

    [SerializeField] private float dickDmg;

    [SerializeField] private float kickForce;

    [SerializeField] private Animator kickAnimator;
    [SerializeField] private float KickCooldown;

    [SerializeField] private float KickFramesStart;
    [SerializeField] private float KicFramesEnd;
    private float kickTimer = 0;
    private bool inKick;
    private bool kickSwitch = false;

    //random bulshit go
    [SerializeField] private SkinnedMeshRenderer KickMesh;


    // Start is called before the first frame update
    void Start()
    {
        inKick = false;
        kickSwitch = false;
    }

    // Update is called once per frame
    void Update()
    {
        // go for the kick
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!inKick)
            {
                StartCoroutine(Kick());
            }
        }


    }


    IEnumerator Kick()
    {
        kickAnimator.speed = 1;
        KickMesh.enabled = true;
        inKick = true;
        //randomfuckingchancetodosomethingcool
        int funny = Random.Range(0, 15);
        kickSwitch = !kickSwitch;
        kickTimer = KickCooldown;

        // correcting the hit angle to be flatter. like when pointing down it will stil kick things forward
        float kickangle = Vector3.Angle(KickStartPoint.forward,Vector3.up);
        if(kickangle > 90)
        {
            kickangle = Mathf.InverseLerp(90,120,kickangle);
        }else
        {
            kickangle = 0;
        }



        //Debug.Log("Kick ANGLE: " + kickangle);

        if (funny == 10)
        {
            kickAnimator.SetTrigger("KickFun");
            kickSwitch = !kickSwitch;
        }
        else
        {
            if (kickSwitch)
            {
                kickAnimator.SetTrigger("KickOne");
            }
            else
            {
                kickAnimator.SetTrigger("KickTwo");
            }
        }

        //forgor how to use coroutine but this should work i guess?

        yield return new WaitForSeconds(KickFramesStart);
        var kickedStuff = Physics.SphereCastAll(KickStartPoint.position, KickRadius, KickStartPoint.forward, kickRange, KickMask);
        foreach (var kicked in kickedStuff)
        {
            bool noInterface = true;
            // sometimes for some reason hit point can be equal to zero, idk why
            Vector3 hitPos = kicked.point;
            if (hitPos == Vector3.zero)
            {
                hitPos = kicked.collider.gameObject.transform.position;
            }

            // now check if kicked things are accually possible to be kicked. like not behind a wall or smth

            
            if (Physics.Raycast(hitPos, KickStartPoint.position - hitPos, kickRange, KickBlockMask))
            {
                Debug.DrawLine(hitPos, KickStartPoint.position, Color.red, 1f);
            }
            else
            {
                Debug.DrawLine(hitPos, KickStartPoint.position, Color.green, 1f);
                Debug.DrawRay(hitPos, Vector3.up, Color.yellow, 1f);


                if (kicked.collider.gameObject.TryGetComponent<IKickeable>(out IKickeable tryKick))
                {
                    if (!tryKick.KickHandleButMorePrecize(KickStartPoint.position))
                    { tryKick.KickHandle(); }
                    noInterface = false;
                }
                else if (kicked.collider.gameObject.TryGetComponent<IDamagable>(out IDamagable tryDmg))
                {
                    tryDmg.Damaged(dickDmg);
                    
                }

                if (kicked.collider.gameObject.TryGetComponent<IKnockback>(out IKnockback tryKnockback))
                {
                    tryKnockback.KnockbackHandle(transform.position, kickForce);
                    noInterface = false;
                }
                
                

                if (noInterface)
                {
                    if (kicked.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rbody))
                    {
                        //corrected kick angle
                        Vector3 flattened = Vector3.ProjectOnPlane(KickStartPoint.forward, Vector3.up);
                        flattened = Vector3.Lerp(KickStartPoint.forward, flattened, kickangle);
                        Debug.DrawRay(KickStartPoint.position + new Vector3(0, 0.3f, 0), flattened, Color.white, 3f);
                        rbody.AddForceAtPosition(flattened * kickForce, hitPos, ForceMode.Impulse);
                        //standard one
                        //rbody.AddForceAtPosition(KickStartPoint.forward * kickForce,hitPos,ForceMode.Impulse);
                    }
                }


                particleEffect.gameObject.transform.position = hitPos;
                particleEffect.Emit(1);
                
                
            }





           // Debug.Log(kicked.collider.gameObject + " + " + hitPos);
        }

        yield return new WaitForSeconds(KicFramesEnd);
        KickMesh.enabled = false;





        kickAnimator.speed = 0;
        yield return null;
        inKick = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(KickStartPoint.position, KickRadius);
        Gizmos.DrawSphere(KickStartPoint.position + KickStartPoint.forward * kickRange, KickRadius);
    }
}

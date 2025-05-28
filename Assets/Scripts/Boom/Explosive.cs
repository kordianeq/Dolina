using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float explosionRadius;
    public float explosionDmg;
    public float explosionForce;
    public Transform effect;
    public float effectDuration;

    public AnimationCurve explCore;
    public Transform explCr;

    public AnimationCurve secondexplCore;
    public Transform secondexplCr;

    public AnimationCurve thirdexplCore;
    public Transform thirdexplCr;

    public float explTimer,explTime;

    public bool Once = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Once)
        {
        var inRadius = Physics.OverlapSphere(transform.position,explosionRadius);
        foreach (var expl in inRadius)
        {
            var rb = expl.gameObject.GetComponent<Rigidbody>();
            if(rb!=null)
            {
                rb.AddForce((expl.transform.position - transform.position) *explosionForce,ForceMode.Impulse);
            }

            if(expl.gameObject.TryGetComponent<IiBoomeable>(out IiBoomeable tryBoom))
            {
                tryBoom.MakeBoom(explosionDmg);
            }
            //CHECK INTERFACES
            if(expl.gameObject.TryGetComponent<IDamagable>(out IDamagable tryDmg))
            {
                tryDmg.Damaged(explosionDmg);
            }

            

        }
        Once = false;
        }

        effect.localScale = Vector3.one*explosionRadius*1.5f;
        if(explTimer < explTime)
        {
            explTimer+=Time.deltaTime;
            float timescl = explCore.Evaluate(explTimer/explTime);
            explCr.localScale = Vector3.one * timescl * explosionRadius;

            float timescltwo = secondexplCore.Evaluate(explTimer/explTime);
            secondexplCr.localScale = Vector3.one * timescltwo * explosionRadius;

            float timesclthre = thirdexplCore.Evaluate(explTimer/explTime);
            thirdexplCr.localScale = new Vector3(1,0,1) * timesclthre * explosionRadius + new Vector3(0,0.2f,0);

        }else
        {
            Destroy(gameObject);
        }

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,explosionRadius);
    }
}

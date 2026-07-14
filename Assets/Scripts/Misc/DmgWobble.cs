using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgWobble : MonoBehaviour
{
    [SerializeField] Transform wobbleTarget;
    [SerializeField] float wobbleTime;
    [SerializeField] AnimationCurve squash;
    [SerializeField] AnimationCurve stretch;
    [SerializeField]  float effectMultiplier;
    Vector3 restScale;
    bool running;


    void Awake()
    {
        if (wobbleTarget == null)
        {
            wobbleTarget = gameObject.transform;
        }

        restScale = wobbleTarget.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void MakeWobble()
    { 
        if (running)
        {

        }
        else
        {
            StartCoroutine(Wobble());
        }
    }

    public IEnumerator Wobble()
    {
        float wTimer = wobbleTime;
        float eval = 0;
        float w = 0;
        float h = 0;
        while (wTimer > 0)
        {
            eval = Mathf.InverseLerp(0, wobbleTime, wTimer);

            h = ((stretch.Evaluate(eval) * 2) - 1) * effectMultiplier;
            w = ((squash.Evaluate(eval) * 2) - 1) * effectMultiplier;
            wobbleTarget.localScale = new Vector3(restScale.x + w, restScale.y + h, restScale.z + w);

            wTimer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        wobbleTarget.localScale = restScale;


        yield return null;
    }
}

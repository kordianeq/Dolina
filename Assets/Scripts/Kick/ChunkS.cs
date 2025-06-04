using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkS : MonoBehaviour
{
    // Start is called before the first frame update
    public float lifetime;
    public float timeVariation;
    private float lifetimer = 0;
    Vector3 initialScale;
    public float despawntime;
    private float despawntimer = 0;
    void Start()
    {
        initialScale = transform.localScale;
        lifetime += Random.Range(0,timeVariation);
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetimer < lifetime)
        {
            lifetimer += Time.deltaTime;
        }
        else
        {
            if (despawntimer < despawntime)
            {
                despawntimer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero,despawntimer / despawntime);
            }else
            {
                Destroy(gameObject);
            }
        }
    }
}

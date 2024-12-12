using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buuuulet : MonoBehaviour
{
    // dont use this shit
    //only placeholder
    // Start is called before the first frame update
    public float flyspeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position -= transform.forward * Time.deltaTime * flyspeed;
        Destroy(gameObject,5f);
    }
}

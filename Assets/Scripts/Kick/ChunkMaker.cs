using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMaker : MonoBehaviour
{
    [SerializeField] Transform[] chunks;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void GoAndBreak(Rigidbody rb)
    {

        chunks = GetComponentsInChildren<Transform>();
        for (int i = 1; i < chunks.Length; i++)
            {
                chunks[i].transform.parent = null;
                chunks[i].GetComponent<Rigidbody>().velocity = rb.velocity;
            }
        Destroy(gameObject);

        /*

        foreach (GameObject cHolder in chunksToMake)
        {
            GameObject chunk = Instantiate(cHolder, transform);
            var parts = chunk.GetComponentsInChildren<Transform>();
            Debug.Log("FUCK "+ parts.Length);
            for (int i = 1; i < parts.Length; i++)
            {
                 Debug.Log("err"+ parts[i].gameObject);
                parts[i].transform.parent = null;
                parts[i].GetComponent<Rigidbody>().velocity = rb.velocity;
            }

            Destroy(chunk);
        }*/

        //Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefebScatterer : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> chunks;
    public int minChunkRange,maxchunkRange;
    public float speedo,ejectForce,spawnRadius;
    private int chunkCount;
    void Start()
    {
        chunkCount = Random.Range(minChunkRange,maxchunkRange);
        for(int i=0;i<chunkCount;i++)
        {
            GameObject inst = Instantiate(chunks[Random.Range(0,chunks.Count)],Random.insideUnitSphere * spawnRadius+ transform.position,transform.rotation);
            inst.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere.normalized*speedo,ForceMode.Impulse);
            
            inst.GetComponent<Rigidbody>().AddForce((transform.position-inst.transform.position).normalized* ejectForce,ForceMode.Impulse);
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

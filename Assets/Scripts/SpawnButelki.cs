using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SpawnButelki : MonoBehaviour
{
    public GameObject butelkaPrefab;
    
    ButelkiStats butelkiStats;

    bool isSpawned;
    public bool autoSpawn;

    
    
    public float spawnDealy;    
    public List<GameObject> spawnPoints;

    int currentPoint;

    
    void Start()
    {
        butelkiStats = gameObject.GetComponent<ButelkiStats>();
        SpawnBottle();
        
    }

  

    public void SpawnBottle()
    {
        currentPoint = Random.Range(0, spawnPoints.Count-1);
        
        
        
        Instantiate(butelkaPrefab, spawnPoints[currentPoint].transform.position, Quaternion.Euler(0,0,0));

        
        isSpawned = true;

        if (autoSpawn && butelkiStats.spawnButelki)
        {

            Invoke(nameof(SpawnReset), spawnDealy);
        }
       
    }
    
  
    void SpawnReset()
    {
        if (autoSpawn && butelkiStats.spawnButelki)
        {
            SpawnBottle();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UiExpansionPack : MonoBehaviour
{
    float mouseX,mouseY;
    [SerializeField] GameObject prefabToInstantiate;
    [SerializeField] Transform parentTransform;
    public bool spawnEnabled = false; // Flag to control spawning
    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && spawnEnabled) // Check if the left mouse button is pressed
        {
            SpawnBuletHit();
        }

       
    }

    void SpawnBuletHit()
    {
        // Capture the mouse position when the mouse button is pressed down
        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;

        Vector3 spawnLocation = new Vector3(mouseX, mouseY, 0);

        float randomZ = Random.Range(0, 360);
        float scale = Random.Range(0.6f, 1.2f);
        var bulletHit =  Instantiate(prefabToInstantiate, spawnLocation, Quaternion.Euler(0, 0, randomZ), parentTransform);
        bulletHit.transform.localScale = new Vector3(scale,scale,scale);
    }

    public void BuletsCleanUp()
    {
        // Clean up bullets after a certain time or condition
        // This method can be called based on your game logic
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject, 0); // Destroy after 2 seconds
        }
    }
}

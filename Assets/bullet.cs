using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float destroyTime = 10f; // Time in seconds before the bullet is destroyed
    private void Awake()
    {
        Invoke("DestroyBullet", destroyTime);
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}

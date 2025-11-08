using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chceckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.SaveButton();
    }
}

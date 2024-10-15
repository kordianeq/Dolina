using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject canvas;
    UiMenager menager;
    public int sceneId;

    void Start()
    {
        menager = canvas.GetComponent<UiMenager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        menager.OnChangeScene(sceneId);
    }
}

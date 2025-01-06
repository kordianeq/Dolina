using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwietoscDependent : MonoBehaviour
{
    PlayerStats stats;
    [SerializeField] GameObject child;
    public float changeLevel;
    public bool flip;
    // Start is called before the first frame update
    void Start()
    {
        stats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(stats.swietosc);
        if (flip)
        {


            if (stats.swietosc >= changeLevel)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
        else
        {
            if (stats.swietosc <= changeLevel)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
    }
}

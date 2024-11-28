using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoUp : MonoBehaviour
{
    Rigidbody ButelkaRB;
    public float upwardForce, sideForce;
    Transform cam;
    ButelkiStats butelkiStats;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Focus").GetComponent<Transform>();

        ButelkaRB = GetComponent<Rigidbody>();

        transform.LookAt(cam);
        
        ButelkaRB.AddForce(Vector3.up * upwardForce + transform.forward * sideForce, ForceMode.Impulse);

        butelkiStats = GameObject.Find("ButelkiCam").GetComponent<ButelkiStats>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            butelkiStats.UpdateStats();
            Destroy(gameObject);
        }
    }

}

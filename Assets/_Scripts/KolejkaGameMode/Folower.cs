using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class Folower : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed = 5;

    float distanceTravelled;


    // Update is called once per frame
    void Update()
    {
        distanceTravelled += speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);

        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
    }

}

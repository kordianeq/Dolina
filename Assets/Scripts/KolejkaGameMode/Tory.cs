using System.Collections.Generic;
using UnityEngine;

public class Tory : MonoBehaviour
{
    public List<Transform> waypoints; // List of empty GameObjects (waypoints)
    public float speed = 5f;          // Movement speed
    public bool loop = false;         // Loop back to the first point

    private int currentWaypointIndex = 0; // Tracks the current waypoint
    private float t = 0f;                 // Time factor for interpolation
    bool keepGoing;
    void Update()
    {
        if (waypoints.Count < 2)
        {

        }
        else
        {
            if (keepGoing)
            {
                CalculatePath();
            }
            
        }

    }
    void CalculatePath()
    {
        Vector3 startPoint = waypoints[currentWaypointIndex].position;
        Vector3 endPoint;
        Vector3 midPoint;
        if (currentWaypointIndex + 2 <= waypoints.Count - 1)  // nie jestem pewien jak to jest liczone mo¿e byæ b³¹d w skali listy 
        {
            midPoint = waypoints[(currentWaypointIndex + 1) % waypoints.Count].position;
            endPoint = waypoints[(currentWaypointIndex + 2) % waypoints.Count].position;

            t += Time.deltaTime * speed / Vector3.Distance(startPoint, endPoint);
            
        }
        else
        {
            endPoint = waypoints[(currentWaypointIndex + 1) % waypoints.Count].position;

            
            t += Time.deltaTime * speed / Vector3.Distance(startPoint, endPoint);

            transform.position = Vector3.Lerp(startPoint, endPoint, t); // Linear interpolation

            // If we've reached the end of this segment, move to the next
            if (t >= 1f)
            {
                t = 0f; // Reset time factor
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Count - 1)
                {
                    keepGoing = false;
                   
                }
            }
        }
        

    }

    // Optionally, draw gizmos in the editor to visualize the path
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (loop)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }
    }

   

}

using UnityEngine;
using UnityEngine.AI;

public class NpcAiUtilitiesBase : MonoBehaviour
{
    public class Target
    {
        //yes i know this is bad
        private Transform positionReference;
        public Vector3 position { get; private set; }
        private Transform origin;
        private LayerMask _rayLayer;
        public bool lost; 
        public bool inLineOfSight {get;private set;}

        public float GetDistance()
        {
            return Vector3.Distance(origin.position, positionReference.position);
        }

        public bool IsinProximity(float _range)
        {
            float dist = Vector3.Distance(origin.position, positionReference.position);
            if (dist < _range)
            {

                Debug.DrawLine(origin.position - new Vector3(0, 0.1f, 0), positionReference.position - new Vector3(0, 0.1f, 0), Color.green);
                return true;
            }
            else
            {
                inLineOfSight = false;
                Debug.DrawLine(origin.position,positionReference.position, Color.yellow);
                Debug.DrawRay(origin.position + new Vector3(0, 0.1f, 0), (positionReference.position- origin.position + new Vector3(0, 0.1f, 0)) * _range / dist, Color.red);
                return false;
            }

        }
        public bool IsInLineOfSight(float _range)
        {
            if (Physics.Raycast(origin.position, positionReference.position - origin.position, out RaycastHit hit, _range, _rayLayer))
            {                
                if(hit.transform.tag == "Player")
                {
                    Debug.DrawLine(positionReference.position, origin.position, Color.green);
                    inLineOfSight = true;
                    return true;

                }
                inLineOfSight = false;
                Debug.DrawRay(origin.position, positionReference.position - origin.position, Color.red);
                Debug.DrawRay(hit.point,Vector3.up,Color.red);
                return false;
            }
            else
            {
                inLineOfSight = false;
                Debug.DrawRay(origin.position, positionReference.position - origin.position, Color.red);
                return false;
                //Debug.DrawLine(positionReference.position, origin.position, Color.green);                
                //return true;
            }
        }
        public void SetTarget(Transform _v)
        {
            positionReference = _v;
            position = positionReference.position;
        }

        public void UpdateTargetPosition()
        {
            position = positionReference.position;
        }

        public Vector3 GetTargetPosition()
        {
            return position;
        }

        public Target(Transform _origin, LayerMask _detectionLayer)
        {
            origin = _origin;
            _rayLayer = _detectionLayer;
        }
    }
    [SerializeField] public Target target;
    [SerializeField] Transform eyeOffset;
    [SerializeField] LayerMask detectionLayer;

   

    void Awake()
    {
        target = new Target(eyeOffset, detectionLayer);
        target.SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
    }

    public Vector3 NavCalc()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
        if (Vector3.Distance(this.transform.parent.position, this.transform.position) > 1)
        {
            // nav.Warp(this.transform.parent.position);
        }
        //ag.Warp(this.transform.parent.position);
        //nav.destination = target;
        //Debug.Log(ag.path.corners[0]);

        if (path.corners.Length > 1)
        {
            // Debug.Log(nav.path.corners[1] - transform.position);
            Debug.DrawRay(path.corners[1], Vector3.up);
            Debug.DrawRay(transform.position, (path.corners[1] - transform.position).normalized);
            return (path.corners[1] - transform.position).normalized;
        }
        return Vector3.zero;
    }


    public float DetectionRangeBase;

}

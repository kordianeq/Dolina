using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class NpcBehaviorBrain : NpcBrainMaster
{
    [SerializeField] public NpcMovementBrain moveBrain;
    [SerializeField] public State defaultState;
    [SerializeField] public State stunnState;
    [SerializeField] public float giveUpTime;
    public State deathState;
    [SerializeField] private int hp;

    [SerializeField] Transform eyeOffset;
    [SerializeField] LayerMask detectionLayer;
    private bool isAgroed;

    public class Target
    {
        //yes i know this is bad
        private Transform positionReference;
        public Vector3 position { get; private set; }
        private Transform origin;
        private LayerMask _rayLayer;
        public bool lost; 
        public bool inLineOfSight {get;private set;}
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

        public Target(Transform _origin, LayerMask _detectionLayer)
        {
            origin = _origin;
            _rayLayer = _detectionLayer;
        }
    }
    [SerializeField] public Target target;

    void Awake()
    {
        //initiate target or smth     
        target = new Target(eyeOffset, detectionLayer);


        target.SetTarget(GameObject.FindGameObjectWithTag("Player").transform);

        //initial set up of all states for state machine
        SetupInstances();
        sMachine.Set(defaultState, sMachine);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.DoBranch();
        //Debug.Log(currentState.childState);
    }

    private void FixedUpdate()
    {
        currentState.FixedDoBranch();
    }

    void Die()
    {
        Debug.Log("Dead i am");
    }

    public void TakeDmg(int _dmg)
    {
        isAgroed = true;
        hp -= _dmg;
        if (hp <= 0)
        {
            Die();
        }
    }
    public bool IsAgroed()
    {
        return isAgroed;
    }


    public override void ForceDeadState()
    {
        Debug.Log("i am dead now");
        sMachine.Set(deathState, sMachine,true);
    }

    public override void ForceResurectState()
    {
        Debug.Log("wtf, i resurected");
        sMachine.Set(defaultState, sMachine,true);
    }

    public override void ForceStunnState()
    {
        Debug.Log("i got stunned");
        sMachine.Set(stunnState, sMachine,true);
    }


    public void SimpleTargetDetec()
    {

    }

}


//trash bin
//currentState.Change(startState);
//currentState.Change(othersrtatsa);
//currentState
//need to fix,need to figure out what the fuck i did 
//currentState = startState; nevermind doesnt work

//wtf xd
/*
if(isAgroed)
{
    isAgroed = false;
    return true;
}
return false;*/

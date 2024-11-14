using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedState : State
{
    public WeaponCore weaponT;
    public Transform target;
    public Mod_State moveMode;
    public override void Enter()
    {
        Debug.Log("IMA GO RANGED");
        SetChild(moveMode);
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {   
        Debug.Log("ranging");

        moveMode.target = target.position;

        //IDK that was stupid
        /*if(childState is Mod_State)
        {
            //this is fucking stupid, need to ask łukasz or Piotrek
            Mod_State st = (Mod_State)childState;
            st.target = target.position;

        }*/



    }
    public override void Exit() { }
}

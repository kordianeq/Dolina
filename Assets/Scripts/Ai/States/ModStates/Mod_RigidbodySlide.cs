using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Mod_RigidbodySlide : Mod_State
{
    public override void Enter()
    {
        Debug.Log("Entered Chase MovementMode");
    }
    public override void Do()
    {

    }
    public override void FixedDo()
    {
        Debug.Log("go brrrrrrrrrrrrrrrr");
        core.rBody.AddForce((target * speed)-core.rBody.velocity,ForceMode.Acceleration);
        //core.eBody.AddForce(Vector3.up * 100f);
    }
    public override void Exit()
    {
        Debug.Log("Fun is over");
    }
}

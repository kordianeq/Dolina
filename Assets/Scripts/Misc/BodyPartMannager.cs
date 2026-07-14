using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BodyPart
{
    public bool broken;
    public bool unbreakeable;
    public float partHp;
    public float partDmgReduction;
    public float partHpMultiplier;
    public PropPart partName;

    public float CalculatePartDmg(float dmg)
    {
        partHp -= dmg * partHpMultiplier;
        return dmg * partDmgReduction;
    }
}

public class BodyPartMannager : MonoBehaviour
{

    [SerializeField] public List<BodyPart> bodyParts;


}

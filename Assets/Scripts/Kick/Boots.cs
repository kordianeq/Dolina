using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Boots", order = 1)]
public class Boots : ScriptableObject
{
    public BootsType bootsType;
    public float kickForceModifier;
    public float kickDamageModifier;
    public float kickRangeModifier;

    public bool inflictBleedEffect;
    public float bleedDamagePerTick;
    public bool inflictStunEffect;
    public float stunDuration;

    [Header("Visuals")]
    public Sprite bootSprite;
    public GameObject bootMesh;
}
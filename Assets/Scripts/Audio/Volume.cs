using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Volume", order = 1)]
public class Volume : ScriptableObject
{
    public float currentVolume;
}

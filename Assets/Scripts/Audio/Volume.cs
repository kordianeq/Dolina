using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/VolumeData", order = 1)]
public class VolumeData : ScriptableObject
{
    public float currentVolume;
}

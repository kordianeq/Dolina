using UnityEngine;
using System.Collections.Generic;

public class HorseGroupManager : MonoBehaviour
{
    public List<HorseAi> horsesInGroup;
    
    
    void Awake()
    {
        UpdateHorseList();
    }

    

    void UpdateHorseList()
    {
        foreach(HorseAi horse in transform.parent)
        {
            horsesInGroup.Add(horse);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HorseGroupManager : MonoBehaviour
{
    public List<HorseAi> horsesInGroup;
    public Transform clostestWaterSource;
    GameObject[] waterSources;
    void Awake()
    {
        UpdateHorseList();
        if(clostestWaterSource == null)
        {   
            waterSources = GameObject.FindGameObjectsWithTag("Water");
            foreach (GameObject water in waterSources)
            {
                //Do naprawy potem
                clostestWaterSource = water.transform;
            }
        }
    }

    

    void UpdateHorseList()
    {
        foreach(HorseAi horse in transform.parent)
        {
            horsesInGroup.Add(horse);
        }
    }
}

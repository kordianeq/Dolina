using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickIntroTrigger : MonoBehaviour, IKickeable
{

    public List<Animator> animsToplay;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Animator a in animsToplay)
        {
            a.speed = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void KickHandle()
    {
        foreach (Animator a in animsToplay)
        {
            
            a.speed = 1;
        }
    }
}

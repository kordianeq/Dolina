using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropPart { Head,Torso, HandL, HandR, LegL, LegR, Props,Bottle }

[System.Serializable]
public class FacePartControll
{
    public bool Overriden;
    public Transform controllBone;
    [SerializeField] public List<AnimPart> animPart;

    public void SetUp()
    {
        if (!Overriden)
        {
            foreach (AnimPart ani in animPart)
            {
                if (controllBone.localPosition.y >= ani.min && controllBone.localPosition.y < ani.max)
                {
                    ani.animatedPart.SetActive(true);
                }
                else
                {
                    ani.animatedPart.SetActive(false);
                }
            }
        }

    }

    public void ForceOne(int id)
    {
        Overriden = true;
        if (id < 0 || id > animPart.Count)
        {

            foreach (AnimPart ani in animPart)
            {

                ani.animatedPart.SetActive(false);

            }

        }
        else
        {
            for (int i = 0; i < animPart.Count; i++)
            {
                if (i == id)
                {
                    animPart[i].animatedPart.SetActive(true);
                }
                else
                {
                    animPart[i].animatedPart.SetActive(false);
                }
            }
        }
    }
}
[System.Serializable]
public class AnimPart
{
    public GameObject animatedPart;
    public float min, max;

    public AnimPart(GameObject _animatedPart, float _min, float _max)
    {
        animatedPart = _animatedPart;
        min = _min;
        max = _max;
    }
}

[System.Serializable]
public class PropControll // this shit is unsafe to use, for now treat it as a placeholder. i will make a better version later (never)
{
    [SerializeField] public PropPart part;
    [SerializeField] public List<GameObject> prop;
    [SerializeField] public GameObject particleEf;
    public void SetProp(int id)
    {
        for (int i = 0; i < prop.Count; i++)
        {
            if (i == id)
            {
                prop[i].SetActive(true);
            }
            else
            {
                prop[i].SetActive(false);
            }
        }
    }

    public void ForceParticle()
    {
        if (particleEf!=null)
        {
            particleEf.SetActive(true);
        }
    }
}

public class RigMiscCore : MonoBehaviour
{
    [SerializeField] public List<PropControll> propControll;
    [SerializeField] public List<FacePartControll> animPart;
    // Start is called before the first frame update
    void Start()
    {
        /*
        propControll[0].SetProp(1);
        propControll[0].ForceParticle();
        animPart[1].ForceOne(-1);
        animPart[2].ForceOne(-1);*/
    }

    // Update is called once per frame
    void Update()
    {
        foreach (FacePartControll anim in animPart)
        {
            anim.SetUp();
        }
    }

    public PropControll GetProp(PropPart p)
    {
        foreach (PropControll pro in propControll)
        {
            if (pro.part == p)
            {
                return pro;
            }
        }

        return null;
    }
}

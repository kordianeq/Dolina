using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum RecType {dmg,kick,stunn};
public interface IReceiver
{
    public void Receive(PropPart bodyPart, float dmg,float force,Vector3 direction);
}

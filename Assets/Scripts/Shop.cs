using UnityEngine;
using Unity.Cinemachine;

public class Shop : MonoBehaviour, IInteracted
{
    public CinemachineCamera shopCamera;

   
    public void NewInteraction()
    {
         shopCamera.Priority = 20;
         GameManager.Instance.Shopping(true);

    }

    void FixedUpdate()
    {
        if(GameManager.Instance.isShopping == false)
        {
            shopCamera.Priority = -1;
        }
    }


}

  

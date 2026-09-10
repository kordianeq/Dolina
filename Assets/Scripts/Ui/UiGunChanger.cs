using UnityEngine;

public class UiGunChanger : MonoBehaviour
{
    public void SelectWeapon(int currentGunIndex)
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == currentGunIndex)
                weapon.gameObject.SetActive(true);
            else
            {
                

                weapon.gameObject.SetActive(false);
                
            }

            i++;
        }
    }
}

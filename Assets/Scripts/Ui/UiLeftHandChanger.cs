using UnityEditor.Playables;
using UnityEngine;
using System;
public enum UltilityItemType
{
    Dynamite = 0,
    Lasso = 1
}
public class LeftHandChanger : MonoBehaviour
{
    public UltilityItemType currentItem = UltilityItemType.Dynamite;
    
    public static event Action<UltilityItemType> OnItemChanged;
    void Awake()
    {
        SelectItem((int)currentItem);
    }
    void Update()
    {
        if(Input.GetButtonDown("SwitchLeftHand"))
        {
            if((int)currentItem >= Enum.GetValues(typeof(UltilityItemType)).Length - 1)
            {
                currentItem = 0;
            }
            else
            {
                currentItem++;
            }
            SelectItem((int)currentItem);
            OnItemChanged?.Invoke(currentItem);
        }
    }
   public void SelectItem(int currentItemIndex)
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == currentItemIndex)
                weapon.gameObject.SetActive(true);
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
}

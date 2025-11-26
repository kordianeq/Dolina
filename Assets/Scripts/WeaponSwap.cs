using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    public int selectedWeapon = 0;
    public int cursorIndex = 0;


    private void Awake()
    {
        // Zamelduj wszystkie komponenty gracza w GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterWeapons(gameObject,this);
        }
    }
    private void Start()
    {
        SelectWeapon();
    }
    private void Update()
    {
        int previousSelected = selectedWeapon;

        ButtonSelect();
        CycleSelect();

        if (previousSelected == selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void CycleSelect()
    {
        if (Input.GetAxis("NextWeapon") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        else if (Input.GetAxis("NextWeapon") < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }
    }
    void ButtonSelect()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            selectedWeapon = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
        {
            selectedWeapon = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && transform.childCount >= 4)
        {
            selectedWeapon = 3;
        }
    }
    void AutoReload()
    {

    }
    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
            {
                weapon.GetComponent<GunSystem>().InstantReload();

                weapon.gameObject.SetActive(false);
                

            }

            i++;
        }
    }
    public void Save(ref WeaponSlot saveData)
    {
        saveData.currentWeapon = selectedWeapon;
        //saveData.weaponInUse = 
    }

    public void Load(WeaponSlot saveData)
    {
        selectedWeapon = saveData.currentWeapon;
    }
}
[System.Serializable]

public struct WeaponSlot
{
    public int currentWeapon;
    // public GameObject weaponInUse;
}



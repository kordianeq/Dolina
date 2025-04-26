using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem
{
    private static SaveData _saveData = new SaveData();
    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerData;
        public WeaponSlot WeaponSlotData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();
        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData,true));
    }

    private static void HandleSaveData() 
    {
        GameManager.Instance.playerStats.Save(ref _saveData.PlayerData);
        GameManager.Instance.weapons.Save(ref _saveData.WeaponSlotData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());
        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadData();
    }
    public static void HandleLoadData()
    {
        GameManager.Instance.playerStats.Load(_saveData.PlayerData);
        GameManager.Instance.weapons.Load(_saveData.WeaponSlotData);
    }
}

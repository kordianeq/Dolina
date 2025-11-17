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
        public GunSaveData GunSaveData;
        public SceneEnemyData EnemyData; 
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
        GameManager.Instance.PlayerStats.Save(ref _saveData.PlayerData);
        GameManager.Instance.Weapons.Save(ref _saveData.WeaponSlotData);

        EnemiesManager enemiesManager = GameManager.FindAnyObjectByType<EnemiesManager>();
        if (enemiesManager != null)
        {
            enemiesManager.Save(ref _saveData.EnemyData);
        }
        else
        {
            Debug.LogWarning("EnemiesManager not found in the scene. Cannot load enemy data.");
        }


    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());
        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadData();
    }
    public static void HandleLoadData()
    {
        GameManager.Instance.PlayerStats.Load(_saveData.PlayerData);
        GameManager.Instance.Weapons.Load(_saveData.WeaponSlotData);

        EnemiesManager  enemiesManager = GameManager.FindAnyObjectByType<EnemiesManager>();
        if (enemiesManager != null)
        {
            enemiesManager.Load(_saveData.EnemyData);
        }
        else
        {
            Debug.LogWarning("EnemiesManager not found in the scene. Cannot load enemy data.");
        }
    }
}

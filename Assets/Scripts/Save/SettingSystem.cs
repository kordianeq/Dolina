using UnityEngine;
using System.IO;

public static class SettingsSystem
{
    // Struktura przechowujaca wszystkie ustawienia gry
    [System.Serializable]
    public struct SettingsData
    {
        public float mouseSensitivity;
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float ambientVolume;
        public bool isFullscreen;
    }

    // Aktualne ustawienia zaladowane w grze (wartosci domyslne)
    public static SettingsData currentSettings = new SettingsData
    {
        mouseSensitivity = 2.0f,
        masterVolume = 1.0f,
        musicVolume = 1.0f,
        sfxVolume = 1.0f,
        ambientVolume = 1.0f,
        isFullscreen = true
    };

    private static string GetFilePath()
    {
        return Application.persistentDataPath + "/settings.json";
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log("Ustawienia zapisane do: " + GetFilePath());
    }

    public static void Load()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            string saveContent = File.ReadAllText(path);
            currentSettings = JsonUtility.FromJson<SettingsData>(saveContent);

            // Zabezpieczenie przed wartosciami 0 przy migracji ze starszej wersji pliku settings.json
            if (!saveContent.Contains("\"musicVolume\"") || currentSettings.musicVolume <= 0f)
            {
                currentSettings.musicVolume = 1.0f;
            }
            if (!saveContent.Contains("\"sfxVolume\"") || currentSettings.sfxVolume <= 0f)
            {
                currentSettings.sfxVolume = 1.0f;
            }
            if (!saveContent.Contains("\"ambientVolume\"") || currentSettings.ambientVolume <= 0f)
            {
                currentSettings.ambientVolume = 1.0f;
            }
            if (!saveContent.Contains("\"masterVolume\""))
            {
                currentSettings.masterVolume = 1.0f;
            }

            Debug.Log("Ustawienia wczytane pomyslnie.");
        }
        else
        {
            Debug.Log("Brak pliku ustawien. Tworzenie domyslnego settings.json...");
            Save();
        }
    }
}

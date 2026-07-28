using UnityEngine;
using System.IO;

public static class SettingsSystem
{
    // Struktura przechowuj¹ca wszystkie ustawienia
    [System.Serializable]
    public struct SettingsData
    {
        public float mouseSensitivity;
        public float masterVolume;
        public bool isFullscreen;
        // Tutaj w przysz³oœci dodasz np. rozdzielczoœæ, jakoœæ cieni itp.
    }

    // Aktualne ustawienia za³adowane w grze. 
    // Nadajemy od razu wartoœci domyœlne na wypadek pierwszego uruchomienia gry.
    public static SettingsData currentSettings = new SettingsData
    {
        mouseSensitivity = 2.0f,
        masterVolume = 1.0f, // 1.0 = 100%
        isFullscreen = true
    };

    // Œcie¿ka do pliku (zauwa¿ zmianê nazwy na settings.json)
    private static string GetFilePath()
    {
        return Application.persistentDataPath + "/settings.json";
    }

    public static void Save()
    {
        // Konwertujemy strukturê na ³adny (czytelny) format JSON
        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(GetFilePath(), json);
        Debug.Log("Ustawienia zapisane do: " + GetFilePath());
    }

    public static void Load()
    {
        string path = GetFilePath();

        // Sprawdzamy czy plik w ogóle istnieje (gracz móg³ go usun¹æ albo odpala grê 1. raz)
        if (File.Exists(path))
        {
            string saveContent = File.ReadAllText(path);
            currentSettings = JsonUtility.FromJson<SettingsData>(saveContent);
            Debug.Log("Ustawienia wczytane pomyœlnie.");
        }
        else
        {
            Debug.Log("Brak pliku ustawieñ. Tworzenie domyœlnego settings.json...");
            // Zapisujemy domyœlne wartoœci ustawione na górze skryptu
            Save();
        }
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

public class DebugController : MonoBehaviour
{
    bool showConsole;
    bool focusConsole;

    string input;

    public static DebugCommand Kill_All;
    public static DebugCommand Kill_Player;
    public static DebugCommand God;

    public List<object> commandList;

    void Update()
{
    if (Input.GetKeyDown(KeyCode.BackQuote))
    {
        showConsole = !showConsole;

        if (showConsole)
        {
            // Otwieranie konsoli
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            focusConsole = true;
            input = "";
        }
        else
        {
            // Zamykanie konsoli przy użyciu tyldy
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GUI.FocusControl(null); // Resetujemy focus
        }
    }
}

private void OnGUI()
{
    if (!showConsole) return;

    Event e = Event.current;

    // 1. Przechwytujemy Enter do zatwierdzenia komendy
    if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
    {
        HandleInput();
        
        showConsole = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        input = "";
        
        // BARDZO WAŻNE: Odpinamy focus od znikającego pola tekstowego!
        GUI.FocusControl(null); 
        
        e.Use();
        return; 
    }

    // 2. Przechwytujemy Tyldę, żeby znak "`" nie wpisał się do konsoli przy jej otwieraniu
    if (e.type == EventType.KeyDown && e.keyCode == KeyCode.BackQuote)
    {
        e.Use();
    }

    float y = 0f;
    GUI.Box(new Rect(0, y, Screen.width, 30), "");
    GUI.backgroundColor = new Color(0, 0, 0, 0);

    GUI.SetNextControlName("ConsoleInput");
    input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

    if (focusConsole)
    {
        GUI.FocusControl("ConsoleInput");
        focusConsole = false;
    }
}

    void Awake()
    {
        // ... (Twój obecny kod Awake zostaje bez zmian)
        Kill_All = new DebugCommand("kill_all", "Kills all enemies in the scene", "kill_all", () => {
            Debug.Log("Killing all enemies");
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    damagable.Damaged(9999);
                }
            }
        });
        Kill_Player = new DebugCommand("kill_player", "Kills the player", "kill_player", () => {
            Debug.Log("Killing player");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.TryGetComponent<PlayerStats>(out PlayerStats playerStats);
                if (playerStats != null)
                {
                    playerStats.Damaged(9999);
                }
            }
        });
        God = new DebugCommand("god", "Toggles god mode for the player", "god", () => {
            Debug.Log("Toggling god mode");
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.TryGetComponent<PlayerStats>(out PlayerStats playerStats);
                if (playerStats != null)
                {
                    playerStats.godMode = !playerStats.godMode;
                    Debug.Log("God mode is now " + (playerStats.godMode ? "ON" : "OFF"));
                }
            }
        });
        commandList = new List<object>()
        {
            Kill_All,
            Kill_Player,
            God,
        };
    }

    

    private void HandleInput()
    {
        for(int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if(input.Contains(commandBase.commandId))
            {
                (commandList[i] as DebugCommand).Invoke();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugController : MonoBehaviour
{
    public static DebugController Instance { get; private set; }

    public enum LogType
    {
        Input,
        Success,
        Error,
        Info
    }

    public struct ConsoleLogEntry
    {
        public string text;
        public LogType type;

        public ConsoleLogEntry(string text, LogType type)
        {
            this.text = text;
            this.type = type;
        }
    }

    [Header("Ustawienia Konsoli")]
    public KeyCode toggleKey = KeyCode.BackQuote;
    public float consoleHeight = 320f;
    public int maxHistoryCount = 50;
    public int maxLogCount = 100;

    [Header("Opcjonalne prefaby wrogów (Debug Spawner)")]
    public GameObject drunkPrefab;
    public GameObject fatPrefab;
    public GameObject gamblerPrefab;
    public GameObject idiotPrefab;

    public bool showConsole { get; private set; }
    private bool focusConsole;
    private string input = "";
    private bool wasPlayerLockedBeforeOpen = false;

    // Historia komend (strzałki w górę / w dół)
    private List<string> commandHistory = new List<string>();
    private int historyIndex = -1;

    // Logi i odpowiedzi konsoli
    private List<ConsoleLogEntry> logEntries = new List<ConsoleLogEntry>();
    private Vector2 scrollPosition;

    // Baza komend
    public List<DebugCommandBase> commandList = new List<DebugCommandBase>();

    // Dostęp do komend dla kompatybilności wstecznej
    public static DebugCommand Kill_All;
    public static DebugCommand Kill_Player;
    public static DebugCommand God;
    public static DebugCommand Noclip;

    private GUIStyle headerStyle;
    private GUIStyle logStyle;
    private GUIStyle suggestionStyle;
    private GUIStyle inputStyle;
    private Texture2D darkBgTex;
    private Texture2D inputBgTex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        InitDefaultCommands();
    }

    private void InitDefaultCommands()
    {
        commandList = new List<DebugCommandBase>();

        // 1. HELP
        commandList.Add(new DebugCommand("help", "Wyświetla listę wszystkich dostępnych komend", "help", () =>
        {
            LogEntry("=== DOSTĘPNE KOMENDY DEBUG ===", LogType.Info);
            foreach (var cmd in commandList)
            {
                LogEntry($"{cmd.commandFormat.PadRight(22)} - {cmd.commandDescription}", LogType.Info);
            }
        }));

        // 2. CLEAR
        commandList.Add(new DebugCommand("clear", "Czyści historię wpisów w konsoli", "clear", () =>
        {
            logEntries.Clear();
        }));

        // 3. NOCLIP
        Noclip = new DebugCommand("noclip", "Przełącza tryb noclip (latanie przez ściany bez kolizji)", "noclip", () =>
        {
            SourceMovement playerMovement = GetPlayerMovement();
            if (playerMovement != null)
            {
                bool state = playerMovement.ToggleNoclip();
                LogEntry($"Noclip jest teraz: {(state ? "WŁĄCZONY (WASD + Spacja/Ctrl by latać, Shift = przyspieszenie)" : "WYŁĄCZONY")}", LogType.Success);
            }
            else
            {
                LogEntry("Nie znaleziono komponentu SourceMovement na graczu!", LogType.Error);
            }
        });
        commandList.Add(Noclip);

        // 4. CHANGE LEVEL / MAP
        var changeLevelCmd = new DebugCommandArgs("changelevel", "Zmienia poziom/scenę (np. changelevel DemoLevel1 lub 'changelevel' - lista)", "changelevel <nazwa>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteChangeLevel(query);
        });
        commandList.Add(changeLevelCmd);

        commandList.Add(new DebugCommandArgs("change_level", "Alias dla changelevel", "change_level <nazwa>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteChangeLevel(query);
        }));

        commandList.Add(new DebugCommandArgs("map", "Alias dla changelevel (np. map DemoLevel1)", "map <nazwa>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteChangeLevel(query);
        }));

        commandList.Add(new DebugCommandArgs("level", "Alias dla changelevel", "level <nazwa>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteChangeLevel(query);
        }));

        // 5. SPAWN ENEMY
        var spawnEnemyCmd = new DebugCommandArgs("spawnenemy", "Spawnuje wroga przed graczem (drunk, fat, gambler, idiot, all)", "spawnenemy <typ>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteSpawnEnemy(query);
        });
        commandList.Add(spawnEnemyCmd);

        commandList.Add(new DebugCommandArgs("spawn_enemy", "Alias dla spawnenemy", "spawn_enemy <typ>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteSpawnEnemy(query);
        }));

        commandList.Add(new DebugCommandArgs("spawn", "Alias dla spawnenemy (np. spawn drunk)", "spawn <typ>", (args) =>
        {
            string query = (args != null && args.Length > 0) ? args[0] : "";
            ExecuteSpawnEnemy(query);
        }));

        // 6. GOD MODE
        God = new DebugCommand("god", "Przełącza nieśmiertelność gracza (God Mode)", "god", () =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.godMode = !stats.godMode;
                LogEntry($"God Mode jest teraz: {(stats.godMode ? "WŁĄCZONY" : "WYŁĄCZONY")}", LogType.Success);
            }
            else
            {
                LogEntry("Nie znaleziono komponentu PlayerStats na scenie!", LogType.Error);
            }
        });
        commandList.Add(God);

        // 7. KILL ALL
        Kill_All = new DebugCommand("kill_all", "Zabija wszystkich wrogów na scenie", "kill_all", () =>
        {
            int killed = 0;

            var cores = FindObjectsByType<EnemyCore>(FindObjectsSortMode.None);
            foreach (var core in cores)
            {
                if (!core.dead)
                {
                    core.SetDead(true);
                    killed++;
                }
            }

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy.TryGetComponent<IDamagable>(out var damagable))
                {
                    damagable.Damaged(9999);
                    killed++;
                }
            }

            LogEntry($"Wyeliminowano {killed} wrogów na scenie.", LogType.Success);
        });
        commandList.Add(Kill_All);

        // 8. KILL PLAYER
        Kill_Player = new DebugCommand("kill_player", "Zadaje śmiertelne obrażenia graczowi", "kill_player", () =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.Damaged(9999);
                LogEntry("Gracz został zlikwidowany.", LogType.Success);
            }
        });
        commandList.Add(Kill_Player);

        // 9. ŚWIĘTOŚĆ (z parametrem)
        commandList.Add(new DebugCommand<float>("swietosc", "Dodaje lub odejmuje Świętość gracza (np. swietosc 50)", "swietosc <wartość>", (amount) =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.AddSwietosc(amount);
                LogEntry($"Zaktualizowano Świętość o {amount:+0;-0;0}. Aktualny stan: {stats.swietosc:F0}", LogType.Success);
            }
        }));

        // 10. HEAL (z parametrem)
        commandList.Add(new DebugCommand<float>("heal", "Leczy gracza o zadaną ilość HP", "heal <wartość>", (amount) =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.playerHp = Mathf.Min(stats.playerHp + amount, stats.maxPlayerHp);
                LogEntry($"Uleczono gracza o {amount}. Aktualne HP: {stats.playerHp}/{stats.maxPlayerHp}", LogType.Success);
            }
        }));

        // 11. HP (ustawienie konkretnego życia)
        commandList.Add(new DebugCommand<float>("hp", "Ustawia dokładną ilość punktów życia gracza", "hp <wartość>", (val) =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.playerHp = Mathf.Clamp(val, 1, stats.maxPlayerHp);
                LogEntry($"Ustawiono HP gracza na: {stats.playerHp}/{stats.maxPlayerHp}", LogType.Success);
            }
        }));

        // 12. DYNAMITY / RZUCANE
        commandList.Add(new DebugCommand<int>("dynamite", "Dodaje wskazaną liczbę dynamitów/rzucanych", "dynamite <ilość>", (count) =>
        {
            PlayerStats stats = GetPlayerStats();
            if (stats != null)
            {
                stats.throwablesCount = Mathf.Max(0, stats.throwablesCount + count);
                if (GameManager.Instance != null && GameManager.Instance.UiMenager != null)
                {
                    GameManager.Instance.UiMenager.UpdateThrowableCount(stats.throwablesCount);
                }
                LogEntry($"Liczba dynamitów: {stats.throwablesCount}", LogType.Success);
            }
        }));

        // 13. TIMESCALE
        commandList.Add(new DebugCommand<float>("timescale", "Zmienia prędkość upływu czasu (np. 0.2 = slow-mo, 1 = normalnie)", "timescale <wartość>", (scale) =>
        {
            Time.timeScale = Mathf.Max(0f, scale);
            LogEntry($"Ustawiono Time.timeScale na {Time.timeScale}", LogType.Success);
        }));

        // 14. TELEPORTACJA
        commandList.Add(new DebugCommand<float, float, float>("tp", "Teleportuje gracza na koordynaty X Y Z", "tp <x> <y> <z>", (x, y, z) =>
        {
            Transform playerT = GetPlayerTransform();
            if (playerT != null)
            {
                playerT.position = new Vector3(x, y, z);
                LogEntry($"Teleportowano gracza na pozycję: ({x}, {y}, {z})", LogType.Success);
            }
        }));

        // 15. CLOSE
        commandList.Add(new DebugCommand("close", "Zamyka konsolę debugowania", "close", () =>
        {
            CloseConsole();
        }));
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (showConsole)
            {
                CloseConsole();
            }
            else
            {
                OpenConsole();
            }
        }
        else if (showConsole && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConsole();
        }
    }

    public void OpenConsole()
    {
        showConsole = true;
        focusConsole = true;
        historyIndex = -1;
        input = "";

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (GameManager.Instance != null)
        {
            wasPlayerLockedBeforeOpen = GameManager.Instance.State != PlayerState.Normal;
            if (!wasPlayerLockedBeforeOpen)
            {
                GameManager.Instance.PlayerStatus(PlayerState.Locked);
            }
        }
    }

    public void CloseConsole()
    {
        showConsole = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (GameManager.Instance != null && !wasPlayerLockedBeforeOpen && !GameManager.Instance.isShopping)
        {
            GameManager.Instance.PlayerStatus(PlayerState.Normal);
        }

        GUI.FocusControl(null);
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        InitStylesIfNeeded();

        Event e = Event.current;

        // Przechwycenie Tyldy, by znak ` nie wpisywał się do inputu
        if (e.type == EventType.KeyDown && e.keyCode == toggleKey)
        {
            e.Use();
            return;
        }

        // Pobierz pasujące podpowiedzi na podstawie pierwszego członu wpisanego tekstu
        List<DebugCommandBase> suggestions = GetMatchingSuggestions(input);

        // Obsługa klawiatury przed rysowaniem
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (!string.IsNullOrWhiteSpace(input))
                {
                    HandleInput();
                }
                e.Use();
                return;
            }
            else if (e.keyCode == KeyCode.UpArrow)
            {
                NavigateHistory(-1);
                e.Use();
                return;
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                NavigateHistory(1);
                e.Use();
                return;
            }
            else if (e.keyCode == KeyCode.Tab)
            {
                if (suggestions.Count > 0)
                {
                    input = suggestions[0].commandId + " ";
                    MoveCursorToEnd();
                }
                e.Use();
                return;
            }
        }

        // TŁO KONSOLI (Styl Quake / Half-Life na górze ekranu)
        float totalW = Screen.width;
        GUI.DrawTexture(new Rect(0, 0, totalW, consoleHeight), darkBgTex);

        // BELKA TYTUŁOWA
        GUI.Label(new Rect(10, 4, totalW - 20, 20), "=== DOLINA DEBUG CONSOLE ===   [Tylda / ESC] Zamknij  |  [TAB] Uzupełnij  |  [↑/↓] Historia  |  Wpisz 'help'", headerStyle);

        // OKNO HISTORII LOGÓW
        float logAreaY = 26f;
        float logAreaH = consoleHeight - 65f;
        Rect logViewRect = new Rect(10, logAreaY, totalW - 20, logAreaH);

        GUILayout.BeginArea(logViewRect);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        for (int i = 0; i < logEntries.Count; i++)
        {
            var entry = logEntries[i];
            logStyle.normal.textColor = GetColorForLogType(entry.type);
            GUILayout.Label(entry.text, logStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // POLE WPISYWANIA KOMENDY
        float inputY = consoleHeight - 34f;
        GUI.DrawTexture(new Rect(0, inputY - 3f, totalW, 35f), inputBgTex);

        GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        promptStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(10, inputY, 20, 24), ">", promptStyle);

        GUI.SetNextControlName("ConsoleInput");
        input = GUI.TextField(new Rect(28, inputY, totalW - 40, 24), input, inputStyle);

        if (focusConsole)
        {
            GUI.FocusControl("ConsoleInput");
            focusConsole = false;
        }

        // LISTA PODPOWIEDZI (WYŚWIETLANA POD KONSOLĄ, GDY GRACZ COŚ WPISUJE)
        if (!string.IsNullOrWhiteSpace(input) && suggestions.Count > 0)
        {
            float sugY = consoleHeight + 4f;
            float sugW = Mathf.Min(560f, totalW - 20f);
            float itemH = 22f;
            float sugH = (suggestions.Count * itemH) + 8f;

            GUI.DrawTexture(new Rect(10, sugY, sugW, sugH), darkBgTex);

            for (int i = 0; i < suggestions.Count && i < 6; i++)
            {
                var cmd = suggestions[i];
                Rect itemRect = new Rect(14, sugY + 4f + (i * itemH), sugW - 8f, itemH);

                string hintText = $"[TAB] {cmd.commandFormat.PadRight(18)} - {cmd.commandDescription}";
                suggestionStyle.normal.textColor = (i == 0) ? Color.yellow : Color.white;

                if (GUI.Button(itemRect, hintText, suggestionStyle))
                {
                    input = cmd.commandId + " ";
                    GUI.FocusControl("ConsoleInput");
                    MoveCursorToEnd();
                }
            }
        }
    }

    private void HandleInput()
    {
        string trimmed = input.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        // Dodaj do historii (nie powielaj z rzędu)
        if (commandHistory.Count == 0 || commandHistory[commandHistory.Count - 1] != trimmed)
        {
            commandHistory.Add(trimmed);
            if (commandHistory.Count > maxHistoryCount)
            {
                commandHistory.RemoveAt(0);
            }
        }
        historyIndex = -1;

        LogEntry($"> {trimmed}", LogType.Input);

        string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string cmdName = parts[0].ToLowerInvariant();
        string[] args = parts.Length > 1 ? new string[parts.Length - 1] : new string[0];
        if (parts.Length > 1)
        {
            Array.Copy(parts, 1, args, 0, args.Length);
        }

        // Wsparcie komend wielowyrazowych: "change level ..." -> "changelevel ..."
        if (cmdName == "change" && args.Length > 0 && args[0].Equals("level", StringComparison.OrdinalIgnoreCase))
        {
            cmdName = "changelevel";
            string[] newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            args = newArgs;
        }
        // Wsparcie "spawn enemy ..." -> "spawnenemy ..."
        else if (cmdName == "spawn" && args.Length > 0 && args[0].Equals("enemy", StringComparison.OrdinalIgnoreCase))
        {
            cmdName = "spawnenemy";
            string[] newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            args = newArgs;
        }

        DebugCommandBase cmd = commandList.Find(c => c.commandId.Equals(cmdName, StringComparison.OrdinalIgnoreCase));
        if (cmd != null)
        {
            bool success = cmd.Execute(args, out string message);
            if (!string.IsNullOrEmpty(message))
            {
                LogEntry(message, success ? LogType.Success : LogType.Error);
            }
        }
        else
        {
            LogEntry($"[BŁĄD] Nieznana komenda '{cmdName}'. Wpisz 'help' aby wyświetlić listę komend.", LogType.Error);
        }

        input = "";
        scrollPosition.y = float.MaxValue;
    }

    private void NavigateHistory(int direction)
    {
        if (commandHistory.Count == 0) return;

        if (direction < 0) // Strzałka w górę - starsze
        {
            if (historyIndex == -1)
            {
                historyIndex = commandHistory.Count - 1;
            }
            else if (historyIndex > 0)
            {
                historyIndex--;
            }
            input = commandHistory[historyIndex];
            MoveCursorToEnd();
        }
        else // Strzałka w dół - nowsze
        {
            if (historyIndex != -1)
            {
                if (historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    input = commandHistory[historyIndex];
                }
                else
                {
                    historyIndex = -1;
                    input = "";
                }
                MoveCursorToEnd();
            }
        }
    }

    private List<DebugCommandBase> GetMatchingSuggestions(string currentInput)
    {
        List<DebugCommandBase> list = new List<DebugCommandBase>();
        if (string.IsNullOrWhiteSpace(currentInput)) return list;

        string prefix = currentInput.TrimStart().Split(' ')[0].ToLowerInvariant();

        foreach (var cmd in commandList)
        {
            if (cmd.commandId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(cmd);
            }
        }

        return list;
    }

    public void LogEntry(string message, LogType type)
    {
        logEntries.Add(new ConsoleLogEntry(message, type));
        if (logEntries.Count > maxLogCount)
        {
            logEntries.RemoveAt(0);
        }
        scrollPosition.y = float.MaxValue;
    }

    private void MoveCursorToEnd()
    {
        TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        if (editor != null)
        {
            editor.text = input;
            editor.cursorIndex = input.Length;
            editor.selectIndex = input.Length;
        }
    }

    private Color GetColorForLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Input: return new Color(0.95f, 0.85f, 0.2f); // Żółty
            case LogType.Success: return new Color(0.35f, 1f, 0.45f); // Zielony
            case LogType.Error: return new Color(1f, 0.35f, 0.35f); // Czerwony
            case LogType.Info:
            default: return new Color(0.85f, 0.85f, 0.85f); // Jasnoszary
        }
    }

    // --- IMPLEMENTACJA: CHANGE LEVEL ---
    private void ExecuteChangeLevel(string query)
    {
        List<string> availableScenes = GetAvailableScenes();

        if (string.IsNullOrWhiteSpace(query) || query.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            LogEntry("=== DOSTĘPNE POZIOMY DO ZAŁADOWANIA ===", LogType.Info);
            foreach (var sc in availableScenes)
            {
                LogEntry($" - {sc}", LogType.Info);
            }
            LogEntry("Wpisz: changelevel <nazwa_poziomu>", LogType.Info);
            return;
        }

        string targetScene = null;

        // Jeśli podano liczbę jako indeks w build settings
        if (int.TryParse(query, out int idx))
        {
            if (idx >= 0 && idx < SceneManager.sceneCountInBuildSettings)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(idx);
                targetScene = System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }

        // Dopasowanie dokładne / częściowe
        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = availableScenes.Find(s => s.Equals(query, StringComparison.OrdinalIgnoreCase));
        }
        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = availableScenes.Find(s => s.StartsWith(query, StringComparison.OrdinalIgnoreCase));
        }
        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = availableScenes.Find(s => s.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (!string.IsNullOrEmpty(targetScene))
        {
            LogEntry($"[OK] Ładowanie poziomu '{targetScene}'...", LogType.Success);
            Time.timeScale = 1f;
            CloseConsole();
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            LogEntry($"[BŁĄD] Nie znaleziono poziomu pasującego do '{query}'. Wpisz 'changelevel' bez parametrów, aby zobaczyć listę.", LogType.Error);
        }
    }

    private List<string> GetAvailableScenes()
    {
        List<string> scenes = new List<string>();

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(name) && !scenes.Contains(name))
            {
                scenes.Add(name);
            }
        }

        string[] defaultKnown = new string[]
        {
            "MainMenu", "DemoLevel1", "TestingLevel", "TestingLevelLukasz",
            "Butelki", "Kolejka", "Demo", "Stylistyka", "Level1",
            "V2LukaszTestingLevel", "LukaszV3TestingLevel", "Mati testing", "LightTesting"
        };

        foreach (var sc in defaultKnown)
        {
            if (!scenes.Contains(sc))
            {
                scenes.Add(sc);
            }
        }

        return scenes;
    }

    // --- IMPLEMENTACJA: SPAWN ENEMY ---
    private void ExecuteSpawnEnemy(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            LogEntry("=== DOSTĘPNE TYPY WROGÓW ===", LogType.Info);
            LogEntry(" - drunk    (DrDrunk - rewolwerowiec)", LogType.Info);
            LogEntry(" - fat      (FATFinal - ciężki rewolwerowiec/boss)", LogType.Info);
            LogEntry(" - gambler  (GamblerFinalFinal - zwinny przeciwnik)", LogType.Info);
            LogEntry(" - idiot    (RealEnemyIdiot - przeciwnik podstawowy/wręcz)", LogType.Info);
            LogEntry(" - all      (spawnuje po 1 z każdego typu w łuku przed graczem)", LogType.Info);
            LogEntry("Użycie: spawnenemy <typ>", LogType.Info);
            return;
        }

        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null)
        {
            LogEntry("[BŁĄD] Nie znaleziono gracza na scenie!", LogType.Error);
            return;
        }

        string q = query.ToLowerInvariant();

        if (q == "all")
        {
            string[] allTypes = new string[] { "drunk", "fat", "gambler", "idiot" };
            float[] offsets = new float[] { -2.5f, -0.8f, 0.8f, 2.5f };

            for (int i = 0; i < allTypes.Length; i++)
            {
                Vector3 offsetPos = playerTransform.position + playerTransform.forward * 4f + playerTransform.right * offsets[i];
                SpawnSingleEnemy(allTypes[i], offsetPos, playerTransform);
            }
            LogEntry("[OK] Zespawnowano wszystkich 4 wrogów przed graczem!", LogType.Success);
            return;
        }

        string resolvedType = ResolveEnemyType(q);
        if (resolvedType == null)
        {
            LogEntry($"[BŁĄD] Nieznany typ wroga '{query}'. Dostępne: drunk, fat, gambler, idiot, all", LogType.Error);
            return;
        }

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * 3.5f;
        GameObject spawned = SpawnSingleEnemy(resolvedType, spawnPos, playerTransform);
        if (spawned != null)
        {
            LogEntry($"[OK] Zespawnowano wroga '{resolvedType}' przed graczem.", LogType.Success);
        }
    }

    private string ResolveEnemyType(string input)
    {
        switch (input.ToLowerInvariant())
        {
            case "drunk":
            case "drdrunk":
            case "pijak":
                return "drunk";

            case "fat":
            case "fatfinal":
            case "heavy":
            case "boss":
            case "gruby":
                return "fat";

            case "gambler":
            case "gamblerfinal":
            case "hazardzista":
                return "gambler";

            case "idiot":
            case "realenemy":
            case "melee":
            case "basic":
                return "idiot";

            default:
                return null;
        }
    }

    private GameObject SpawnSingleEnemy(string type, Vector3 targetPos, Transform playerTransform)
    {
        GameObject prefab = null;

        // 1. Sprawdź referencje z inspektora
        switch (type)
        {
            case "drunk": prefab = drunkPrefab; break;
            case "fat": prefab = fatPrefab; break;
            case "gambler": prefab = gamblerPrefab; break;
            case "idiot": prefab = idiotPrefab; break;
        }

        // 2. Sprawdź folder Resources/Enemies
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Enemies/" + type);
        }

        // 3. Sprawdź AssetDatabase w edytorze jako ostateczny fallback
        #if UNITY_EDITOR
        if (prefab == null)
        {
            string path = "";
            switch (type)
            {
                case "drunk": path = "Assets/Prefabs/Npc/Enemy/DrDrunk.prefab"; break;
                case "fat": path = "Assets/Prefabs/Npc/Enemy/FATFinal.prefab"; break;
                case "gambler": path = "Assets/Prefabs/Npc/Enemy/GamblerFinalFinal.prefab"; break;
                case "idiot": path = "Assets/Prefabs/RealEnemyIdiot.prefab"; break;
            }
            if (!string.IsNullOrEmpty(path))
            {
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        #endif

        if (prefab == null)
        {
            LogEntry($"[BŁĄD] Nie znaleziono prefabu dla wroga '{type}'!", LogType.Error);
            return null;
        }

        // Dopasowanie do wysokości podłoża
        Vector3 finalPos = targetPos;
        if (Physics.Raycast(targetPos + Vector3.up * 2.5f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Default", "Ground")))
        {
            finalPos.y = hit.point.y;
        }

        // Obrót twarzą do gracza
        Vector3 lookDir = playerTransform.position - finalPos;
        lookDir.y = 0;
        Quaternion rot = (lookDir.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(lookDir) : Quaternion.identity;

        GameObject instance = Instantiate(prefab, finalPos, rot);
        return instance;
    }

    // --- POMOCNIKI REFERENCJI ---
    private PlayerStats GetPlayerStats()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
            return GameManager.Instance.PlayerStats;

        return FindFirstObjectByType<PlayerStats>();
    }

    private SourceMovement GetPlayerMovement()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerRef != null)
            return GameManager.Instance.PlayerRef;

        return FindFirstObjectByType<SourceMovement>();
    }

    private Transform GetPlayerTransform()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerRef != null)
            return GameManager.Instance.PlayerRef.transform;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private void InitStylesIfNeeded()
    {
        if (darkBgTex == null)
        {
            darkBgTex = new Texture2D(1, 1);
            darkBgTex.SetPixel(0, 0, new Color(0.06f, 0.07f, 0.10f, 0.93f));
            darkBgTex.Apply();
        }

        if (inputBgTex == null)
        {
            inputBgTex = new Texture2D(1, 1);
            inputBgTex.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.05f, 0.98f));
            inputBgTex.Apply();
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            headerStyle.normal.textColor = new Color(0.6f, 0.65f, 0.75f);
        }

        if (logStyle == null)
        {
            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true
            };
        }

        if (inputStyle == null)
        {
            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            inputStyle.normal.textColor = Color.white;
        }

        if (suggestionStyle == null)
        {
            suggestionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            suggestionStyle.hover.textColor = Color.cyan;
        }
    }
}
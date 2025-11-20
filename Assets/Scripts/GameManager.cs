using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerState
{
    Normal,
    Locked,
    Butelki,
    Kolejka
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Publiczne w³aœciwoœci, ale z prywatnym "set"
    // Inne skrypty mog¹ je odczytaæ, ale tylko GameManager mo¿e je ustawiæ.
    [SerializeField] public PlayerStats PlayerStats { get; private set; }
    [SerializeField] public PlayerMovement PlayerRef { get; private set; }
    [SerializeField] public CameraControll PlayerCam { get; private set; }
    [SerializeField] public UiMenager UiMenager { get; private set; }
    [SerializeField] public WeaponSwap Weapons { get; private set; }
    [SerializeField] public GameObject WeaponParrent { get; private set; }
    [SerializeField] public List<GunSystem> Guns { get; private set; } = new List<GunSystem>();


    // ... inne zmienne jak State, isGamePaused ...
    public PlayerState State;
    public bool isGamePaused = false;

    private void Awake()
    {
        // TYLKO logika singletona
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // --- NOWE METODY REJESTRACJI ---

    public void RegisterPlayer(PlayerMovement playerMovement, PlayerStats stats, CameraControll cam)
    {
        PlayerRef = playerMovement;
        PlayerStats = stats;
        PlayerCam = cam;
        Debug.Log("Player registered to GameManager");
    }

    public void RegisterUi(UiMenager ui)
    {
        UiMenager = ui;
        Debug.Log("UI registered to GameManager");
    }

    public void RegisterWeapons(GameObject weaponParrent, WeaponSwap weaponSwap)
    {
        WeaponParrent = weaponParrent;
        Weapons = weaponSwap;

        // Logika Ładowania broni teraz jest tutaj
        Guns.Clear();
        foreach (Transform gun in WeaponParrent.transform)
        {
            if (gun.TryGetComponent(out GunSystem gunSystem))
            {
                Guns.Add(gunSystem);
            }
        }
        Debug.Log($"Weapons registered. Found {Guns.Count} guns.");
    }

    // Metoda do wyczyszczenia referencji przy ³adowaniu nowej sceny
    // (na razie nie jest konieczna, ale to dobra praktyka)

    private void Update()
    {
        if (Input.GetButtonDown("pauseGame") && PlayerStats.isDead == false)
        {
            if (!isGamePaused)
            {
                PauseGame();
            }
            else
            {
                // ResumeGame();
            }
        }
        Death();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ten kod uruchomi się po za³adowaniu KA¯DEJ nowej sceny
        Time.timeScale = 1; // Upewnij siê, ¿e czas jest odblokowany
        if (scene.name == "MainMenu")
        {
            return; // Nie inicjuj gracza w menu g³ównym
        }
        // PlayerStatus(PlayerState.Normal);// Zresetuj stan gracza
        isGamePaused = false; // Zresetuj pauzê
    }
    public void OnSceneUnload()
    {
        PlayerStats = null;
        PlayerRef = null;
        PlayerCam = null;
        UiMenager = null;
        Weapons = null;
    }



    /// <summary>
    /// Used to pause the game and show the pause menu
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("Pausing Game");
        Time.timeScale = 0;
        isGamePaused = true;
        PlayerStatus(PlayerState.Locked);
        UiMenager.PauseGame();

    }

    /// <summary>
    /// Used to pause the game and allows to choose whether to show the PAUSE MENU or not
    /// </summary>
    /// <param name="ShowMenu"></param>
    public void PauseGame(bool ShowMenu)
    {
        PlayerStatus(PlayerState.Locked);
        Time.timeScale = 0;
        UiMenager.PauseGame(ShowMenu);
        isGamePaused = true;

    }
    public void ResumeGame()
    {

        Debug.Log("Resuming Game");
        Time.timeScale = 1;
        //uiMenager.ResumeGame();
        isGamePaused = false;
        PlayerStatus(PlayerState.Normal);

    }

    public void UpdateThrowablesCount()
    {
        UiMenager.UpdateThrowableCount(PlayerStats.throwablesCount);
    }


    public void Death()
    {
        if (PlayerStats.playerHp <= 0 && PlayerStats.isDead == false)
        {
            PlayerStats.isDead = true;
            
            Debug.Log("Player died");
            PlayerRef.movementLocked = true;
            PlayerCam.LockCamera(true);
            WeaponParrent.SetActive(false);
            UiMenager.DeathPanel();
        }
    }

    public void HorseMount(Horse horse)
    {
        PlayerRef.transform.position = horse.playerSlot.position;
        PlayerRef.transform.rotation = horse.playerSlot.rotation;
        PlayerRef.mounted = true;

    }
    public void SaveButton()
    {
        SaveSystem.Save();
        Debug.Log("Saved");
        UiMenager.SaveIcon();
    }
    public void LoadButton()
    {
        SaveSystem.Load();
    }

    public void PlayerStatus(PlayerState state)
    {
        //Debug.Log("Dupa");
        State = state;
        switch (State)
        {
            case PlayerState.Normal:

                PlayerRef.movementLocked = false;
                PlayerCam.LockCamera(false);
                if (WeaponParrent) WeaponParrent.SetActive(true);

                //foreach (var gun in guns)
                //{
                //    gun.enabled = true;
                //}

                return;
            case PlayerState.Locked:

                PlayerRef.movementLocked = true;
                PlayerCam.LockCamera(true);
                if (WeaponParrent) WeaponParrent.SetActive(false);

                //foreach (var gun in guns)
                //{
                //    gun.enabled = false;
                //}
                return;
            case PlayerState.Butelki:

                PlayerRef.movementLocked = true;
                PlayerCam.LockCamera(true);
                WeaponParrent.SetActive(false);
                return;


            default: return;
        }
    }


}

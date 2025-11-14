using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    Locked,
    Butelki,
    Kolejka
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return null;
            }
            if (_instance == null)
            {
                Instantiate(Resources.Load<GameManager>("GameManager"));
            }
#endif
            return _instance;
        }
    }
    public PlayerStats playerStats;
    public WeaponSwap weapons { get; set; }
    public List<GunSystem> guns;



    GameObject weaponParrent;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        weaponParrent = GameObject.Find("GunSlot");
        weapons = weaponParrent.GetComponent<WeaponSwap>();

        //cinemashineBrain = GameObject.FindWithTag("MainCamera").GetComponent<CinemachineBrain>();

        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.Find("CinemachineCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;
    }

    public PlayerState State;

    public PlayerMovement playerRef;
    CameraControll playerCam;
   
    public UiMenager uiMenager;
    GameObject gunSlot;
    [SerializeField] public static List<GameObject> cameras = new List<GameObject>();

    public bool isGamePaused = false;

    private void Start()
    {
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        weaponParrent = GameObject.Find("GunSlot");
        weapons = weaponParrent.GetComponent<WeaponSwap>();

        
       
        guns.Clear();

        foreach (Transform gun in weaponParrent.transform)
        {
            guns.Add(gun.GetComponent<GunSystem>());
        }
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.Find("CinemachineCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;


    }

    private void OnLevelWasLoaded(int level)
    {
        Debug.Log("Level Loaded: " + level);
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        weaponParrent = GameObject.Find("GunSlot");
        weapons = weaponParrent.GetComponent<WeaponSwap>();

        guns.Clear();

        foreach (Transform gun in weaponParrent.transform)
        {
            guns.Add(gun.GetComponent<GunSystem>());
        }
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.Find("CinemachineCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;


        //Camera manager
        cameras.AddRange(GameObject.FindGameObjectsWithTag("Camera"));

    }
    private void Update()
    {
        
        Death();
        //if (uiMenager.currentScene.name == "Butelki")
        //{
        //    PlayerStatus(PlayerState.Butelki);
        //}
        //else if (uiMenager.currentScene.name == "Kolejka")
        //{
        //    PlayerStatus(PlayerState.Kolejka);
        //}

        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            Debug.Log("Saved");
            SaveSystem.Save();
        }

        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Debug.Log("Load");
            SaveSystem.Load();
        }

        if (Input.GetButtonDown("pauseGame"))
        {
            if (isGamePaused)
            {
                //ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Used to pause the game and show the pause menu
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0;
        uiMenager.PauseGame();
        isGamePaused = true;
        PlayerStatus(PlayerState.Locked);

    }

    /// <summary>
    /// Used to pause the game and allows to choose whether to show the PAUSE MENU or not
    /// </summary>
    /// <param name="ShowMenu"></param>
    public void PauseGame(bool ShowMenu)
    {
        PlayerStatus(PlayerState.Locked);
        Time.timeScale = 0;
        uiMenager.PauseGame(ShowMenu);
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
        uiMenager.UpdateThrowableCount(playerStats.throwablesCount);
    }

    public static void ActivateCamera(GameObject activatedCamera)
    {
        foreach (GameObject cam in cameras)
        {
            cam.SetActive(cam == activatedCamera);
        }
    }
    public void Death()
    {
        if (playerStats.playerHp <= 0 && playerStats.isDead == false)
        {
            playerStats.isDead = true;
            Debug.Log("Player died");
            playerRef.movementLocked = true;
            playerCam.LockCamera(true);
            gunSlot.SetActive(false);
            uiMenager.DeathPanel();
        }
    }

    public void HorseMount(Horse horse)
    {   
        playerRef.transform.position = horse.playerSlot.position;
        playerRef.transform.rotation = horse.playerSlot.rotation;   
        playerRef.mounted = true;

    }
    public void SaveButton()
    {
        SaveSystem.Save();
        Debug.Log("Saved");
        uiMenager.SaveIcon();
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

                playerRef.movementLocked = false;
                playerCam.LockCamera(false);
                gunSlot.SetActive(true);

                //foreach (var gun in guns)
                //{
                //    gun.enabled = true;
                //}

                return;
            case PlayerState.Locked:

                playerRef.movementLocked = true;
                playerCam.LockCamera(true);
                gunSlot.SetActive(false);

                //foreach (var gun in guns)
                //{
                //    gun.enabled = false;
                //}
                return;
            case PlayerState.Butelki:

                playerRef.movementLocked = true;
                playerCam.LockCamera(true);
                gunSlot.SetActive(false);
                return;


            default: return;
        }
    }


}

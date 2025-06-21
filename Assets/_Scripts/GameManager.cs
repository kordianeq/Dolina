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
    public PlayerStats playerStats { get; set; }

    public bool isGunUnlocked;



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

    }

    private void OnLevelWasLoaded(int level)
    {
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();

        if (isGunUnlocked)
        {
            gunSlot.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public PlayerState State;

    PlayerMovement playerRef;
    public CameraControll playerCam;
    UiMenager uiMenager;
    public GameObject gunSlot;
    public GunSystem gun;

    private void Start()
    {
        isGunUnlocked = false;
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        weaponParrent = GameObject.Find("GunSlot");


        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;


    }

  
    //public void SetGun()
    //{
    //   gun = GameObject.Find("Revolver").GetComponent<GunSystem>();
    //}
    private void Update()
    {

        Death();

        if (uiMenager.isGamePaused)
        {
            if (gun)
            {
                gun.allowShooting = false;
            }

        }
        else
        {
            if (gun)
            {
                gun.allowShooting = true;
            }
        }
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
    public void SaveButton()
    {
        SaveSystem.Save();
        Debug.Log("Saved");
        uiMenager.SaveIcon();
    }
    public void LoadButton()
    {
        if (playerStats.isDead)
        {
            playerStats.isDead = false;
            playerRef.movementLocked = false;
            playerCam.LockCamera(false);
            gunSlot.SetActive(true);
            uiMenager.deathPanel.SetActive(false);
            Time.timeScale = 1;
        }
        SaveSystem.Load();

    }
    public void PlayerStatus(PlayerState state)
    {
        State = state;
        switch (State)
        {
            case PlayerState.Normal:

                playerRef.movementLocked = false;
                playerCam.LockCamera(false);
                gunSlot.SetActive(true);

                return;
            case PlayerState.Locked:

                playerRef.movementLocked = true;
                playerCam.LockCamera(true);
                gunSlot.SetActive(false);
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

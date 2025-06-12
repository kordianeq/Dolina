using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Playables;

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
    public PlayerStats playerStats { get;set; }
    public WeaponSwap weapons { get; set; }
    public List<GunSystem> guns;

    public List<EnemyCore> enemies;

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

    public PlayerState State;

    PlayerMovement playerRef;
    CameraControll playerCam;
    UiMenager uiMenager;
    GameObject gunSlot;

    private void Start()
    {
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        weaponParrent = GameObject.Find("GunSlot");
        weapons = weaponParrent.GetComponent<WeaponSwap>();

        foreach(Transform gun in weaponParrent.transform)
        {
           guns.Add(gun.GetComponent<GunSystem>());
        }
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;
        
        foreach(EnemyCore enemycore in GameObject.FindObjectsOfType<EnemyCore>())
        {
            //Debug.Log("Found enemy: " + enemycore.name);
            enemies.Add(enemycore);
        }

    }
    private void Update()
    {
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

        if(Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Debug.Log("Load");
            SaveSystem.Load();
        }
    }

   
    public void PlayerStatus( PlayerState state)
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

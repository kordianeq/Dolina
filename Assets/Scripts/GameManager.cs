using UnityEngine;
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
    //private static GameManager _instance;

    //public static GameManager Instance { get { return _instance; } }


    //private void Awake()
    //{
    //    if (_instance != null && _instance != this)
    //    {
    //        Destroy(this.gameObject);
    //    }
    //    else
    //    {
    //        _instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //}

    public PlayerState State;
    PlayerMovement playerRef;
    CameraControll playerCam;
    UiMenager uiMenager;
    GameObject gunSlot;

    private void Start()
    {
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        uiMenager = GameObject.Find("Canvas").GetComponent<UiMenager>();
        State = PlayerState.Normal;
    }
    private void Update()
    {
        if (uiMenager.currentScene.name == "Butelki")
        {
            PlayerStatus(PlayerState.Butelki);
        }
        else if (uiMenager.currentScene.name == "Kolejka")
        {
            PlayerStatus(PlayerState.Kolejka);
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

using UnityEngine;

public enum PlayerState
{
    Normal,
    Locked
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
    GunSystem[] gunSystem;
    GameObject gunSlot;

    private void Start()
    {
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
        gunSlot = GameObject.Find("GunSlot");
        State = PlayerState.Normal;
    }
    private void Update()
    {
        
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
            default: return;
        }
    }

  
}

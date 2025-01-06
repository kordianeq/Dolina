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

    private void Start()
    {
        playerRef = GameObject.Find("Player").GetComponent<PlayerMovement>();
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<CameraControll>();
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
                return;
            case PlayerState.Locked:
                
                playerRef.movementLocked = true;   
                playerCam.LockCamera(true);
                return;
            default: return;
        }
    }

  
}

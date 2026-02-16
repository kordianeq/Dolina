using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.XInput;


public class CameraControll : MonoBehaviour
{
    [Header("Referencje")]
    //public Transform orientation;
    [SerializeField] public Animator fpsAnim;
    [SerializeField] public CinemachineInputAxisController cinemachineInput;

    [Header("Ustawienia Feelingu (Tilt)")]
    public float tiltAmount = 3f; // Jak mocno kamera przechyla siê na boki (zalecane: 2-5)
    public float tiltSpeed = 10f; // Jak szybko wraca do poziomu

    // Zmienne wewnêtrzne
    float xRotation;
    float yRotation;
    float tiltRotation; // Nowa zmienna dla osi Z
    bool lockMode;

    interactiveSlider sensitivitySlider;
    void Start()
    {

        LockCamera(false);
        sensitivitySlider =  GameManager.Instance.UiMenager.sensitivitySlider;
        //sensitivitySlider.value = sensX;

    }

    private void Awake()
    {
        cinemachineInput = GetComponent<CinemachineInputAxisController>();
    }


    void Update()
    {
        
        if (lockMode) return; // Jeœli zablokowana, nie wykonuj reszty kodu

        //CalculateCameraRotation();
    }

    //void CalculateCameraRotation()
    //{
     
        
    //    float inputX = Input.GetAxisRaw("Horizontal");
    //    float targetTilt = -inputX * tiltAmount;

    //    // P³ynne przejœcie (Lerp) do docelowego k¹ta przechy³u
    //    tiltRotation = Mathf.Lerp(tiltRotation, targetTilt, Time.deltaTime * tiltSpeed);

    //    // 4. Aplikowanie Rotacji
    //    // Kamera (transform) dostaje X (góra-dó³), Y (lewo-prawo) i Z (przechy³)
    //    transform.rotation = Quaternion.Euler(xRotation, yRotation, tiltRotation);

    //    // Cia³o gracza (orientation) obraca siê tylko w osi Y (lewo-prawo)
    //    if (orientation != null)
    //    {
    //        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    //    }
    //}

    public void AdjustCameraSensitivity(float newSensitivity)
    {
        if (cinemachineInput != null)
        {
            
            for (int i = 0; i < cinemachineInput.Controllers.Count; i++)
            {
                var axisController = cinemachineInput.Controllers[i];

                float sign = Mathf.Sign(axisController.Input.Gain);
                axisController.Input.Gain = newSensitivity * sign;

                cinemachineInput.Controllers[i] = axisController;
            }
            Debug.Log($"Camera sensitivity adjusted to: {newSensitivity}");
        }
    }


    public void LockCamera(bool state)
    {
        lockMode = state;

        if (state)
        {
            // Tryb Menu/Pauzy: Kursor widoczny i uwolniony
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cinemachineInput.enabled = false; // Wy³¹cz kontrolê kamery w trybie menu/pauzy
        }
        else
        {
            // Tryb Gry: Kursor zablokowany na œrodku i ukryty
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cinemachineInput.enabled = true; // W³¹cz kontrolê kamery w trybie gry
        }
    }
}
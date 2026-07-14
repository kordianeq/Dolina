using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.XInput;


public class CameraControll : MonoBehaviour
{
    [Header("Referencje")]
    /// <summary>
    /// Transform reprezentujący orientację gracza
    /// </summary>
    [SerializeField] private Transform orientation;
    [SerializeField] public Animator fpsAnim;
    [SerializeField] public CinemachineInputAxisController cinemachineInput;
    [SerializeField] public CinemachineCamera cinemachineCamera;
    public UiMenager uiMenager;

   [ Header("Shooting Effects")]
    public AnimationCurve fovEffectCurve; 
    public float fovEffectStrength = 5f;
    public float fovEffectDuration = 0.5f;

    

    [Header("Ustawienia Feelingu (Tilt)")]
    public float tiltAmount = 3f; // Jak mocno kamera przechyla się na boki (zalecane: 2-5)
    public float tiltSpeed = 10f; // Jak szybko wraca do poziomu

    // Zmienne wewnętrzne
    float tiltRotation; // Przechylenie na osi Z
    bool lockMode;
    float originalFov;

 
    void Start()
    {
        LockCamera(false);
    }

    private void Awake()
    {
        cinemachineInput = GetComponent<CinemachineInputAxisController>();
        cinemachineCamera = GetComponent<CinemachineCamera>();
        uiMenager = GameManager.Instance.UiMenager;
        originalFov = cinemachineCamera.Lens.FieldOfView;
    }


    void Update()
    {
        if (lockMode) return;

        CalculateCameraRotation();
    }

    private void LateUpdate()
    {
        if (lockMode) return;

        //camera tilt effect
        cinemachineCamera.Lens.Dutch = tiltRotation;
    }

    void CalculateCameraRotation()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float targetTilt = -inputX * tiltAmount;

        tiltRotation = Mathf.Lerp(tiltRotation, targetTilt, Time.deltaTime * tiltSpeed);

        float tiltThreshold = 0.05f;

        if (Mathf.Abs(tiltRotation) < tiltThreshold)
        {
            tiltRotation = 0f;
        }
    }
    private Coroutine currentEffectRoutine;
    public void ShootEffect()
    {
        uiMenager.shootVignette.Fade();
        if (currentEffectRoutine != null)
        {
            StopCoroutine(currentEffectRoutine);
        }

        // 3. Odpal nową korutynę i od razu zapisz ją do naszego "uchwytu"
        currentEffectRoutine = StartCoroutine(FovEffectRoutine());

    }

   

    IEnumerator FovEffectRoutine() 
    {
        float elapsedTime = 0f;
        float duration = fovEffectDuration;

        while (elapsedTime < duration) 
        {
            
            float t = elapsedTime / duration;

            float effectStrength = fovEffectCurve.Evaluate(t);

            // 3. TUTAJ aplikujesz modyfikator
            // np. aktualneFOV = bazoweFOV + (maxZmianaFOV * effectStrength);
            
            cinemachineCamera.Lens.FieldOfView = originalFov + (fovEffectStrength * effectStrength);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.FieldOfView = originalFov; // Resetuj FOV do wartości bazowej po zakończeniu efektu
        currentEffectRoutine = null;
    }

    

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (cinemachineInput != null)
            {
                cinemachineInput.enabled = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (cinemachineInput != null)
            {
                cinemachineInput.enabled = true;
            }
        }
    }
}
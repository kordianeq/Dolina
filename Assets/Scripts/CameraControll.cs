using UnityEngine;

public class CameraControll : MonoBehaviour
{
    [Header("Ustawienia Czu³oœci")]
    [Tooltip("UWAGA: Zmniejsz te wartoœci w Inspektorze! (np. na 1-2)")]
    public float sensX;
    public float sensY;

    [Header("Referencje")]
    public Transform orientation;
    [SerializeField] public Animator drinkAnim; // Zachowane pole publiczne, tak jak chcia³eœ

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
        // Upewnij siê, ¿e kursor jest zablokowany na starcie
        LockCamera(false);
        sensitivitySlider =  GameManager.Instance.UiMenager.sensitivitySlider;
        sensitivitySlider.value = sensX;

    }

    void Update()
    {
        
        if (lockMode) return; // Jeœli zablokowana, nie wykonuj reszty kodu

        CalculateCameraRotation();
    }

    void CalculateCameraRotation()
    {
        // 1. Pobieranie Inputu Myszki (BEZ Time.deltaTime!)
        // Mno¿ymy tylko przez 0.01, ¿eby wartoœci w inspektorze by³y bardziej czytelne (np. 1-5 zamiast 0.01)
        // Mo¿esz usun¹æ " * 0.01f", jeœli wolisz bardzo ma³e liczby w inspektorze.
        float multiplier = 0.01f;
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX; // Usuniêto Time.deltaTime
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        // 2. Obliczanie Rotacji X i Y
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 3. Obliczanie Przechy³u (Tilt)
        // Pobieramy ruch klawiatury (A/D), aby dodaæ efekt przechy³u
        float inputX = Input.GetAxisRaw("Horizontal");
        float targetTilt = -inputX * tiltAmount;

        // P³ynne przejœcie (Lerp) do docelowego k¹ta przechy³u
        tiltRotation = Mathf.Lerp(tiltRotation, targetTilt, Time.deltaTime * tiltSpeed);

        // 4. Aplikowanie Rotacji
        // Kamera (transform) dostaje X (góra-dó³), Y (lewo-prawo) i Z (przechy³)
        transform.rotation = Quaternion.Euler(xRotation, yRotation, tiltRotation);

        // Cia³o gracza (orientation) obraca siê tylko w osi Y (lewo-prawo)
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    // Ta funkcja pozosta³a bez zmian w logice, aby pasowa³a do GameManagera
    public void LockCamera(bool state /*true == camera locked/menu open */)
    {
        lockMode = state;

        if (state)
        {
            // Tryb Menu/Pauzy: Kursor widoczny i uwolniony
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Tryb Gry: Kursor zablokowany na œrodku i ukryty
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
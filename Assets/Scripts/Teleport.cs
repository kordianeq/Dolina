using UnityEngine;
using UnityEngine.UI;

public class Teleport : MonoBehaviour, IInteracted
{
    private UiMenager menager;
    private Image darkScreen;

    [Header("Ustawienia Aktywacji")]
    [Tooltip("Czy zmiana sceny następuje po wejściu w trigger (true), czy po wciśnięciu 'E' (false).")]
    public bool trigger = true;

    [Tooltip("Czy ładować scenę z ekranem ładowania (asynchronicznie), czy natychmiast.")]
    public bool loadingScreen = false;

    [Tooltip("ID sceny do załadowania w Build Settings.")]
    public int sceneId;

    [Header("Ściemnianie Ekranu na Podstawie Dystansu")]
    [Tooltip("Czy ekran ma się stopniowo ściemniać, im bliżej celu znajduje się gracz.")]
    public bool isDistnaceBased = true;

    [Tooltip("Minimalny dystans (wtedy ekran jest w 100% czarny i następuje teleportacja).")]
    public float minDistance = 0.5f;

    [Tooltip("Maksymalny dystans (od tej odległości ekran zaczyna się ściemniać, poniżej jest przezroczysty).")]
    public float maxDistance = 3f;

    [SerializeField] private float distance = 10f;
    private bool hasTeleported = false;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Start()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (menager == null)
        {
            if (UiMenager.Instance != null)
            {
                menager = UiMenager.Instance;
            }
            else if (GameManager.Instance != null && GameManager.Instance.UiMenager != null)
            {
                menager = GameManager.Instance.UiMenager;
            }
            else
            {
                var canvas = GameObject.FindWithTag("Canvas");
                if (canvas != null)
                {
                    menager = canvas.GetComponent<UiMenager>();
                }
            }
        }

        if (menager != null && darkScreen == null)
        {
            darkScreen = menager.darkScreen;
        }
    }

    public void NewInteraction()
    {
        if (!trigger)
        {
            ExecuteTeleport();
        }
    }

    private void ExecuteTeleport()
    {
        if (hasTeleported) return;
        hasTeleported = true;

        EnsureReferences();
        if (menager == null)
        {
            Debug.LogError("[Teleport] Brak referencji do UiMenager!");
            return;
        }

        Debug.Log($"[Teleport] Zmiana sceny na ID: {sceneId} (LoadingScreen: {loadingScreen})");

        if (loadingScreen)
        {
            menager.ChangeSceneWithLoadingScreen(sceneId);
        }
        else
        {
            menager.OnChangeScene(sceneId);
        }
    }

    private void BlendDarkScreen(float alpha)
    {
        EnsureReferences();
        if (darkScreen == null) return;

        // Kanał alfa w Unity to wartość od 0.0f do 1.0f
        alpha = Mathf.Clamp01(alpha);

        if (alpha <= 0.001f)
        {
            darkScreen.gameObject.SetActive(false);
        }
        else
        {
            if (!darkScreen.gameObject.activeSelf)
            {
                darkScreen.gameObject.SetActive(true);
            }

            Color c = darkScreen.color;
            c.a = alpha;
            darkScreen.color = c;
        }
        if(alpha >= 0.99f)
        {
            ExecuteTeleport();
            darkScreen.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !trigger || hasTeleported) return;

        // Jeśli teleport nie jest oparty o dystans, od razu zmieniamy scenę
        if (!isDistnaceBased)
        {
            ExecuteTeleport();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || !trigger || hasTeleported) return;

        if (isDistnaceBased)
        {
            // Dynamicznie liczymy aktualną pozycję gracza względem punktu teleportu
            distance = Vector3.Distance(other.transform.position, transform.position);

            // Gdy distance == maxDistance -> alpha = 0.0 (przezroczysty)
            // Gdy distance == minDistance -> alpha = 1.0 (pełna czerń)
            float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

            BlendDarkScreen(alpha);

            // Gdy gracz zbliży się na minDistance lub ekran osiągnie pełną czerń -> teleport!
            if (distance <= minDistance || alpha >= 0.99f)
            {
                ExecuteTeleport();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            distance = maxDistance;

            // Jeśli gracz się wycofał przed teleportem, ukrywamy czarny ekran
            if (!hasTeleported)
            {
                BlendDarkScreen(0f);
            }
        }
    }

    private void OnDisable()
    {
        // Zabezpieczenie: przy wyłączeniu obiektu upewniamy się, że nie zostawiamy czarnego ekranu
        if (!hasTeleported && darkScreen != null)
        {
            BlendDarkScreen(0f);
        }
    }
}

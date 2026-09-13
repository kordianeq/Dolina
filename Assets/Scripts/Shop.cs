using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class Shop : MonoBehaviour, IInteracted
{
    public enum ShopState
    {
        Closed,
        Overview,
        InspectingCabinet
    }

    [Header("Kamery (Cinemachine)")]
    [Tooltip("Kamera ogólna wozu / straganu (Overview)")]
    public CinemachineCamera shopCamera;
    public int overviewPriority = 20;
    public int exitPriority = -1;

    [Header("Sekcje sklepu (Szafki, półki)")]
    [Tooltip("Lista szafek w sklepie. Jeśli pusta, zostaną automatycznie znalezione w dzieciach.")]
    public List<ShopCabinet> cabinets = new List<ShopCabinet>();

    [Header("Ustawienia Raycastu")]
    public LayerMask interactableLayers = ~0;
    public float maxRayDistance = 100f;

    [Header("Opcjonalny interfejs UI (Canvas)")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipDescription;
    public TextMeshProUGUI tooltipCost;
    public TextMeshProUGUI hintText;

    [Header("Pomocniczy interfejs ekranowy (OnGUI)")]
    [Tooltip("Wyświetla estetyczny interfejs tekstowy i tooltipy myszy, gdy brak przypisanego Canvasa.")]
    public bool enableOnGuiOverlay = true;

    [Header("Stan (Tylko do odczytu)")]
    [SerializeField] private ShopState currentState = ShopState.Closed;
    [SerializeField] private ShopCabinet currentCabinet;

    private PlayerStats cachedPlayerStats;
    private ShopItem hoveredItem;
    private ShopCabinet hoveredCabinet;
    private float justOpenedTimer = 0f;

    public ShopState CurrentState => currentState;

    private void Awake()
    {
        if (shopCamera != null)
        {
            shopCamera.Priority = exitPriority;
        }

        if (cabinets == null || cabinets.Count == 0)
        {
            cabinets = new List<ShopCabinet>(GetComponentsInChildren<ShopCabinet>(true));
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentState == ShopState.Closed)
        {
            return;
        }

        if (justOpenedTimer > 0f)
        {
            justOpenedTimer -= Time.unscaledDeltaTime;
        }

        // Zabezpieczenie przed zewnętrznym zamknięciem sklepu
        if (GameManager.Instance != null && !GameManager.Instance.isShopping)
        {
            ExitShop(false);
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.UiMenager != null && GameManager.Instance.UiMenager.interactPanel != null && GameManager.Instance.UiMenager.interactPanel.activeSelf)
        {
            GameManager.Instance.UiMenager.interactPanel.SetActive(false);
        }

        HandleBackOrExitInput();
        HandleMouseRaycast();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isShopping && currentState != ShopState.Closed)
        {
            ExitShop(false);
        }
    }

    public void NewInteraction()
    {
        if (currentState != ShopState.Closed) return;

        cachedPlayerStats = FindPlayerStats();

        currentState = ShopState.Overview;
        justOpenedTimer = 0.25f;

        if (shopCamera != null)
        {
            shopCamera.Priority = overviewPriority;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Shopping(true);
            if (GameManager.Instance.UiMenager != null && GameManager.Instance.UiMenager.interactPanel != null)
            {
                GameManager.Instance.UiMenager.interactPanel.SetActive(false);
            }
        }

        Debug.Log("[Shop] Otwarto widok ogólny sklepu.");
    }

    private void HandleBackOrExitInput()
    {
        // PPM (prawy klik myszy), Escape, lub E (po upływie timera otwarcia)
        bool backPressed = Input.GetMouseButtonDown(1) ||
                           Input.GetKeyDown(KeyCode.Escape) ||
                           (justOpenedTimer <= 0f && Input.GetKeyDown(KeyCode.E));

        if (!backPressed) return;

        if (currentState == ShopState.InspectingCabinet)
        {
            CloseCurrentCabinet();
        }
        else if (currentState == ShopState.Overview)
        {
            ExitShop(true);
        }
    }

    private void HandleMouseRaycast()
    {
        hoveredItem = null;
        hoveredCabinet = null;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, interactableLayers))
        {
            if (currentState == ShopState.Overview)
            {
                // W widoku ogólnym szukamy szafki lub ewentualnie wolnostojącego przedmiotu na ladzie
                var cabinet = hit.collider.GetComponentInParent<ShopCabinet>();
                if (cabinet != null)
                {
                    hoveredCabinet = cabinet;
                    if (Input.GetMouseButtonDown(0))
                    {
                        OpenCabinet(cabinet);
                        return;
                    }
                }
                else
                {
                    var item = hit.collider.GetComponentInParent<ShopItem>();
                    if (item != null && !item.isPurchased)
                    {
                        hoveredItem = item;
                        if (Input.GetMouseButtonDown(0))
                        {
                            TryPurchaseItem(item);
                        }
                    }
                }
            }
            else if (currentState == ShopState.InspectingCabinet)
            {
                // W zbliżeniu na szafkę celujemy w konkretne przedmioty
                var item = hit.collider.GetComponentInParent<ShopItem>();
                if (item != null && !item.isPurchased)
                {
                    hoveredItem = item;
                    if (Input.GetMouseButtonDown(0))
                    {
                        TryPurchaseItem(item);
                    }
                }
            }
        }

        UpdateCanvasTooltip();
    }

    private void OpenCabinet(ShopCabinet cabinet)
    {
        if (cabinet == null) return;

        currentCabinet = cabinet;
        currentCabinet.Open();
        currentState = ShopState.InspectingCabinet;

        Debug.Log($"[Shop] Zbliżenie na sekcję: {cabinet.cabinetName}");
    }

    private void CloseCurrentCabinet()
    {
        if (currentCabinet != null)
        {
            currentCabinet.Close();
            currentCabinet = null;
        }

        currentState = ShopState.Overview;

        if (shopCamera != null)
        {
            shopCamera.Priority = overviewPriority;
        }

        Debug.Log("[Shop] Powrót do widoku ogólnego sklepu.");
    }

    public void ExitShop(bool notifyGameManager = true)
    {
        if (currentCabinet != null)
        {
            currentCabinet.Close();
            currentCabinet = null;
        }

        if (shopCamera != null)
        {
            shopCamera.Priority = exitPriority;
        }

        currentState = ShopState.Closed;
        hoveredItem = null;
        hoveredCabinet = null;

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        if (notifyGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.Shopping(false);
        }

        Debug.Log("[Shop] Zamknięto sklep i przywrócono sterowanie graczem.");
    }

    private void TryPurchaseItem(ShopItem item)
    {
        if (item == null) return;
        if (cachedPlayerStats == null) cachedPlayerStats = FindPlayerStats();

        if (cachedPlayerStats != null)
        {
            bool success = item.Buy(cachedPlayerStats);
            if (success)
            {
                UpdateCanvasTooltip();
            }
        }
    }

    private PlayerStats FindPlayerStats()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerRef != null)
        {
            var stats = GameManager.Instance.PlayerRef.GetComponent<PlayerStats>();
            if (stats != null) return stats;
        }
        return FindFirstObjectByType<PlayerStats>();
    }

    private void UpdateCanvasTooltip()
    {
        if (tooltipPanel == null) return;

        if (hoveredItem != null)
        {
            tooltipPanel.SetActive(true);
            if (tooltipTitle != null) tooltipTitle.text = hoveredItem.itemName;
            if (tooltipDescription != null) tooltipDescription.text = hoveredItem.description;

            if (tooltipCost != null)
            {
                float currentSwietosc = cachedPlayerStats != null ? cachedPlayerStats.swietosc : 0f;
                bool canAfford = hoveredItem.CanAfford(currentSwietosc);
                string colorHex = canAfford ? "#55FF55" : "#FF5555";
                tooltipCost.text = $"Koszt: <color={colorHex}>{hoveredItem.costSwietosc} Świętości</color>";
            }
        }
        else
        {
            tooltipPanel.SetActive(false);
        }

        if (hintText != null)
        {
            if (currentState == ShopState.InspectingCabinet)
            {
                hintText.text = "[LPM] Kup przedmiot   |   [PPM / ESC / E] Wróć do wozu";
            }
            else if (currentState == ShopState.Overview)
            {
                hintText.text = "[LPM] Otwórz szafkę   |   [PPM / ESC / E] Wyjdź ze sklepu";
            }
        }
    }

    private void OnGUI()
    {
        if (!enableOnGuiOverlay || currentState == ShopState.Closed) return;

        float currentSwietosc = cachedPlayerStats != null ? cachedPlayerStats.swietosc : 0f;

        // Dolny pasek informacyjny
        float barWidth = 600f;
        float barHeight = 42f;
        float barX = (Screen.width - barWidth) / 2f;
        float barY = Screen.height - barHeight - 20f;

        GUI.Box(new Rect(barX, barY, barWidth, barHeight), GUIContent.none);

        GUIStyle barStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        barStyle.normal.textColor = Color.white;

        string controlsHint = currentState == ShopState.InspectingCabinet
            ? $"Świętość: {currentSwietosc:F0}   |   [LPM] Kup przedmiot   |   [PPM / ESC / E] Wróć do wozu"
            : $"Świętość: {currentSwietosc:F0}   |   [LPM] Wybierz sekcję / towar   |   [PPM / ESC / E] Wyjdź ze sklepu";

        GUI.Label(new Rect(barX, barY, barWidth, barHeight), controlsHint, barStyle);

        // Tooltip nad kursorem myszy (jeśli nie używamy panelu Canvas)
        if (tooltipPanel == null)
        {
            Vector2 mousePos = Event.current.mousePosition;

            if (hoveredItem != null)
            {
                float tipW = 260f;
                float tipH = 110f;
                float tipX = Mathf.Clamp(mousePos.x + 15f, 10f, Screen.width - tipW - 10f);
                float tipY = Mathf.Clamp(mousePos.y + 15f, 10f, Screen.height - tipH - 10f);

                GUI.Box(new Rect(tipX, tipY, tipW, tipH), GUIContent.none);

                GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold
                };
                titleStyle.normal.textColor = Color.yellow;

                GUIStyle descStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    wordWrap = true
                };
                descStyle.normal.textColor = Color.white;

                GUIStyle costStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
                bool canAfford = hoveredItem.CanAfford(currentSwietosc);
                costStyle.normal.textColor = canAfford ? Color.green : Color.red;

                GUI.Label(new Rect(tipX + 10f, tipY + 8f, tipW - 20f, 22f), hoveredItem.itemName, titleStyle);
                GUI.Label(new Rect(tipX + 10f, tipY + 30f, tipW - 20f, 45f), hoveredItem.description, descStyle);
                GUI.Label(new Rect(tipX + 10f, tipY + 78f, tipW - 20f, 24f), $"Koszt: {hoveredItem.costSwietosc:F0} Świętości", costStyle);
            }
            else if (hoveredCabinet != null && currentState == ShopState.Overview)
            {
                float tipW = 220f;
                float tipH = 45f;
                float tipX = Mathf.Clamp(mousePos.x + 15f, 10f, Screen.width - tipW - 10f);
                float tipY = Mathf.Clamp(mousePos.y + 15f, 10f, Screen.height - tipH - 10f);

                GUI.Box(new Rect(tipX, tipY, tipW, tipH), GUIContent.none);

                GUIStyle cabStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
                cabStyle.normal.textColor = Color.white;

                GUI.Label(new Rect(tipX, tipY, tipW, tipH), $"[LPM] {hoveredCabinet.cabinetName}", cabStyle);
            }
        }
    }
}

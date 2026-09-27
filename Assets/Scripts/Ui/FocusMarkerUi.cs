using UnityEngine;
using UnityEngine.UI;

public class FocusMarkerUI : MonoBehaviour
{
    [Header("Komponenty UI")]
    public RectTransform rectTransform;
    public Image dotImage;
    public Image circleImage;
    public Image skullImage;

    [Header("Ustawienia Animacji Okręgu")]
    [Tooltip("Początkowa skala okręgu przy 0% (np. 0.2 - tuż przy kropce).")]
    public float minCircleScale = 0.25f;
    [Tooltip("Maksymalna skala okręgu przy 100% / 200 DMG.")]
    public float maxCircleScale = 1.0f;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (skullImage != null) skullImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Aktualizuje pozycję, progres naładowania i wygląd znacznika.
    /// </summary>
    public void UpdateMarker(Vector2 screenPosition, float percentage, Color defaultCol, Color lethalCol)
    {
        // 1. Zabezpieczenie przed niewybranym/przezroczystym kolorem z Inspektora (RGBA 0,0,0,0)
        if (defaultCol.a <= 0.05f) defaultCol = Color.white;
        if (lethalCol.a <= 0.05f) lethalCol = Color.red;

        // 2. Ustawienie pozycji na ekranie
        if (rectTransform != null)
        {
            rectTransform.position = screenPosition;
        }

        // 3. Skalowanie okręgu wraz z progresem (od min do max)
        float currentScale = Mathf.Lerp(minCircleScale, maxCircleScale, percentage);
        if (circleImage != null)
        {
            circleImage.transform.localScale = new Vector3(currentScale, currentScale, 1f);
        }

        // 4. Sprawdzenie progu śmiertelnego (100% / 200 DMG)
        bool isLethal = percentage >= 1.0f;

        if (isLethal)
        {
            // Osiągnięto 200 DMG -> Pokazujemy czaszkę, zmieniamy kolor na śmiertelny
            if (skullImage != null) skullImage.gameObject.SetActive(true);
            if (dotImage != null) dotImage.gameObject.SetActive(false); // Czaszka zastępuje kropkę

            if (circleImage != null) circleImage.color = lethalCol;
            if (skullImage != null) skullImage.color = lethalCol;
        }
        else
        {
            // Ładowanie -> Widoczna kropka i rosnący okrąg w kolorze domyślnym
            if (skullImage != null) skullImage.gameObject.SetActive(false);
            if (dotImage != null) dotImage.gameObject.SetActive(true);

            if (dotImage != null) dotImage.color = defaultCol;
            if (circleImage != null) circleImage.color = defaultCol;
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
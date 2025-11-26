using System.Collections;
using UnityEngine;

public class damageOverlay : MonoBehaviour
{
    PlayerStats playerStats;
    CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration, peakAlpha;
    float alphaAdded = 0;
    bool corutineStarted = false;
    private void Awake()
    {
        playerStats = GameManager.Instance.PlayerStats;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Teraz bezpiecznie pobierz statystyki, bo Gracz na pewno już je zarejestrował
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
        {
            playerStats = GameManager.Instance.PlayerStats;
        }
        else
        {
            Debug.LogError("Brak PlayerStats w GameManagerze! Czy gracz jest na scenie?");
        }
    }
    void Update()
    {
        if (playerStats.playerHp / playerStats.maxPlayerHp <= 0.3f)
        {
            alphaAdded = 0.5f;
        }
        else if (playerStats.playerHp / playerStats.maxPlayerHp > 0.3f)
        {
            alphaAdded = 0f;
        }

    }
    
    public void Damaged()
    {
        if (!corutineStarted) StartCoroutine(FadePanel(fadeDuration, peakAlpha, currentValue => canvasGroup.alpha = currentValue + alphaAdded));
        //Debug.Log("Damage overlay triggered");
    }
    private IEnumerator FadePanel(float duration, float peakValue, System.Action<float> onUpdate)
    {
        corutineStarted = true;


        float halfDuration = duration / 2.0f;
        float timer = 0f;

        // --- Faza 1: Animacja z 0 do 8 ---
        while (timer < halfDuration)
        {
            // Mathf.Lerp liczy wartość pośrednią. timer / halfDuration daje nam postęp od 0.0 do 1.0
            float currentValue = Mathf.Lerp(0f, peakValue, timer / halfDuration);

            // Wywołujemy akcję (funkcję), którą podaliśmy w StartCoroutine, przekazując jej obliczoną wartość
            onUpdate(currentValue);

            // Zwiększamy timer o czas, jaki upłynął od ostatniej klatki
            timer += Time.deltaTime;

            // Kończymy tę klatkę i wracamy tu w następnej
            yield return null;
        }

        // --- Faza 2: Animacja z 8 do 0 ---
        timer = 0f; // Resetujemy timer
        while (timer < halfDuration)
        {
            float currentValue = Mathf.Lerp(peakValue, 0f, timer / halfDuration);
            onUpdate(currentValue);

            timer += Time.deltaTime;
            yield return null;
        }

        // --- Zakończenie ---
        // Upewniamy się, że na końcu wartość to DOKŁADNIE 0
        onUpdate(0f);

        corutineStarted = false;
    }
}

using UnityEngine;
using TMPro;

public class UiDmgNumber : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro damageText; // Zmień na TextMeshProUGUI, jeśli używasz Canvasu
    private Transform camTransform;

    [Header("Settings")]
    public float lifetime = 1f;
    public float floatSpeed = 2f;
    public Vector2 randomizeOffset = new Vector2(0.5f, 0.5f);

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        // Jeśli nie przypisano w Inspektorze, szukamy na tym obiekcie LUB w jego dzieciach
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshPro>();
            if (damageText == null)
            {
                damageText = GetComponentInChildren<TextMeshPro>();
            }
        }

        if (camTransform == null && GameManager.Instance != null && GameManager.Instance.PlayerCam != null)
        {
            camTransform = GameManager.Instance.PlayerCam.transform;
        }
    }

    public void SetDamageNumber(float damageAmount)
    {
        EnsureReferences();

        if (damageText != null)
        {
            int roundedDamage = Mathf.RoundToInt(damageAmount);
            damageText.SetText(roundedDamage.ToString());
        }
        else
        {
            Debug.LogError($"[UiDmgNumber] Brak komponentu TextMeshPro na obiekcie '{gameObject.name}' lub jego dzieciach!", this);
        }
    }

    public void PlayDamageNumberEffect(Vector3 hitPosition, float damageAmount)
    {
        
        SetDamageNumber(damageAmount);

        if (damageText != null)
        {
            Color c = damageText.color;
            c.a = 1f;
            damageText.color = c;
        }

        EnsureReferences();

        if (camTransform != null)
        {
            Vector3 offset = (camTransform.right * Random.Range(-randomizeOffset.x, randomizeOffset.x)) +
                             (camTransform.up * Random.Range(-randomizeOffset.y, randomizeOffset.y));

            transform.position = hitPosition + offset;
        }
        else
        {
            transform.position = hitPosition;
        }
    }

    void Update()
    {
        if (damageText == null) return;

        // Billboard - obracanie do kamery
        if (camTransform != null)
        {
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward, camTransform.rotation * Vector3.up);
        }

        // Unoszenie tekstu
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Płynne zanikanie (Alpha)
        Color tempColor = damageText.color;
        tempColor.a -= (1f / lifetime) * Time.deltaTime;
        damageText.color = tempColor;

        // Niszczenie obiektu
        if (damageText.color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
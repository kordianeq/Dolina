using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ShopItem : MonoBehaviour
{
    [Header("Informacje o przedmiocie")]
    public string itemName = "Ulepszenie";
    [TextArea(2, 4)]
    public string description = "Opis ulepszenia lub przedmiotu.";
    public float costSwietosc = 50f;
    public bool isOneTimePurchase = true;
    public bool isPurchased = false;

    [Header("Wizualia (Opcjonalne)")]
    [Tooltip("Model 3D przedmiotu. Jeśli pusty, użyty zostanie ten GameObject.")]
    public GameObject visualModel;
    [Tooltip("Dźwięk zakupu przedmiotu.")]
    public AudioClip purchaseSound;
    [Tooltip("Efekt cząsteczkowy przy zakupie.")]
    public GameObject purchaseEffectPrefab;

    [Header("Działanie ulepszenia")]
    public UpgradeType upgradeType = UpgradeType.Custom;
    public float upgradeAmount = 25f;

    [Header("Własne zdarzenie po zakupie")]
    public UnityEvent onPurchased;

    public enum UpgradeType
    {
        Custom,
        MaxHp,
        Heal,
        AddThrowables,
        DamageReduction
    }

    private void Awake()
    {
        if (visualModel == null)
        {
            visualModel = gameObject;
        }
    }

    public bool CanAfford(float currentSwietosc)
    {
        return !isPurchased && currentSwietosc >= costSwietosc;
    }

    public bool Buy(PlayerStats stats)
    {
        if (isPurchased) return false;
        if (stats == null) return false;

        if (!CanAfford(stats.swietosc))
        {
            Debug.Log($"[ShopItem] Za mało Świętości na zakup '{itemName}'! Wymagane: {costSwietosc}, masz: {stats.swietosc}");
            return false;
        }

        stats.AddSwietosc(-costSwietosc);
        ApplyUpgrade(stats);

        if (purchaseSound != null)
        {
            AudioSource.PlayClipAtPoint(purchaseSound, transform.position);
        }

        if (purchaseEffectPrefab != null)
        {
            Instantiate(purchaseEffectPrefab, transform.position, transform.rotation);
        }

        onPurchased?.Invoke();

        if (isOneTimePurchase)
        {
            isPurchased = true;
            if (visualModel != null && visualModel != gameObject)
            {
                visualModel.SetActive(false);
            }
            else
            {
                if (TryGetComponent<Collider>(out var col)) col.enabled = false;
                if (TryGetComponent<Renderer>(out var rend)) rend.enabled = false;
                foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            }
        }

        Debug.Log($"[ShopItem] Pomyślnie zakupiono '{itemName}' za {costSwietosc} Świętości.");
        return true;
    }

    private void ApplyUpgrade(PlayerStats stats)
    {
        switch (upgradeType)
        {
            case UpgradeType.MaxHp:
                stats.maxPlayerHp += upgradeAmount;
                stats.playerHp += upgradeAmount;
                break;
            case UpgradeType.Heal:
                stats.playerHp = Mathf.Min(stats.playerHp + upgradeAmount, stats.maxPlayerHp);
                break;
            case UpgradeType.AddThrowables:
                stats.throwablesCount += (int)upgradeAmount;
                if (GameManager.Instance != null && GameManager.Instance.UiMenager != null)
                {
                    GameManager.Instance.UiMenager.UpdateThrowableCount(stats.throwablesCount);
                }
                break;
            case UpgradeType.DamageReduction:
                stats.DamageReduction(upgradeAmount);
                break;
            case UpgradeType.Custom:
                break;
        }
    }
}

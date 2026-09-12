using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ArenaTrigger : MonoBehaviour
{
    [Tooltip("Strefa areny, która ma zostać aktywowana po wejściu gracza w ten trigger.")]
    public EnemyZone targetZone;

    [Tooltip("Czy trigger ma zadziałać tylko jeden raz?")]
    public bool triggerOnce = true;

    [Header("Eventy (opcjonalne)")]
    [Tooltip("Opcjonalny event wywoływany w momencie wejścia gracza w ten trigger.")]
    public UnityEvent onTriggerEntered;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (targetZone == null)
        {
            targetZone = GetComponentInParent<EnemyZone>();
        }
    }

    private void Start()
    {
        if (targetZone != null)
        {
            targetZone.RegisterTrigger(this);
        }
    }

    private void OnEnable()
    {
        if (targetZone != null)
        {
            targetZone.RegisterTrigger(this);
            targetZone.onZoneReset.AddListener(ResetTrigger);
        }
    }

    private void OnDisable()
    {
        if (targetZone != null)
        {
            targetZone.UnregisterTrigger(this);
            targetZone.onZoneReset.RemoveListener(ResetTrigger);
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null)
        {
            hasTriggered = true;
            onTriggerEntered?.Invoke();

            if (targetZone != null)
            {
                targetZone.OnPlayerEnterZone();
            }
            else
            {
                Debug.LogWarning($"[ArenaTrigger] Na obiekcie '{gameObject.name}' nie przypisano żadnej strefy 'targetZone'!");
                if (triggerOnce)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}


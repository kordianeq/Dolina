using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Czy checkpoint ma się aktywować tylko jeden raz?")]
    public bool triggerOnce = true;

    [Tooltip("Czy niszczyć obiekt po aktywacji? (Domyślnie true)")]
    public bool destroyOnTrigger = true;

    [Header("Eventy (opcjonalne)")]
    public UnityEvent onCheckpointTriggered;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null)
        {
            hasTriggered = true;
            SaveSystem.TriggerCheckpoint();
            onCheckpointTriggered?.Invoke();

            if (destroyOnTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}

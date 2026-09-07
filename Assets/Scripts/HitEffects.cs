using UnityEngine;

public class HitEffectsManager : MonoBehaviour
{
    public GameObject[] hitEffectPrefab;
    // 4. Metoda MUSI być publiczna, żeby Unity mogło ją zobaczyć w Inspektorze.
    // Parametr musi się zgadzać z typem, który wysyła pistolet (czyli GameObject).
    public void PlayHitEffect(Vector3 hitPosition)
    {
        if (hitEffectPrefab == null || hitEffectPrefab.Length == 0)
        {
            Debug.LogWarning("HitEffectsManager: 'hitEffectPrefab' is not assigned or empty. Assign prefabs in the Inspector.");
            return;
        }
        var idx = Random.Range(0, hitEffectPrefab.Length);
        var prefab = hitEffectPrefab[idx];
        if (prefab == null)
        {
            Debug.LogWarning($"HitEffectsManager: prefab at index {idx} is null. Please assign a valid prefab.");
            return;
        }
        Instantiate(prefab, hitPosition, GameManager.Instance.PlayerCam.transform.rotation);
    }
}
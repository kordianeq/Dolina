using UnityEngine;
using TMPro;

public class HitEffectsManager : MonoBehaviour
{
    public GameObject[] hitEffectPrefab;
    public GameObject damageNumberPrefab;
    
    public void PlayHitEffect(Vector3 hitPosition, float damageAmount)
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
    public void PlayDamageNumberEffect(Vector3 hitPosition, float damageAmount)
    {

        if (damageNumberPrefab == null) return;
        if(!GameManager.Instance.allowDamageNumbers) return;
        var damageNumberInstance = Instantiate(damageNumberPrefab, hitPosition, GameManager.Instance.PlayerCam.transform.rotation);
        damageNumberInstance.GetComponent<UiDmgNumber>().PlayDamageNumberEffect(hitPosition, damageAmount);
        
        
    }
}
using UnityEngine;

public class HitEffectsManager : MonoBehaviour
{
    public GameObject[] hitEffectPrefab;
    // 4. Metoda MUSI być publiczna, żeby Unity mogło ją zobaczyć w Inspektorze.
    // Parametr musi się zgadzać z typem, który wysyła pistolet (czyli GameObject).
    public void PlayHitEffect(Vector3 hitPosition)
    {
        Instantiate(hitEffectPrefab[Random.Range(0, hitEffectPrefab.Length)], hitPosition, GameManager.Instance.PlayerCam.transform.rotation);
    }
}
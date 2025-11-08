using UnityEngine;
using System.Collections;
using UnityEngine.Audio;


public class Horse : MonoBehaviour, IInteracted
{
    public float hungerLevel = 100f;
    public float hungerDecreaseRate = 1f;

    [HideInInspector] public float hp;
    public float maxHp = 100f;
    AudioManager audioManager;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] Hit;
    [SerializeField] AudioClip Death;
    [SerializeField] AudioClip Eat;
    [SerializeField] AudioClip[] Footsteps;
    [SerializeField] AudioClip[] Idle;

    void Awake()
    {
        hp = maxHp;
        Debug.Log("Horse Awake");
        audioManager = GetComponent<AudioManager>();
        StartCoroutine(playRandomIdle());
        
    }
   
    IEnumerator playRandomIdle()
    {
        while (true)
        {
            var randomValue = Random.Range(0, 20);
            if (randomValue == 9)
            {
                audioManager.PlaySound(Idle);
            }
            yield return new WaitForSeconds(1);
        }

    }
    public void HitSound()
    {
        audioManager.PlaySound(Hit);
    }
    public void DeathSound()
    {
        audioManager.PlaySound(Death);
    }
    public void EatSound()
    {
        audioManager.PlaySound(Eat);
    }
   

    void FixedUpdate()
    {
        DecreaseHunger();
    }

    void DecreaseHunger()
    {
        hungerLevel = hungerLevel - hungerDecreaseRate * Time.fixedDeltaTime;
    }
    void Hungry()
    {
        Debug.Log("The horse is hungry.");
    }
    public void NewInteraction()
    {
        Debug.Log("Horse interaction triggered.");
        //gameObject.GetComponent<BoxCollider>().enabled = false;
        GameManager.Instance.HorseMount();
    }


}

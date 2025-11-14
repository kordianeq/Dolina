using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.AI;


public class Horse : MonoBehaviour, IInteracted, IDamagable
{
    public float kickDamage;
    [HideInInspector] public bool isDead = false;
    public float hungerLevel = 100f;
    public float hungerDecreaseRate = 1f;

    [HideInInspector] public float hp;
    public float maxHp = 100f;
    AudioManager audioManager;

    [Header("Mount Settings")]
    public float maxSpeed;
    public Transform playerSlot;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] Hit;
    [SerializeField] AudioClip Death;
    [SerializeField] AudioClip Eat;
    [SerializeField] AudioClip[] Footsteps;
    [SerializeField] AudioClip[] Idle;

    HorseAi horseAi;
    void Awake()
    {
        hp = maxHp;
        //Debug.Log("Horse Awake");
        audioManager = GetComponent<AudioManager>();
        StartCoroutine(playRandomIdle());
        horseAi = GetComponent<HorseAi>();
    }
   
    IEnumerator playRandomIdle()
    {
        while (true)
        {
            var randomValue = Random.Range(0, 10);
            if (randomValue == 9)
            {
                audioManager.PlaySound(Idle);
            }
            yield return new WaitForSeconds(1);
        }

    }

    public void Damaged(float damage)
    {
        Debug.Log("Horse took damage: " + damage);
        hp -= damage;
        HitSound();
        if (hp <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Debug.Log("Horse has died.");
        isDead = true;
        horseAi.horseAnimator.Play("HorseDeath");
        DeathSound();

        
    }
    public void HitSound()
    {
        audioManager.PlaySound(Hit);
    }
    public void DeathSound()
    {
        audioManager.PlaySound(Death);
        Invoke(nameof(HorseDestroy), Death.length);

    }
    void HorseDestroy()
    {
        Destroy(gameObject);
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
        GetComponent<HorseAi>().mounted = true;

        GameManager.Instance.HorseMount(this);
        gameObject.transform.SetParent(GameManager.Instance.playerRef.gameObject.transform, false);
        gameObject.transform.position = GameManager.Instance.playerRef.gameObject.transform.position; 
        
    }
  


}

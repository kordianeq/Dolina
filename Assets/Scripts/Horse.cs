using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.AI;

public class Horse : MonoBehaviour, IInteracted, IDamagable
{
    public float kickDamage = 15f;
    [HideInInspector] public bool isDead = false;
    public float hungerLevel = 100f;
    public float hungerDecreaseRate = 1f;
    public float thirstLevel =100f;
    public float thirstDecreaseRate = 0.1f;

    [HideInInspector] public float hp;
    public float maxHp = 100f;
    private AudioMaker audioMaker;

    [Header("Mount Settings")]
    public float maxSpeed = 12f;
    public Transform playerSlot;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] Hit;
    [SerializeField] private AudioClip Death;
    [SerializeField] private AudioClip Eat;
    [SerializeField] private AudioClip[] Footsteps;
    [SerializeField] private AudioClip[] Idle;

    private HorseAi horseAi;
    private Coroutine idleCoroutine;

    private void Awake()
    {
        hp = maxHp;
        audioMaker = GetComponent<AudioMaker>();
        horseAi = GetComponent<HorseAi>();
        idleCoroutine = StartCoroutine(PlayRandomIdle());
    }

    private IEnumerator PlayRandomIdle()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(4f, 8f));
            if (!isDead && audioMaker != null && Idle != null && Idle.Length > 0)
            {
                var randomValue = Random.Range(0, 10);
                if (randomValue >= 7)
                {
                    audioMaker.PlaySound(Idle);
                }
            }
        }
    }

    public void Damaged(float damage)
    {
        if (isDead) return;
        Debug.Log($"[Horse] Otrzymano {damage} obrażeń. Aktualne HP: {hp - damage}/{maxHp}");
        hp -= damage;
        HitSound();

        // Powiadomienie AI o otrzymaniu ciosu (np. spłoszenie/panika)
        if (horseAi != null)
        {
            horseAi.OnTookDamage();
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        Debug.Log("[Horse] Koń zginął.");
        isDead = true;

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        if (horseAi != null && horseAi.horseAnimator != null)
        {
            horseAi.horseAnimator.Play("HorseDeath");
        }

        DeathSound();
    }

    public void HitSound()
    {
        if (audioMaker != null && Hit != null && Hit.Length > 0)
        {
            audioMaker.PlaySound(Hit);
        }
    }

    public void DeathSound()
    {
        float destroyDelay = 3.0f;
        if (Death != null)
        {
            audioMaker?.PlaySound(Death);
            destroyDelay = Mathf.Max(destroyDelay, Death.length);
        }

        Invoke(nameof(HorseDestroy), destroyDelay);
    }

    private void HorseDestroy()
    {
        Destroy(gameObject);
    }

    public void EatSound()
    {
        if (audioMaker != null && Eat != null)
        {
            audioMaker.PlaySound(Eat);
        }
    }

    private void FixedUpdate()
    {
        DecreaseHunger();
    }

    private void DecreaseHunger()
    {
        hungerLevel = Mathf.Max(0f, hungerLevel - hungerDecreaseRate * Time.fixedDeltaTime);
    }

    public void NewInteraction()
    {
        Debug.Log("[Horse] Interakcja z koniem (Jazda konna w przygotowaniu).");
        // Miejsce na przyszłą integrację jazdy konnej
    }
}

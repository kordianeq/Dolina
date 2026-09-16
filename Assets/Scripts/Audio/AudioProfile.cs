using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioProfile", menuName = "Audio/Audio Profile")]
public class AudioProfile : ScriptableObject
{
    [Header("Muzyka")]
    [Tooltip("Główny utwór muzyczny profilu.")]
    public AudioClip musicClip;
    [Range(0f, 1f), Tooltip("Względna głośność tego utworu (balansowanie utworu).")]
    public float musicVolume = 1f;
    public bool loopMusic = true;

    [Header("Otoczenie (Ambience)")]
    [Tooltip("Dźwięk otoczenia (wiatr, pustynia, cykady, wnętrze saloonu itp.).")]
    public AudioClip ambientClip;
    [Range(0f, 1f), Tooltip("Względna głośność tego tła.")]
    public float ambientVolume = 1f;
    public bool loopAmbience = true;

    [Header("Płynne przejście")]
    [Tooltip("Czas w sekundach na płynne wyciszenie starego i wejście nowego profilu.")]
    public float crossfadeDuration = 1.5f;
}


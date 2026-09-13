using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class ShopCabinet : MonoBehaviour
{
    [Header("Informacje o sekcji")]
    [Tooltip("Nazwa sekcji / szafki wyświetlana graczowi na widoku ogólnym.")]
    public string cabinetName = "Szafka";

    [Header("Kamera sekcji (Cinemachine)")]
    [Tooltip("Kamera zbliżeniowa skierowana na tę szafkę / półkę.")]
    public CinemachineCamera inspectCamera;
    public int activeCameraPriority = 30;
    public int inactiveCameraPriority = -1;

    [Header("Animacja szafki (Opcjonalnie)")]
    public Animator cabinetAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";

    [Header("Dźwięki (Opcjonalnie)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Przedmioty w tej szafce")]
    [Tooltip("Lista przedmiotów w tej sekcji. Jeśli pusta, zostaną automatycznie pobrane z obiektów potomnych.")]
    public List<ShopItem> items = new List<ShopItem>();

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (inspectCamera != null)
        {
            inspectCamera.Priority = inactiveCameraPriority;
        }

        if (items == null || items.Count == 0)
        {
            items = new List<ShopItem>(GetComponentsInChildren<ShopItem>(true));
        }
    }

    public void Open()
    {
        IsOpen = true;

        if (inspectCamera != null)
        {
            inspectCamera.Priority = activeCameraPriority;
        }

        if (cabinetAnimator != null && !string.IsNullOrEmpty(openTrigger))
        {
            cabinetAnimator.SetTrigger(openTrigger);
        }

        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }

    public void Close()
    {
        IsOpen = false;

        if (inspectCamera != null)
        {
            inspectCamera.Priority = inactiveCameraPriority;
        }

        if (cabinetAnimator != null && !string.IsNullOrEmpty(closeTrigger))
        {
            cabinetAnimator.SetTrigger(closeTrigger);
        }

        if (closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
        }
    }
}


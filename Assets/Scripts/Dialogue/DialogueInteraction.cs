using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;

public class DialogueInteraction : MonoBehaviour, IInteracted
{
    public GameObject beginingDialogue;
    UiMenager uiMenager;
    
    public string NpcName;
    public Sprite NpcImage;
    
    [Header("Optional, use if script is not used as main dialogue executor")]
    [SerializeField] DialogueInteraction parentDialogueInteraction;
    IDialogue nextDialogueI;
    public void NewInteraction()
    {   if(parentDialogueInteraction != null)
        {
            NpcName = parentDialogueInteraction.NpcName;
            NpcImage = parentDialogueInteraction.NpcImage;
        }
        
        StartDialogue();
    }
    void Start()
    {
        uiMenager = UiMenager.Instance;
        uiMenager.dialogueImage.gameObject.SetActive(false);
        if(beginingDialogue.TryGetComponent<IDialogue>(out IDialogue dialogue))
        {
            nextDialogueI = dialogue;
            
        }
        else
        {
            Debug.LogError("Failed to get dialogue element");
        }
    }
    void StartDialogue()
    {
        uiMenager.Dialogue(true);
        
        nextDialogueI.NextLine();
        
    }
}

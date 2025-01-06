using UnityEngine;

public class DialogueInteraction : MonoBehaviour, IInteracted
{
    public GameObject beginingDialogue;
    UiMenager uiMenager;
    
    IDialogue nextDialogueI;
    public void NewInteraction()
    {
        Debug.Log("cos");
        StartDialogue();
    }
    void Start()
    {
        uiMenager = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UiMenager>();
        
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

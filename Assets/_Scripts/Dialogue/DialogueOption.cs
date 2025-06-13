using UnityEngine;

public enum ChoiceType
{
    Exit,
    NextDialogue,
    Interaction
}
public class DialogueOption : MonoBehaviour, IDialogue
{
    public string optionText;
    public ChoiceType choiceType;
    [Header("Not needed if 'Exit' selected")]
    public GameObject nextDialogueObj;

    
    UiMenager menager;
    void Start()
    {
        menager = GameObject.FindWithTag("Canvas").GetComponent<UiMenager>();
    }
    public void NextLine()
    {
       

        switch (choiceType)
        {
            case ChoiceType.Exit:
                DialogueExit();
                return;
            case ChoiceType.NextDialogue:
                nextDialogueObj.GetComponent<IDialogue>().NextLine();
                return;
            case ChoiceType.Interaction:
                nextDialogueObj.GetComponent<IInteracted>().NewInteraction();
                return;
            default:
                return;
        }
    }



    void DialogueExit()
    {
        menager.Dialogue(false);

    }
}

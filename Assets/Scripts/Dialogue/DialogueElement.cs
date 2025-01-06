using UnityEngine;

public class DialogueElement : MonoBehaviour, IDialogue
{
    UiMenager menager;
    public GameObject nextDialogueObj;
    [Header("Text")]
    [SerializeField] string DialogueName;
    [SerializeField] string DialogueText;

    public KeyCode dialogueSkip = KeyCode.Space;

    public bool isChoice;
    public bool isExit;

    private void Start()
    {
        menager = GameObject.FindWithTag("Canvas").GetComponent<UiMenager>();

    }
    public void NextLine()
    {
        UpdateText();
    }
    void UpdateText()
    {
        menager.dialogueName.text = DialogueName;
        if (isChoice)
        {
            menager.dialogueText.gameObject.SetActive(false);
            menager.dialogueChoicePanel.gameObject.SetActive(true);
        }
        else
        {
            menager.dialogueChoicePanel.gameObject.SetActive(false);
            menager.dialogueText.gameObject.SetActive(true);
            menager.dialogueText.text = DialogueText;
            

        }


    }

    void Skip()// launches next file of this type do make chain reaction of dialogues
    {
        if(nextDialogueObj)
        {
            if (nextDialogueObj.TryGetComponent(out IDialogue nextOne))
            {
                nextOne.NextLine();
            }
            else
            {
                DialogueExit();
            }
        }
        else
        {
            DialogueExit();
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(dialogueSkip))
        {
            Skip();
        }
    }

    void DialogueExit()
    {
        menager.Dialogue(false);
        
    }
}


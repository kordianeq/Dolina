using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueElement : MonoBehaviour, IDialogue
{
    UiMenager menager;
    public GameObject nextDialogueObj;
    [Header("Text")]
    [SerializeField] string DialogueName;
    [SerializeField] string DialogueText;

    
    
    bool skipEnable;

    
    [Header("Choice options")]
    public bool isChoice;
    public int numberOfChoices;

    [Header("Options")]
    public bool doSth;
    public GameObject objectToDoSthWith;



    [SerializeField] List<DialogueOption> options;
   
    [HideInInspector] public bool isInDialogue;

    private void Start()
    {
        menager = GameObject.FindWithTag("Canvas").GetComponent<UiMenager>();
        
    }
  
    public void NextLine()
    {
        skipEnable = true;
        UpdateText();

        if(doSth)
        {
            
            DoStuff();
        }
    }

    void DoStuff()
    {
        if(objectToDoSthWith.TryGetComponent<IUnlockable>(out IUnlockable unlockable))
        {
            unlockable.Unlock();
            
        }
    }
    void UpdateText()//updates text on ui
    {
        menager.dialogueName.text = DialogueName;
        if (isChoice)
        {
            menager.dialogueText.gameObject.SetActive(false);
            menager.dialogueChoicePanel.gameObject.SetActive(true);

            int i = 0;

            foreach (Transform button in menager.dialogueChoicePanel.transform)
            {
                
                if (numberOfChoices - 1 >= i)
                {

                    button.gameObject.SetActive(true);
                    button.gameObject.GetComponent<Button>().onClick.AddListener(options[i].NextLine);
                    button.GetChild(0).GetComponent<TextMeshProUGUI>().text = options[i].optionText;
                }
                else
                {
                    
                    button.gameObject.SetActive(false);
                }
                i++;
            }
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
       
        if (nextDialogueObj)
        {
            //Debug.Log("Skibidi");
            if (nextDialogueObj.TryGetComponent(out IDialogue nextOne))
            {
                nextOne.NextLine();
                skipEnable = false;
            }
            else
            {
                DialogueExit();
                skipEnable = false;
            }
        }
        else
        {
            Debug.Log("Not Skibidi");
            DialogueExit();
            skipEnable = false;
        }
        


    }

    void Update()
    {
        if (Input.GetButtonDown("DialogueSkip")  && skipEnable && isChoice == false )
        {
            Skip();
        }
    }

    void DialogueExit()
    {
        menager.Dialogue(false);

    }
}


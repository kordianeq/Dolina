using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueElement : MonoBehaviour, IDialogue
{
    UiMenager menager;
    
    private DialogueInteraction dialogueParent;
    public GameObject nextDialogueObj;
    [Header("Dialogue options")]
    [SerializeField] string DialogueText;
    [SerializeField] bool isPlayerTalking = false;
     

    [Header("Additional Options (optional, change if 3rdparty)")]
    [SerializeField] bool is3rdParty = false;
    [SerializeField] string DialogueName;
    [SerializeField] Sprite image;
    
    bool skipEnable;

    public bool doSth;
    public UnityEvent OnDialogueDoSth;

    [Header("Choice options")]
    public bool isChoice;
    public int numberOfChoices;

    [Header("Options")]
    

    [SerializeField] List<DialogueOption> options;
   
    [HideInInspector] public bool isInDialogue;

    private void Start()
    {
        menager = UiMenager.Instance;
    }
    void Awake()
    {
        if (menager == null)
        {
            menager = UiMenager.Instance;
        }
        
        if(dialogueParent == null)
        {
            dialogueParent = GetComponentInParent<DialogueInteraction>();
        }

        SetDefaultValuesForDialogue();
    }
  
    public void NextLine()
    {
        skipEnable = true;
        UpdateText();
        UpdateImage();
        if(doSth)
        {
            
            DoStuff();
        }
    }

    void SetDefaultValuesForDialogue()
    {
        //Assign default values from parent unless it isnt overriden in script
        if (dialogueParent != null)
        {
            if(isPlayerTalking)
            {
                DialogueName = "Ty";

                return;  
            } 
            if(DialogueName == null) DialogueName = dialogueParent.NpcName;
            if(image == null) image = dialogueParent.NpcImage;

        }
        else
        {
            Debug.LogWarning("NoDialogueParrent");
        }
    }
    void DoStuff()
    {
        OnDialogueDoSth.Invoke();
    }
    void UpdateImage()
    {
        
        if (menager == null) return;

        if(image == null)
        {
            menager.dialogueImage.gameObject.SetActive(false);
            return;
        }
        else
        {
            menager.dialogueImage.gameObject.SetActive(true);
        }
        menager.dialogueImage.sprite = image;

    }
    void UpdateText()//updates text on ui
    {
        if(DialogueName == null || DialogueName == "") DialogueName = dialogueParent.NpcName;

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


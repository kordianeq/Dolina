using UnityEngine;

public class QuestManager : MonoBehaviour
{
    
    NewQuest activeQuest;


    private void Start()
    {
        if (activeQuest != null) SetQuest(activeQuest);
    }
    private void Update()
    {
        
    }

    void SetQuest(NewQuest newQuest)
    {
        activeQuest = newQuest;
    }
}

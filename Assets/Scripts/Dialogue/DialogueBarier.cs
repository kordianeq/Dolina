using UnityEngine;
using UnityEngine.Events;

public class DialogueBarier : MonoBehaviour
{
    public UnityEvent OnEnterDialogueBarier;
     
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            
            OnEnterDialogueBarier.Invoke();

            //Mechanika powrotu do npca
        }
    }

}

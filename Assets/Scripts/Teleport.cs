using UnityEngine;

public class Teleport : MonoBehaviour, IInteracted
{

    UiMenager menager;
    public bool trigger, loadingScreen;
    public int sceneId;


    void Start()
    {

        menager = GameObject.FindWithTag("Canvas").GetComponent<UiMenager>();
    }

    public void NewInteraction()
    {
        if (trigger == false)
        {
            menager.OnChangeScene(sceneId);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (trigger)
            {
                if (loadingScreen)
                {
                    menager.ChangeSceneWithLoadingScreen(sceneId);
                    Debug.Log("Triggered");
                }
                else
                {
                    menager.OnChangeScene(sceneId);
                }

            }
        }
    }
}

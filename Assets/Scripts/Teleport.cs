using UnityEngine;

public class Teleport : MonoBehaviour, IInteracted
{
    public GameObject canvas;
    UiMenager menager;
    public bool trigger, loadingScreen;
    public int sceneId;


    void Start()
    {

        menager = canvas.GetComponent<UiMenager>();
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

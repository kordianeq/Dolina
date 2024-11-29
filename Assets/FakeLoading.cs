using UnityEngine;
using UnityEngine.UI;

public class FakeLoading : MonoBehaviour
{
    sliderScript slider;
    UiMenager UiMenager;
    int SceneId;
    float current;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    public void StartLoading(int sceneId)
    {
        
        slider = GetComponent<sliderScript>();
        UiMenager = GetComponentInParent<UiMenager>();
        
        slider.max = 50;
        slider.min = 0;
        slider.value = 0;

        current = 0;
        SceneId = sceneId;
        Loading();
        
    }
    void Loading()
    {
        if (current == slider.max)
        {
            LoadingComplete(SceneId);
        }
        else
        {
            slider.value = current;
            current++;
            Debug.Log(current);
            Invoke(nameof(ResetTime), 0.1f);
        }
    }

    void ResetTime()
    {
        Loading();
    }

    void LoadingComplete(int sceneToLoad)
    {
        UiMenager.OnChangeScene(sceneToLoad);
    }
}

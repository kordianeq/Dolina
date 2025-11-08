using UnityEngine;

public class DebugController : MonoBehaviour
{
    bool showConsole;

    string input;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("Toggling debug console");
            showConsole = !showConsole;
        }
    }
    private void OnGUI()
    {
        if (!showConsole)
        {
           

            return;
        }
        float y = 0f;
        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
        
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiMenager : MonoBehaviour
{
    bool isGamePaused;
    public KeyCode pauseGame = KeyCode.Escape;
    public GameObject pausePanel;
    Scene currentScene;

      int lastScene = 0;
    

    public GameObject interactPanel;

    [Header("main panels")]
    public GameObject gameUi;
    public GameObject butelkiUi;

    // Start is called before the first frame update
    void Start()
    {
        
        currentScene = SceneManager.GetActiveScene();
        //if ( currentScene.buildIndex != 0)
        //{
        //    pausePanel.SetActive(false);
        //}
        SceneChecker(currentScene.buildIndex);
       
    }

    // Update is called once per frame
    void Update()
    {
     
        if(Input.GetKeyDown(pauseGame))
        {
            PauseGame();
        }
        if(currentScene.buildIndex == 0 )
        {
            isGamePaused = false;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void LoadLastScene()
    {
        SceneManager.LoadScene(lastScene);
    }
    public void OnChangeScene(int SceneId)
    {
        lastScene = currentScene.buildIndex;
        SceneManager.LoadScene(SceneId);
        
    }
    
    public void OnClickExit()
    {
        Application.Quit();
    }

   public void PauseGame()
    {
        isGamePaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void UnpauseGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTimeScale(float TimeScale)
    {
        Time.timeScale = TimeScale;
    }

    void SceneChecker(int level)
    {
        switch (level)
        {
            case 0:
                // code block
                break;
            case 2:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                gameUi.SetActive(false);
                butelkiUi.SetActive(true);

                break;
            default:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                gameUi.SetActive(true);
                butelkiUi.SetActive(false);
                break;
        }
    }
    private void OnLevelWasLoaded(int level)
    {
        currentScene = SceneManager.GetActiveScene();

        SceneChecker(level);

    }



}

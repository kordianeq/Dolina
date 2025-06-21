using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UiMenager : MonoBehaviour
{
    [HideInInspector] public bool isGamePaused;
    public KeyCode pauseGame = KeyCode.Escape;
    GameManager gameManager;
    
    UiExpansionPack uiExpansionPack;
    public Scene currentScene;

    int lastScene = 0;
    public TextMeshProUGUI interactText;

    
    [Header("gunSystem")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunName;

    [Header("quests")]
    public TextMeshProUGUI questName;

    [Header("dialogue")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueName;

    [Header("panels")]
    public GameObject interactPanel;
    public GameObject dialoguePanel;
    public GameObject dialogueChoicePanel;
    public GameObject scopePanel;
    public GameObject saveIcon;
    [Header("main panels")]
    public GameObject gameUi;
    public GameObject butelkiUi;
    public GameObject loadingScreen;
    public GameObject pausePanel;
    public GameObject deathPanel;

    PlayerState playerState;
    FakeLoading fakeLoading;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindWithTag("gameManager"))
        {
            gameManager = GameObject.FindWithTag("gameManager").GetComponent<GameManager>();
           
        }
        else Debug.LogWarning("GameManager not found in scene");

        uiExpansionPack = GetComponent<UiExpansionPack>();

        fakeLoading = GetComponentInChildren<FakeLoading>();
        currentScene = SceneManager.GetActiveScene();

        SceneChecker(currentScene.buildIndex);

        //Limit FPS
        //QualitySettings.vSyncCount = 0; 
        //Application.targetFrameRate = 60;
    }
    void Update()
    {

        if (Input.GetButtonDown("pauseGame"))
        {
            PauseGame();
        }
        if (currentScene.buildIndex == 0)
        {
            isGamePaused = false;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void Dialogue(bool state)
    {
        dialoguePanel.SetActive(state);

        if (state == false)
        {
            gameManager.PlayerStatus(PlayerState.Normal);
        }
        else
        {
            gameManager.PlayerStatus(PlayerState.Locked);
        }
    }
    public void DialogueEnd()
    {
        dialoguePanel.SetActive(false);
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
    public void ChangeSceneWithLoadingScreen(int SceneId)
    {
        loadingScreen.SetActive(true);
        fakeLoading = GetComponentInChildren<FakeLoading>();
        fakeLoading.StartLoading(SceneId);
    }

    public void DeathPanel()
    {
        Time.timeScale = 0;
        deathPanel.SetActive(true);
        deathPanel.GetComponent<PanelFader>().Fade();
    }

  
    public void SaveIcon()
    {
        saveIcon.GetComponent<PanelFader>().Fade();

        Invoke(nameof(HideSaveIcon), 2f);
    }

    public void HideSaveIcon()
    {
        Debug.Log("Hide Save Icon");
        saveIcon.GetComponent<PanelFader>().Fade();
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

        uiExpansionPack.spawnEnabled = true;
    }

    public void UnpauseGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
        uiExpansionPack.spawnEnabled = true;
        uiExpansionPack.BuletsCleanUp();
        if (gameManager.State == PlayerState.Locked)
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }



    }

    public void OnClickSave()
    {
        SaveSystem.Save();
    }
    public void OnClickLoad()
    {
        SaveSystem.Load();
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
        loadingScreen.SetActive(false);
        SceneChecker(level);

    }

    

}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class UiMenager : MonoBehaviour
{

    
    GameManager gameManager;


    public Scene currentScene;
    Animator animator;
    int lastScene = 0;
    public TextMeshProUGUI interactText;
    public TextMeshProUGUI throwableText;
    public damageOverlay damageOverlayScript;

    [Header("gunSystem")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunName;
    public TextMeshProUGUI totalAmmoText;

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

    //PlayerState playerState;
    FakeLoading fakeLoading;
     Slider loadingBar;

    [Header("Sliders")]
    public interactiveSlider sensitivitySlider;

    // Start is called before the first frame update
    void Awake()
    {
        // Poinformuj GameManager, ¿e oto jestem!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUi(this);
        }
        else
        {
            Debug.LogError("Nie mogê znaleŸæ GameManager.Instance!");
        }
    }

    void Start()
    {
        if (GameObject.FindWithTag("gameManager"))
        {
            gameManager = GameObject.FindWithTag("gameManager").GetComponent<GameManager>();

        }
        else Debug.LogWarning("GameManager not found in scene");

        
        if(TryGetComponent<Animator>(out Animator anim))
        {
            animator = anim;
        }
        
        fakeLoading = GetComponentInChildren<FakeLoading>();
        currentScene = SceneManager.GetActiveScene();

        SceneChecker(currentScene.buildIndex);


        //Limit FPS
        //QualitySettings.vSyncCount = 0; 
        //Application.targetFrameRate = 60;
    }


    
    void Update()
    {
        if (currentScene.buildIndex == 0)
        {

            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateThrowableCount(int count)
    {
        throwableText.text = count.ToString();
    }

    public void OptionsAnimation(bool enable)
    {
        animator.SetBool("Options", enable);
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

    public void ReloadScene()
    {
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    
    public void ChangeSceneWithLoadingScreen(int SceneId)
    {
        
        StartCoroutine(LoadSceneAsync(SceneId));
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        loadingScreen.SetActive(true);
        loadingBar = loadingScreen.GetComponentInChildren<Slider>();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progress;
            Debug.Log("Loading progress: " + (progress * 100) + "%");
            yield return null;
        }
    }
    public void DeathPanel()
    {
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

        pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PauseGame(bool ShowMenu)
    {
        if (ShowMenu)
            pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
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
        GameManager.Instance.ResumeGame();
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
        if(loadingScreen) loadingScreen.SetActive(false);
        SceneChecker(level);

    }



}

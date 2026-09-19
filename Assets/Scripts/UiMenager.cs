using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class UiMenager : MonoBehaviour
{
    public static UiMenager Instance { get; private set; }

    GameManager gameManager;


    public Scene currentScene;
    Animator animator;
    int lastScene = 0;
    public TextMeshProUGUI interactText;
    public TextMeshProUGUI throwableText;
    public TextMeshProUGUI enemyCountText;
    [Tooltip("Opcjonalny nadrzędny panel/tło licznika wrogów.")]
    public GameObject enemyCountPanel;
    public damageOverlay damageOverlayScript;

    [Header("gunSystem")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunName;
    public TextMeshProUGUI totalAmmoText;
    public UiGunChanger UiGunChanger;

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
    public GameObject shopPanel;
    public PanelFader shootVignette;

    [Header("main panels")]
    public GameObject gameUi;
    public GameObject butelkiUi;
    public GameObject loadingScreen;
    public GameObject pausePanel;
    public GameObject deathPanel;

    [Header("Ability Slots")]
    public GameObject UndyingTotemSlot;

    //PlayerState playerState;
    FakeLoading fakeLoading;
     Slider loadingBar;

    [Header("Sliders")]
    public sensitivitySlider sensitivitySlider;
    [Tooltip("Główny suwak głośności Master (kompatybilność wsteczna).")]
    public volumeSlider volSlider;
    public volumeSlider masterVolSlider;
    public volumeSlider musicVolSlider;
    public volumeSlider sfxVolSlider;
    public volumeSlider ambientVolSlider;


   #region Start and Awake
    void Awake()
    {
        Instance = this;

        // Poinformuj GameManager, e oto jestem!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterUi(this);
        }
        else
        {
            Debug.LogError("Nie mog znale GameManager.Instance!");
        }

      
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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
        SettingsSystem.Load();
        ApplySettings();

        ClearEnemyCount();
    }

#endregion
    
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

    public void UpdateEnemyCount(int remaining, int total)
    {
        CancelInvoke(nameof(ClearEnemyCount));

        if (enemyCountPanel != null && !enemyCountPanel.activeSelf)
        {
            enemyCountPanel.SetActive(true);
        }

        if (enemyCountText != null)
        {
            if (!enemyCountText.gameObject.activeSelf)
            {
                enemyCountText.gameObject.SetActive(true);
            }

            enemyCountText.text = $"{remaining}/{total}";
        }
    }

    public void ClearEnemyCount()
    {
        CancelInvoke(nameof(ClearEnemyCount));

        if (enemyCountText != null)
        {
            enemyCountText.text = "";
        }

        if (enemyCountPanel != null)
        {
            enemyCountPanel.SetActive(false);
        }
    }

    public void ClearEnemyCountDelayed(float delay)
    {
        CancelInvoke(nameof(ClearEnemyCount));
        if (delay <= 0f)
        {
            ClearEnemyCount();
        }
        else
        {
            Invoke(nameof(ClearEnemyCount), delay);
        }
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    
    public void ChangeSceneWithLoadingScreen(int SceneId)
    {
        Time.timeScale = 1f;
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
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);

            // Wyłączamy PanelFader, który zerował alpha do 0
            if (deathPanel.TryGetComponent<PanelFader>(out var fader))
            {
                fader.enabled = false;
            }

            if (deathPanel.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.alpha = 1f;
                // Wyłączamy interakcję na ułamek sekundy, aby przytrzymana spacja / LPM z walki nie aktywowały od razu restartu
                cg.interactable = false;
                cg.blocksRaycasts = true;
                StartCoroutine(EnableDeathPanelInteraction(cg));
            }

            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator EnableDeathPanelInteraction(CanvasGroup cg)
    {
        yield return new WaitForSecondsRealtime(0.4f);
        if (cg != null)
        {
            cg.interactable = true;
        }
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
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
            pausePanel.SetActive(false);
        }
        
    }
    public void ResumeGameButton()
    {
        gameManager.ResumeGame();
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

    #region SaveSettings

    public void SaveCurrentSettings()
    {
        volumeSlider master = masterVolSlider != null ? masterVolSlider : volSlider;
        if (master != null) SettingsSystem.currentSettings.masterVolume = master.localVolume;
        if (musicVolSlider != null) SettingsSystem.currentSettings.musicVolume = musicVolSlider.localVolume;
        if (sfxVolSlider != null) SettingsSystem.currentSettings.sfxVolume = sfxVolSlider.localVolume;
        if (ambientVolSlider != null) SettingsSystem.currentSettings.ambientVolume = ambientVolSlider.localVolume;

        if (sensitivitySlider != null) SettingsSystem.currentSettings.mouseSensitivity = sensitivitySlider.localSensitivity;

        // Wywołujemy zapis do JSON
        SettingsSystem.Save();

        // Aplikujemy zmiany od razu
        ApplySettings();
    }

    // Wprowadzanie ustawień w życie
    private void ApplySettings()
    {
        volumeSlider master = masterVolSlider != null ? masterVolSlider : volSlider;
        if (master != null) master.SetSliderValue(SettingsSystem.currentSettings.masterVolume);
        if (musicVolSlider != null) musicVolSlider.SetSliderValue(SettingsSystem.currentSettings.musicVolume);
        if (sfxVolSlider != null) sfxVolSlider.SetSliderValue(SettingsSystem.currentSettings.sfxVolume);
        if (ambientVolSlider != null) ambientVolSlider.SetSliderValue(SettingsSystem.currentSettings.ambientVolume);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ApplyAllVolumes(
                SettingsSystem.currentSettings.masterVolume,
                SettingsSystem.currentSettings.musicVolume,
                SettingsSystem.currentSettings.sfxVolume,
                SettingsSystem.currentSettings.ambientVolume
            );
        }
        else
        {
            AudioListener.volume = SettingsSystem.currentSettings.masterVolume;
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetSliderValue(SettingsSystem.currentSettings.mouseSensitivity);
        }

        Debug.Log("Loaded settings");
    }
    #endregion

}
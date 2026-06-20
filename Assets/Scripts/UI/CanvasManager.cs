using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    private const string SensitivityKey = "MouseSensitivity";

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Settings Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Mouse Sensitivity")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private float minSensitivity = 40f;
    [SerializeField] private float maxSensitivity = 240f;
    [SerializeField] private float defaultSensitivity = 120f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        IsPaused = false;
    }

    private void Start()
    {
        if (mouseLook == null)
            mouseLook = FindFirstObjectByType<MouseLook>();

        BindSliders();
        LoadSavedSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(paused);

        AudioManager.Instance?.SetSfxPaused(paused);

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void BindSliders()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
    }

    private void LoadSavedSettings()
    {
        float music = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1f;
        float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1f;
        float sensitivity01 = PlayerPrefs.GetFloat(SensitivityKey, SensitivityToSlider(defaultSensitivity));

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(music);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfx);

        if (sensitivitySlider != null)
            sensitivitySlider.SetValueWithoutNotify(sensitivity01);

        ApplySensitivity(sensitivity01);
        AudioManager.Instance?.SetSfxVolume(sfx);
    }

    private void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    private void OnSensitivitySliderChanged(float value01)
    {
        ApplySensitivity(value01);
        PlayerPrefs.SetFloat(SensitivityKey, value01);
    }

    private void ApplySensitivity(float value01)
    {
        if (mouseLook == null)
            return;

        float sensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, Mathf.Clamp01(value01));
        mouseLook.SetSensitivity(sensitivity);
    }

    private float SensitivityToSlider(float sensitivity)
    {
        return Mathf.InverseLerp(minSensitivity, maxSensitivity, sensitivity);
    }
}

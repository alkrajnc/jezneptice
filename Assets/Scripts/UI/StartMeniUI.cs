using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

#pragma warning disable CS0618

public class StartMenuUI : MonoBehaviour
{

    private GameObject menuPanel;
    private GameObject mockPanel;
    private Font font;
    private GameObject settingsPanel;
    private GameObject accountPanel;
    private GameObject levelPanel;
    private GameObject mainMenuContent;
    private const string KEY_VOLUME = "VOLUME";
    private const string KEY_NAME = "PLAYER_NAME";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<StartMenuUI>() != null) return;

        var go = new GameObject("StartMenuUI");
        DontDestroyOnLoad(go);
        go.AddComponent<StartMenuUI>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void Start()
    {
        MainMenu();
    }

    private void GotoMenu(Menu menu)
    {
        mainMenuContent?.SetActive(false);
        switch (menu)
        {
            case Menu.MainMenu:
            case Menu.Settings:
                { OpenSettings(); break; }
            case Menu.Account:
                { OpenAccount(); break; }
            case Menu.LevelSelect:
                { OpenLevelSelect(); break; }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MainMenu();
    }

    private void OpenSettings()
    {

        if (settingsPanel != null) Destroy(settingsPanel);

        settingsPanel = MakePanel(menuPanel, "SettingsPanel", new Color(0f, 0f, 0f, 0.75f));

        MakeText(settingsPanel, "Title", "Settings", 52, Color.white,
            new Vector2(0, 160), new Vector2(600, 80));

        CreateVolumeSlider();

        MakeText(settingsPanel, "VolumeLabel", "Volume", 28, Color.white,
            new Vector2(0, 90), new Vector2(300, 40));

        var backBtn = MakeButton(settingsPanel, "BackBtn", "Back",
            new Vector2(0, -140), new Vector2(260, 60), Color.yellow);

        backBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ReturnToMainMenu(settingsPanel);
        });
    }

    private void ReturnToMainMenu(GameObject from)
    {
        Destroy(from);
        from = null;
        mainMenuContent.SetActive(true);
        MainMenu();
    }

    private void CreateVolumeSlider()
    {
        var sliderGO = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Image), typeof(Slider));
        sliderGO.transform.SetParent(settingsPanel.transform, false);

        var rect = sliderGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 20);           // ← tanek slider (višina 20)
        rect.anchoredPosition = new Vector2(0, 40);

        var slider = sliderGO.GetComponent<Slider>();
        slider.interactable = true;

        // Background (track)
        var bgImage = sliderGO.GetComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);   // temno siva
        bgImage.type = Image.Type.Sliced; // če imaš sliced sprite, še lepše

        // Fill Area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1, 0.5f);
        fillAreaRect.sizeDelta = new Vector2(0, 10);

        var fillImageGO = new GameObject("Fill", typeof(Image));
        fillImageGO.transform.SetParent(fillArea.transform, false);
        var fillRect = fillImageGO.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;

        var fillImage = fillImageGO.GetComponent<Image>();
        fillImage.color = Color.yellow;

        // Handle
        var handleGO = new GameObject("Handle", typeof(Image));
        handleGO.transform.SetParent(sliderGO.transform, false);
        var handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(25, 35);        // velikost "ročice"

        var handleImage = handleGO.GetComponent<Image>();
        handleImage.color = Color.white;

        // Povezave (OBVEZNO!)
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = PlayerPrefs.GetFloat("Volume", 1f);

        // Posodobi glasnost takoj
        AudioListener.volume = slider.value;

        // Listener
        slider.onValueChanged.AddListener(value =>
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("Volume", value);
        });
    }
    private void OpenAccount()
    {

        if (accountPanel != null) Destroy(accountPanel);

        accountPanel = MakePanel(menuPanel, "AccountPanel", new Color(0f, 0f, 0f, 0.75f));

        MakeText(accountPanel, "Title", "ACCOUNT", 52, Color.white,
            new Vector2(0, 160), new Vector2(600, 80));

        // ===== INPUT FIELD =====
        var inputGO = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGO.transform.SetParent(accountPanel.transform, false);

        var input = inputGO.GetComponent<InputField>();

        var bg = inputGO.GetComponent<Image>();
        bg.color = Color.white;

        var rt = inputGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 60);
        rt.anchoredPosition = new Vector2(0, 40);

        // TEXT
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform, false);

        var text = textGO.AddComponent<Text>();
        text.font = font;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleLeft;

        input.textComponent = text;

        // PLACEHOLDER
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputGO.transform, false);

        var placeholder = placeholderGO.AddComponent<Text>();
        placeholder.font = font;
        placeholder.text = "Vnesi ime...";
        placeholder.color = Color.gray;

        input.placeholder = placeholder;

        // LOAD SAVE
        input.text = PlayerPrefs.GetString(KEY_NAME, "Player1");

        // SAVE on change (ne Enter-only!)
        input.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetString(KEY_NAME, v);
        });

        // SHOW NAME
        MakeText(accountPanel, "CurrentName",
            "IME: " + input.text, 28, Color.white,
            new Vector2(0, 110), new Vector2(500, 40));

        // BACK
        var backBtn = MakeButton(accountPanel, "BackBtn", "NAZAJ",
            new Vector2(0, -140), new Vector2(260, 60), Color.yellow);

        backBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ReturnToMainMenu(accountPanel);
        });
    }
    private void MainMenu()
    {
        if (menuPanel != null) Destroy(menuPanel.transform.root.gameObject);
        settingsPanel = null;
        accountPanel = null;
        levelPanel = null;
        mainMenuContent = null;
        mockPanel = null;

        Time.timeScale = 0f;

        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        var canvasGO = new GameObject("StartMenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        menuPanel = MakePanel(canvasGO, "StartPanel", Color.clear);

        var backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(menuPanel.transform, false);
        var bdImg = backdrop.AddComponent<Image>();
        var bdRT = backdrop.GetComponent<RectTransform>();
        bdRT.anchorMin = Vector2.zero;
        bdRT.anchorMax = Vector2.one;
        bdRT.offsetMin = Vector2.zero;
        bdRT.offsetMax = Vector2.zero;

        var sprite = LoadSprite("UI/menu");
        if (sprite != null)
        {
            bdImg.sprite = sprite;
            bdImg.type = Image.Type.Simple;
            bdImg.preserveAspect = false;
        }
        else
        {
            bdImg.color = new Color(0.12f, 0.18f, 0.28f, 0.96f);
        }

        // Darken overlay so UI text stays readable
        var overlay = new GameObject("DarkOverlay");
        overlay.transform.SetParent(menuPanel.transform, false);
        var ovImg = overlay.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.35f);
        var ovRT = overlay.GetComponent<RectTransform>();
        ovRT.anchorMin = Vector2.zero;
        ovRT.anchorMax = Vector2.one;
        ovRT.offsetMin = Vector2.zero;
        ovRT.offsetMax = Vector2.zero;

        mainMenuContent = new GameObject("MainMenuContent");
        mainMenuContent.transform.SetParent(menuPanel.transform, false);
        var contentRT = mainMenuContent.AddComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        MakeText(mainMenuContent, "Title", "Jezne Ptice", 82, Color.white, new Vector2(0, 230), new Vector2(700, 100));
        MakeText(mainMenuContent, "Subtitle", "Poceni Angry Birds. ", 28, new Color(1f, 0.85f, 0.25f), new Vector2(0, 150), new Vector2(850, 60));

        var playBtn = MakeButton(mainMenuContent, "PlayButton", "Play", new Vector2(0, 40), new Vector2(360, 70), new Color(1f, 0.78f, 0.12f));
        playBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GotoMenu(Menu.LevelSelect);
        });

        var settingsBtn = MakeButton(mainMenuContent, "SettingsButton", "Settings", new Vector2(0, -55), new Vector2(360, 65), new Color(0.8f, 0.8f, 0.8f));
        settingsBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GotoMenu(Menu.Settings);
        });
        var accountBtn = MakeButton(mainMenuContent, "AccountButton", "Account", new Vector2(0, -145), new Vector2(360, 65), new Color(0.8f, 0.8f, 0.8f));
        accountBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            GotoMenu(Menu.Account);
        });
    }
    private void OpenLevelSelect()
    {
        if (levelPanel != null) Destroy(levelPanel);
        if (settingsPanel != null) Destroy(settingsPanel);
        if (accountPanel != null) Destroy(accountPanel);
        if (mockPanel != null) Destroy(mockPanel);

        levelPanel = MakePanel(menuPanel, "LevelSelectPanel", new Color(0f, 0f, 0f, 0.76f));

        MakeText(levelPanel, "Title", "IZBERI LEVEL", 52, Color.white,
            new Vector2(0, 210), new Vector2(700, 80));

        MakeText(levelPanel, "Subtitle", "Pot naprej se odpira po vrsti.", 26,
            new Color(1f, 0.85f, 0.25f), new Vector2(0, 150), new Vector2(760, 50));

        var level1Btn = MakeButton(levelPanel, "Level1Button", "LEVEL 1",
            new Vector2(0, 55), new Vector2(320, 68), new Color(1f, 0.78f, 0.12f));
        level1Btn.GetComponent<Button>().onClick.AddListener(() => StartLevel(1));

        var level2Btn = MakeButton(levelPanel, "Level2Button", "LEVEL 2",
            new Vector2(0, -35), new Vector2(320, 68), new Color(1f, 0.72f, 0.1f));
        level2Btn.GetComponent<Button>().onClick.AddListener(() => StartLevel(2));

        MakeText(levelPanel, "NextLevelHint2", "LEVEL 3", 28,
            new Color(0.38f, 0.38f, 0.38f), new Vector2(0, -115), new Vector2(320, 58));

        var backBtn = MakeButton(levelPanel, "BackButton", "NAZAJ",
            new Vector2(0, -220), new Vector2(260, 60), new Color(0.8f, 0.8f, 0.8f));
        backBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ReturnToMainMenu(menuPanel);
        });
    }
    private void StartGame()
    {
        StartLevel(1);
    }
    private void StartLevel(int levelIndex)
    {
        PlayerPrefs.SetInt("SELECTED_LEVEL", levelIndex);
        Debug.Log($"[StartMenuUI] Izbran level: {levelIndex}");

        LevelManager.Instance?.StartSelectedLevel(levelIndex);

        GameManager.Instance?.StartGame();

        Time.timeScale = 1f;

        if (menuPanel != null)
            Destroy(menuPanel.transform.root.gameObject);
    }
    private void ShowMock(string title, string message)
    {
        if (mockPanel != null) Destroy(mockPanel);

        mockPanel = MakePanel(menuPanel, "MockPanel", new Color(0f, 0f, 0f, 0.72f));

        MakeText(mockPanel, "MockTitle", title, 52, Color.white, new Vector2(0, 120), new Vector2(600, 80));
        MakeText(mockPanel, "MockText", message, 28, new Color(1f, 0.9f, 0.45f), new Vector2(0, 20), new Vector2(760, 120));

        var backBtn = MakeButton(mockPanel, "BackButton", "NAZAJ", new Vector2(0, -130), new Vector2(260, 60), new Color(1f, 0.78f, 0.12f));
        backBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(mockPanel));
    }
    private GameObject MakePanel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var img = go.AddComponent<Image>();
        img.color = color;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }
    private Text MakeText(GameObject parent, string name, string value, int size, Color color, Vector2 position, Vector2 sizeDelta, FontStyle fontStyle = FontStyle.Normal)
    {
        var textComponent = new GameObject(name);
        textComponent.transform.SetParent(parent.transform, false);

        var text = textComponent.AddComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        var rt = textComponent.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = sizeDelta;

        return text;
    }
    private GameObject MakeButton(GameObject parent, string name, string label, Vector2 position, Vector2 sizeDelta, Color color)
    {
        var buttonComponent = new GameObject(name);
        buttonComponent.transform.SetParent(parent.transform, false);

        var img = buttonComponent.AddComponent<Image>();
        img.color = color;

        img.sprite = CreateRoundedSprite();
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;

        var button = buttonComponent.AddComponent<Button>();

        var rt = buttonComponent.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = sizeDelta;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(buttonComponent.transform, false);

        var text = labelGO.AddComponent<Text>();
        text.text = label;
        text.font = font;
        text.fontSize = 30;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;

        var textRT = labelGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        button.transition = Selectable.Transition.None;

        var hover = buttonComponent.AddComponent<ButtonHoverEffect>();
        hover.baseColor = color;
        hover.baseSize = sizeDelta;
        hover.image = img;


        return buttonComponent;
    }

    private Sprite CreateRoundedSprite()
    {
        int size = 64;
        int radius = 12;
        var tex = new Texture2D(size, size);
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = IsInsideRounded(x, y, size, radius)
                    ? Color.white : Color.clear;

        tex.SetPixels32(pixels);
        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
    }

    private bool IsInsideRounded(int x, int y, int size, int r)
    {
        int x0 = x < r ? r : (x >= size - r ? size - 1 - r : x);
        int y0 = y < r ? r : (y >= size - r ? size - 1 - r : y);
        float dx = x - x0, dy = y - y0;
        return dx * dx + dy * dy <= r * r;
    }

    private Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

}

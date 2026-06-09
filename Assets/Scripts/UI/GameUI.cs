using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Score")]
    public Text scoreText;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public Text winScoreText;
    public Text loseScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideInGameScoreText();
        ConfigureResultText(winScoreText);
        ConfigureResultText(loseScoreText);
        ConfigureResultPanel(winPanel, true);
        ConfigureResultPanel(losePanel, false);
    }

    public void ShowWin(int score)
    {
        ShowWin(score, GameManager.Instance != null ? GameManager.Instance.LastStarRating : 0);
    }

    public void ShowWin(int score, int stars)
    {
        if (winScoreText != null)
            winScoreText.text = BuildResultText(score, stars);

        ConfigureResultPanel(winPanel, true);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            winPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowLose(int score)
    {
        int stars = GameManager.Instance != null ? GameManager.Instance.LastStarRating : 0;
        ShowLose(score, stars);
    }

    public void ShowLose(int score, int stars)
    {
        if (loseScoreText != null)
            loseScoreText.text = BuildResultText(score, stars);

        ConfigureResultPanel(losePanel, false);

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            losePanel.transform.SetAsLastSibling();
        }
    }

    public void RestartLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("SELECTED_LEVEL", 1);
        PlayerPrefs.SetInt("SELECTED_LEVEL", Mathf.Max(1, currentLevel));
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        PlayerPrefs.SetInt("SELECTED_LEVEL", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("SELECTED_LEVEL", 1);
        int nextLevel = currentLevel + 1;

        if (!LevelExists(nextLevel))
            return;

        PlayerPrefs.SetInt("SELECTED_LEVEL", nextLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HideInGameScoreText()
    {
        if (scoreText == null)
        {
            foreach (var text in GetComponentsInChildren<Text>(true))
            {
                if (text.name == "ScoreText")
                {
                    scoreText = text;
                    break;
                }
            }
        }

        scoreText?.gameObject.SetActive(false);
    }

    private void ConfigureResultText(Text text)
    {
        if (text == null) return;

        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 34;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.lineSpacing = 1.25f;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 5f);
        rect.sizeDelta = new Vector2(520f, 120f);
    }

    private void ConfigureResultPanel(GameObject panel, bool showNextButton)
    {
        if (panel == null) return;

        Button restartButton = FindOrCreateButton(panel, "RestartBtn", "PONOVI NIVO");
        Button homeButton = FindOrCreateButton(panel, "HomeBtn", "DOMOV");
        Button nextButton = FindOrCreateButton(panel, "NextLevelBtn", "NASLEDNJI NIVO");

        ConfigureResultButton(homeButton, "DOMOV", new Vector2(-255f, -135f), new Vector2(190f, 52f), new Color(0.82f, 0.82f, 0.82f), GoHome, true);
        ConfigureResultButton(restartButton, "PONOVI NIVO", new Vector2(0f, -135f), new Vector2(210f, 52f), new Color(1f, 0.78f, 0.02f), RestartLevel, true);

        bool nextAvailable = showNextButton && LevelExists(PlayerPrefs.GetInt("SELECTED_LEVEL", 1) + 1);
        ConfigureResultButton(nextButton, "NASLEDNJI NIVO", new Vector2(270f, -135f), new Vector2(235f, 52f), new Color(0.25f, 0.68f, 0.35f), NextLevel, nextAvailable);
        nextButton.gameObject.SetActive(showNextButton);
    }

    private Button FindOrCreateButton(GameObject panel, string name, string label)
    {
        Transform existing = panel.transform.Find(name);
        if (existing != null && existing.TryGetComponent(out Button existingButton))
            return existingButton;

        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);

        var text = labelObject.GetComponent<Text>();
        text.text = label;
        text.font = GetUiFont();
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;

        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return buttonObject.GetComponent<Button>();
    }

    private void ConfigureResultButton(Button button, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction action, bool interactable)
    {
        if (button == null) return;

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = interactable ? color : new Color(0.45f, 0.45f, 0.45f, 1f);

        button.interactable = interactable;
        button.onClick.RemoveAllListeners();
        if (interactable)
            button.onClick.AddListener(action);

        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        foreach (var buttonLabel in button.GetComponentsInChildren<Text>(true))
        {
            buttonLabel.text = label;
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.fontSize = 22;
            buttonLabel.color = interactable ? Color.black : new Color(0.18f, 0.18f, 0.18f, 1f);
            buttonLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            buttonLabel.verticalOverflow = VerticalWrapMode.Overflow;

            var labelRect = buttonLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }
    }

    private bool LevelExists(int level)
    {
        if (level <= 1)
            return true;

        return Resources.Load<TextAsset>($"Levels/level_{level}") != null;
    }

    private Font GetUiFont()
    {
        if (winScoreText != null && winScoreText.font != null)
            return winScoreText.font;

        if (loseScoreText != null && loseScoreText.font != null)
            return loseScoreText.font;

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private string BuildResultText(int score, int stars)
    {
        return $"{BuildStarsText(stars)}\nScore: {score}";
    }

    private string BuildStarsText(int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        string result = "";

        for (int i = 0; i < 3; i++)
        {
            result += i < stars
                ? "<color=#FFD21A>\u2605</color>"
                : "<color=#9A9A9A>\u2605</color>";
        }

        return result;
    }
}

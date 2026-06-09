using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#pragma warning disable CS0618

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
        ConfigurePanelButton(winPanel);
        ConfigurePanelButton(losePanel);

        foreach (var btn in GetComponentsInChildren<Button>(true))
            btn.onClick.AddListener(RestartLevel);
    }

    public void ShowWin(int score)
    {
        ShowWin(score, GameManager.Instance != null ? GameManager.Instance.LastStarRating : 0);
    }

    public void ShowWin(int score, int stars)
    {
        if (winScoreText != null)
            winScoreText.text = BuildResultText(score, stars);

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

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            losePanel.transform.SetAsLastSibling();
        }
    }

    public void RestartLevel()
    {
        PlayerPrefs.SetInt("SELECTED_LEVEL", 0); 
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

        if (scoreText != null)
            scoreText.gameObject.SetActive(false);
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
        rect.anchoredPosition = new Vector2(0f, -5f);
        rect.sizeDelta = new Vector2(520f, 120f);
    }

    private void ConfigurePanelButton(GameObject panel)
    {
        if (panel == null) return;

        foreach (var button in panel.GetComponentsInChildren<Button>(true))
        {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -125f);
            rect.sizeDelta = new Vector2(280f, 58f);

            foreach (var label in button.GetComponentsInChildren<Text>(true))
            {
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 24;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                label.verticalOverflow = VerticalWrapMode.Overflow;

                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
            }
        }
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

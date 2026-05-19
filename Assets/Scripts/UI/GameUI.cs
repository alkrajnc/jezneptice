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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var btn in GetComponentsInChildren<Button>(true))
            btn.onClick.AddListener(RestartLevel);
    }

    void Update()
    {
        if (scoreText != null && GameManager.Instance != null)
            scoreText.text = $"Score: {GameManager.Instance.Score}";
    }

    public void ShowWin(int score)
    {
        if (winScoreText != null) winScoreText.text = $"Score: {score}";
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void ShowLose(int score)
    {
        if (loseScoreText != null) loseScoreText.text = $"Score: {score}";
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

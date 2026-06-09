using System.Collections;
using UnityEngine;

public enum GameState
{
    Playing,
    LevelComplete,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level cilj")]
    [SerializeField] private int oneStarScore = 250;
    [SerializeField] private int twoStarScore = 350;
    [SerializeField] private int threeStarScore = 430;

    public GameState currentState;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int Score { get; private set; }
    public int ScoreToCompleteLevel => oneStarScore;
    public int LastStarRating { get; private set; }

    public void StartGame()
    {
        currentState = GameState.Playing;
        Score = 0;
        LastStarRating = 0;
        StartCoroutine(MuteAudioBriefly(1f));
        Debug.Log($"Game Started. Cilj: {oneStarScore} tock.");
    }

    public void SetScoreTarget(int scoreTarget)
    {
        SetStarTargets(scoreTarget, scoreTarget, scoreTarget);
    }

    public void SetStarTargets(int oneStar, int twoStar, int threeStar)
    {
        oneStarScore = Mathf.Max(0, oneStar);
        twoStarScore = Mathf.Max(oneStarScore, twoStar);
        threeStarScore = Mathf.Max(twoStarScore, threeStar);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        Debug.Log($"[GameManager] Score: {Score}");

        if (currentState == GameState.Playing && oneStarScore > 0 && Score >= oneStarScore)
            Debug.Log("[GameManager] Cilj dosezen. Pocakamo, da igralec porabi vse ptice.");
    }

    public void FinishLevelAfterBirdsUsed()
    {
        if (currentState != GameState.Playing) return;

        if (oneStarScore > 0 && Score >= oneStarScore)
            WinLevel();
        else
            LoseLevel();
    }

    public void WinLevel()
{
    if (currentState != GameState.Playing) return;
    currentState = GameState.LevelComplete;
    LastStarRating = CalculateStarRating(Score);
    Debug.Log($"Level Complete! Stars: {LastStarRating}");
    GameUI.Instance?.ShowWin(Score, LastStarRating);

    
    int trenutniLevel = PlayerPrefs.GetInt("SELECTED_LEVEL", 1);

    string scoreKey = $"LEVEL_{trenutniLevel}_BEST_SCORE";
    int prejsnjiBestScore = PlayerPrefs.GetInt(scoreKey, 0);
    
    if (Score > prejsnjiBestScore)
    {
        PlayerPrefs.SetInt(scoreKey, Score);
        PlayerPrefs.SetInt($"LEVEL_{trenutniLevel}_BEST_STARS", LastStarRating);
    }

    int maxOdklenjenLevel = PlayerPrefs.GetInt("MAX_UNLOCKED_LEVEL", 1);
    if (trenutniLevel == maxOdklenjenLevel)
    {
        PlayerPrefs.SetInt("MAX_UNLOCKED_LEVEL", trenutniLevel + 1);
    }

    // Shranimo spremembe na disk
    PlayerPrefs.Save();
}

    public void LoseLevel()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver;
        LastStarRating = CalculateStarRating(Score);
        Debug.Log("Game Over!");
        GameUI.Instance?.ShowLose(Score, LastStarRating);
    }

    private int CalculateStarRating(int score)
    {
        if (threeStarScore > 0 && score >= threeStarScore) return 3;
        if (twoStarScore > 0 && score >= twoStarScore) return 2;
        if (oneStarScore > 0 && score >= oneStarScore) return 1;
        return 0;
    }

    private IEnumerator MuteAudioBriefly(float duration)
    {
        float previousVolume = AudioListener.volume;
        AudioListener.volume = 0f;

        yield return new WaitForSecondsRealtime(duration);

        AudioListener.volume = previousVolume;
    }
}

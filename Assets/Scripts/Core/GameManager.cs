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

    public void StartGame()
    {
        currentState = GameState.Playing;
        Score = 0;
        Debug.Log("Game Started");
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        Debug.Log($"[GameManager] Score: {Score}");
    }

    public void WinLevel()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.LevelComplete;
        Debug.Log("Level Complete!");
        GameUI.Instance?.ShowWin(Score);
    }

    public void LoseLevel()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver;
        Debug.Log("Game Over!");
        GameUI.Instance?.ShowLose(Score);
    }
}
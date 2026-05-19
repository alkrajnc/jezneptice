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
        currentState = GameState.LevelComplete;
        Debug.Log("Level Complete!");
    }

    public void LoseLevel()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over!");
    }
}
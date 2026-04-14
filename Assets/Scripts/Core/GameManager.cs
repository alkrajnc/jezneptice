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

    public void StartGame()
    {
        currentState = GameState.Playing;
        Debug.Log("Game Started");
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
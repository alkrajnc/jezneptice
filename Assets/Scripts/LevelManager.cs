using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private List<PigController> activePigs = new List<PigController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPig(PigController pig)
    {
        if (!activePigs.Contains(pig))
            activePigs.Add(pig);
    }

    public void OnPigDestroyed(PigController pig)
    {
        activePigs.Remove(pig);

        if (activePigs.Count == 0)
            GameManager.Instance?.WinLevel();
    }
}

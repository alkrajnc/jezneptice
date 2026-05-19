using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level JSON")]
    public TextAsset levelJson;

    [Header("Prefabi")]
    public GameObject pigPrefab;
    public GameObject woodBlockPrefab;
    public GameObject stoneBlockPrefab;
    public GameObject iceBlockPrefab;

    private List<PigController> activePigs = new List<PigController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (levelJson != null)
            LoadLevel();
    }

    // ── Level loading ──────────────────────────────────────────────
    public void LoadLevel()
    {
        LevelData data = JsonUtility.FromJson<LevelData>(levelJson.text);
        Debug.Log("Loading level: " + data.levelId);

        activePigs.Clear();

        foreach (var pig in data.pigs)
        {
            var go = Instantiate(pigPrefab, new Vector2(pig.x, pig.y), Quaternion.identity);
            var pc = go.GetComponent<PigController>();
            if (pc != null) RegisterPig(pc);
        }

        foreach (var block in data.blocks)
        {
            GameObject prefab = GetBlockPrefab(block.type);
            if (prefab != null)
                Instantiate(prefab, new Vector2(block.x, block.y), Quaternion.identity);
            else
                Debug.LogWarning("Neznan tip bloka: " + block.type);
        }
    }

    private GameObject GetBlockPrefab(string type)
    {
        switch (type)
        {
            case "wood":  return woodBlockPrefab;
            case "stone": return stoneBlockPrefab;
            case "ice":   return iceBlockPrefab;
            default:      return null;
        }
    }

    // ── Pig tracking ───────────────────────────────────────────────
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

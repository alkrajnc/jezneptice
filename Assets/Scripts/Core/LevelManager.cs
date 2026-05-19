using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TextAsset levelJson;

    public GameObject pigPrefab;
    public GameObject woodBlockPrefab;
    public GameObject stoneBlockPrefab;
    public GameObject iceBlockPrefab;

    void Start()
    {
        LoadLevel();
    }

    void LoadLevel()
    {
        if (levelJson == null)
        {
            Debug.LogError("Level JSON ni nastavljen!");
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(levelJson.text);

        Debug.Log("Loading level: " + data.levelId);

        foreach (var pig in data.pigs)
        {
            Instantiate(pigPrefab, new Vector2(pig.x, pig.y), Quaternion.identity);
        }

        foreach (var block in data.blocks)
        {
            GameObject prefab = GetBlockPrefab(block.type);

            if (prefab != null)
            {
                Instantiate(prefab, new Vector2(block.x, block.y), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Neznan tip bloka: " + block.type);
            }
        }
    }

    GameObject GetBlockPrefab(string type)
    {
        switch (type)
        {
            case "wood":
                return woodBlockPrefab;
            case "stone":
                return stoneBlockPrefab;
            case "ice":
                return iceBlockPrefab;
            default:
                return null;
        }
    }
}
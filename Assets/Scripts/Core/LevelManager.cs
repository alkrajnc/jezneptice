using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private const float BlockSpriteWorldSize = 0.32f;

    [Header("Level JSON")]
    public TextAsset levelJson;

    [Header("Prefabi")]
    public GameObject pigPrefab;
    public GameObject woodBlockPrefab;
    public GameObject stoneBlockPrefab;
    public GameObject iceBlockPrefab;

    private readonly List<PigController> activePigs = new List<PigController>();

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
        else
            FindScenePigs();
    }

    public void StartSelectedLevel(int levelIndex)
    {
        BackgroundMusicController.instance?.PlayLevelMusic();
        if (levelIndex <= 1)
        {
            GameManager.Instance?.SetStarTargets(250, 350, 430);
            ConfigureBirdQueue(new[] { "red", "blue", "yellow", "red" });
            FindScenePigs();
            Debug.Log("[LevelManager] Level 1 pripravljen iz scene.");
            return;
        }

        LoadJsonLevel(levelIndex);
    }

    private void FindScenePigs()
    {
        activePigs.Clear();
        foreach (var pig in FindObjectsByType<PigController>(FindObjectsSortMode.None))
            RegisterPig(pig);
        Debug.Log($"[LevelManager] Najdeni prasici v sceni: {activePigs.Count}");
    }

    public void LoadLevel()
    {
        LevelData data = JsonUtility.FromJson<LevelData>(levelJson.text);
        ApplyLevelData(data);
    }

    private void LoadJsonLevel(int levelIndex)
    {
        TextAsset levelAsset = Resources.Load<TextAsset>($"Levels/level_{levelIndex}");
        if (levelAsset == null)
        {
            Debug.LogWarning($"[LevelManager] JSON za level {levelIndex} ne obstaja. Nalagam Level 1.");
            StartSelectedLevel(1);
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(levelAsset.text);
        ApplyLevelData(data);
    }

    private void ApplyLevelData(LevelData data)
    {
        if (data == null) return;

        GameObject pigTemplate = CreateTemplateCopy(FindPigTemplate());
        GameObject blockTemplate = CreateTemplateCopy(FindBlockTemplate());

        ClearCurrentLevelObjects();
        activePigs.Clear();

        if (data.scoreTargets != null)
        {
            GameManager.Instance?.SetStarTargets(
                data.scoreTargets.oneStar,
                data.scoreTargets.twoStars,
                data.scoreTargets.threeStars);
        }

        ConfigureBirdQueue(data.birds);

        if (data.blocks != null)
        {
            foreach (var block in data.blocks)
                CreateBlock(block, blockTemplate);
        }

        if (data.pigs != null)
        {
            foreach (var pig in data.pigs)
                CreatePig(pig, pigTemplate);
        }

        Debug.Log($"[LevelManager] Level {data.levelId} nalozen. Pujskov: {activePigs.Count}");

        if (pigTemplate != null)
            Destroy(pigTemplate);
        if (blockTemplate != null)
            Destroy(blockTemplate);
    }

    private void ConfigureBirdQueue(string[] birds)
    {
        if (birds == null || birds.Length == 0) return;

        var queue = FindFirstObjectByType<BirdQueue>();
        if (queue == null) return;

        queue.RebuildBirdSequenceFromNames(birds);
    }

    private void ClearCurrentLevelObjects()
    {
        foreach (var pig in FindObjectsByType<PigController>(FindObjectsSortMode.None))
            Destroy(pig.gameObject);

        foreach (var block in FindObjectsByType<BlockDamage>(FindObjectsSortMode.None))
            Destroy(block.gameObject);
    }

    private GameObject FindPigTemplate()
    {
        if (pigPrefab != null) return pigPrefab;
        var pig = FindFirstObjectByType<PigController>();
        return pig != null ? pig.gameObject : null;
    }

    private GameObject FindBlockTemplate()
    {
        var block = FindFirstObjectByType<BlockDamage>();
        return block != null ? block.gameObject : null;
    }

    private GameObject CreateTemplateCopy(GameObject source)
    {
        if (source == null) return null;

        GameObject copy = Instantiate(source);
        copy.name = source.name + "_Template";
        copy.SetActive(false);
        return copy;
    }

    private void CreatePig(PigData pig, GameObject template)
    {
        GameObject go;
        if (template != null)
            go = Instantiate(template, new Vector2(pig.x, pig.y), Quaternion.identity);
        else
            go = CreateFallbackPig(new Vector2(pig.x, pig.y));

        go.name = "Pig_Level";
        go.tag = "Pig";
        go.SetActive(true);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = true;

        var pigController = go.GetComponent<PigController>();
        if (pigController != null)
        {
            pigController.Configure(pig.health, pig.killScore, pig.damageScore);
            RegisterPig(pigController);
        }
    }

    private GameObject CreateBlock(BlockData block, GameObject template)
    {
        BlockMaterial material = ParseBlockMaterial(block.type);
        GameObject prefab = GetBlockPrefab(block.type);

        GameObject go;
        if (prefab != null)
            go = Instantiate(prefab);
        else if (template != null)
            go = Instantiate(template);
        else
            go = CreateFallbackBlock(material);

        go.name = $"Block_{material}";
        go.tag = "Block";
        go.transform.position = new Vector3(block.x, block.y, 0f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, block.rotation);
        go.transform.localScale = new Vector3(
            ToBlockScale(block.width),
            ToBlockScale(block.height),
            1f);
        go.SetActive(true);

        var damage = go.GetComponent<BlockDamage>();
        if (damage != null)
            damage.Configure(material, block.score, block.health);

        return go;
    }

    private float ToBlockScale(float worldSize)
    {
        return worldSize > 0f ? worldSize / BlockSpriteWorldSize : 1f;
    }

    private GameObject CreateFallbackPig(Vector2 position)
    {
        var go = new GameObject("Pig_Level");
        go.transform.position = position;
        go.tag = "Pig";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite();
        sr.color = new Color(0.2f, 0.78f, 0.18f, 1f);
        sr.sortingOrder = 5;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.32f;

        go.AddComponent<Rigidbody2D>();
        go.AddComponent<AudioSource>();
        go.AddComponent<PigController>();
        return go;
    }

    private GameObject CreateFallbackBlock(BlockMaterial material)
    {
        var go = new GameObject($"Block_{material}");
        go.tag = "Block";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSolidSprite();
        sr.sortingOrder = 4;

        go.AddComponent<BoxCollider2D>();
        go.AddComponent<Rigidbody2D>();
        go.AddComponent<AudioSource>();
        go.AddComponent<BlockDamage>();
        return go;
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private GameObject GetBlockPrefab(string type)
    {
        switch ((type ?? "").ToLowerInvariant())
        {
            case "wood": return woodBlockPrefab;
            case "stone": return stoneBlockPrefab;
            case "ice": return iceBlockPrefab;
            default: return null;
        }
    }

    private BlockMaterial ParseBlockMaterial(string type)
    {
        switch ((type ?? "").ToLowerInvariant())
        {
            case "stone": return BlockMaterial.Stone;
            case "ice": return BlockMaterial.Ice;
            case "metal": return BlockMaterial.Metal;
            default: return BlockMaterial.Wood;
        }
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
            Debug.Log("[LevelManager] Vsi pujski uniceni. Level se zakljuci, ko igralec porabi vse ptice.");
    }
}

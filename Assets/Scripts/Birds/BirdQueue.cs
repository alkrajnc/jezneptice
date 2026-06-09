using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QueuedBirdType
{
    Red,
    Blue,
    Orange
}

/// <summary>
/// Upravlja vrsto ptičev za en nivo.
/// SlingshotController pokliče GetNextBird() ko je prejšnja ptica izstreljena.
/// </summary>
public class BirdQueue : MonoBehaviour
{
    [Header("Ptiči v tem nivoju")]
    [Tooltip("Povleci Bird Prefabe sem v Inspector (po vrsti)")]
    public List<GameObject> birdPrefabs = new List<GameObject>();
    public List<QueuedBirdType> birdSequence = new List<QueuedBirdType>();

    [Header("Sprite-i ptic")]
    public Sprite redBirdSprite;
    public Sprite blueBirdSprite;
    public Sprite orangeBirdSprite;

    [Header("Osnovne nastavitve ptic")]
    public AudioClip birdLaunchSfx;
    public float birdScale = 1.4f;

    [Header("Pozicija za čakanje")]
    [Tooltip("Kje se spawna naslednja ptica (na frači)")]
    public Transform slingshotSpawnPoint;

    [Tooltip("Kje stojijo čakajoče ptice (levo od frače)")]
    public Transform queueStartPoint;
    public float queueSpacing = 1.2f;
    public float queueRowSpacing = 0.9f;
    public float queueScreenPadding = 0.6f;

    [Header("Ozadje queue-ja")]
    public Color queueBackgroundColor = new Color(0f, 0f, 0f, 0.65f);
    public Vector2 queueBackgroundPadding = new Vector2(0.7f, 0.45f);

    [Header("Testna ptica (za razvoj)")]
    [Tooltip("Povleci Bird_Red iz scene sem — za testiranje brez prefabov")]
    public GameObject testBird;

    // Notranje stanje
    private Queue<GameObject> birdQueue = new Queue<GameObject>();
    private List<GameObject> spawnedQueueBirds = new List<GameObject>();
    private GameObject currentBird;
    private SpriteRenderer queueBackgroundRenderer;

    void Start()
    {
        if (birdSequence.Count > 0)
        {
            SpawnGeneratedQueue();
            LoadNextBird();
            return;
        }

        if (testBird != null)
        {
            currentBird = testBird;
            return;
        }
        SpawnQueue();
        LoadNextBird();
    }

    // ── Javne metode ───────────────────────────────────────────────

    /// <summary>
    /// Vrne trenutno ptico na frači (null če jih ni več).
    /// </summary>
    public GameObject GetCurrentBird() => currentBird;

    /// <summary>
    /// Pokliče SlingshotController po izstrelu — naloži naslednjo ptico.
    /// </summary>
    public void OnBirdLaunched()
    {
        currentBird = null;
        AdvanceQueue();
        LoadNextBird();
        UpdateQueueBackground();

        if (currentBird == null)
            StartCoroutine(FinishLevelAfterDelay(3f));
    }

    private IEnumerator FinishLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.Instance?.currentState == GameState.Playing)
            GameManager.Instance.FinishLevelAfterBirdsUsed();
    }

    /// <summary>
    /// Koliko ptičev je še preostalih (vključno s trenutno).
    /// </summary>
    public int RemainingBirds() => birdQueue.Count + (currentBird != null ? 1 : 0);

    public void SetBirdSequence(IEnumerable<QueuedBirdType> sequence)
    {
        birdSequence.Clear();
        birdSequence.AddRange(sequence);
    }

    public void SetBirdSequenceFromNames(IEnumerable<string> sequence)
    {
        birdSequence.Clear();

        foreach (string birdName in sequence)
            birdSequence.Add(ParseBirdType(birdName));
    }

    public void RebuildBirdSequence(IEnumerable<QueuedBirdType> sequence)
    {
        ClearSpawnedBirds();
        SetBirdSequence(sequence);
        SpawnGeneratedQueue();
        LoadNextBird();
    }

    public void RebuildBirdSequenceFromNames(IEnumerable<string> sequence)
    {
        ClearSpawnedBirds();
        SetBirdSequenceFromNames(sequence);
        SpawnGeneratedQueue();
        LoadNextBird();
    }

    // ── Zasebne metode ─────────────────────────────────────────────

    /// <summary>
    /// Spawna vse ptice v vrsti (brez prve, ki gre takoj na fračo).
    /// </summary>
    private void SpawnQueue()
    {
        birdQueue.Clear();
        spawnedQueueBirds.Clear();

        for (int i = 0; i < birdPrefabs.Count; i++)
        {
            Vector3 pos = GetQueuePosition(i);
            GameObject bird = Instantiate(birdPrefabs[i], pos, Quaternion.identity);

            // Pomanjšaj ptice v vrsti
            bird.transform.localScale *= 0.7f;

            birdQueue.Enqueue(bird);
            spawnedQueueBirds.Add(bird);
        }

        UpdateQueueBackground();
    }

    private void SpawnGeneratedQueue()
    {
        birdQueue.Clear();
        spawnedQueueBirds.Clear();

        for (int i = 0; i < birdSequence.Count; i++)
        {
            GameObject bird = CreateBird(birdSequence[i], GetQueuePosition(i));
            bird.transform.localScale = Vector3.one * birdScale * 0.7f;

            birdQueue.Enqueue(bird);
            spawnedQueueBirds.Add(bird);
        }

        UpdateQueueBackground();
    }

    private void ClearSpawnedBirds()
    {
        if (currentBird != null)
            Destroy(currentBird);

        foreach (var bird in spawnedQueueBirds)
        {
            if (bird != null)
                Destroy(bird);
        }

        currentBird = null;
        birdQueue.Clear();
        spawnedQueueBirds.Clear();

        if (queueBackgroundRenderer != null)
            queueBackgroundRenderer.gameObject.SetActive(false);
    }

    private Vector3 GetQueuePosition(int index)
    {
        Vector3 start = queueStartPoint != null
            ? queueStartPoint.position
            : slingshotSpawnPoint.position + Vector3.left * queueSpacing;

        int columns = GetQueueColumnCount(start);
        int row = index / columns;
        int column = index % columns;

        return start + Vector3.left * column * queueSpacing + Vector3.down * row * queueRowSpacing;
    }

    private int GetQueueColumnCount(Vector3 start)
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic)
            return Mathf.Max(1, birdSequence.Count);

        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * camera.aspect;
        float leftEdge = camera.transform.position.x - halfWidth + queueScreenPadding;
        float availableWidth = Mathf.Max(0f, start.x - leftEdge);

        return Mathf.Max(1, Mathf.FloorToInt(availableWidth / queueSpacing) + 1);
    }

    private GameObject CreateBird(QueuedBirdType type, Vector3 position)
    {
        GameObject bird = new GameObject($"Bird_{type}");
        bird.tag = "Bird";
        bird.transform.position = position;

        SpriteRenderer spriteRenderer = bird.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetBirdSprite(type);
        spriteRenderer.sortingOrder = 5;

        CircleCollider2D collider = bird.AddComponent<CircleCollider2D>();
        collider.radius = 0.15f;

        Rigidbody2D birdRb = bird.AddComponent<Rigidbody2D>();
        birdRb.bodyType = RigidbodyType2D.Kinematic;
        birdRb.gravityScale = 1.5f;

        BirdController controller = bird.AddComponent<BirdController>();
        controller.launchSfx = birdLaunchSfx;
        controller.hasSpecialAbility = type == QueuedBirdType.Red || type == QueuedBirdType.Orange;
        controller.specialAbilityType = type == QueuedBirdType.Orange
            ? BirdSpecialAbility.SpeedBoost
            : BirdSpecialAbility.EnergyBurst;

        bird.AddComponent<AudioSource>();
        return bird;
    }

    private Sprite GetBirdSprite(QueuedBirdType type)
    {
        switch (type)
        {
            case QueuedBirdType.Blue:
                return blueBirdSprite;
            case QueuedBirdType.Orange:
                return orangeBirdSprite;
            default:
                return redBirdSprite;
        }
    }

    private void UpdateQueueBackground()
    {
        if (spawnedQueueBirds.Count == 0)
        {
            if (queueBackgroundRenderer != null)
                queueBackgroundRenderer.gameObject.SetActive(false);
            return;
        }

        EnsureQueueBackground();

        Bounds bounds = new Bounds(spawnedQueueBirds[0].transform.position, Vector3.zero);
        foreach (var bird in spawnedQueueBirds)
        {
            if (bird != null)
                bounds.Encapsulate(bird.transform.position);
        }

        Vector3 size = bounds.size;
        size.x += queueBackgroundPadding.x;
        size.y += queueBackgroundPadding.y;
        size.z = 1f;

        queueBackgroundRenderer.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0.2f);
        queueBackgroundRenderer.transform.localScale = new Vector3(Mathf.Max(size.x, 0.8f), Mathf.Max(size.y, 0.8f), 1f);
        queueBackgroundRenderer.gameObject.SetActive(true);
    }

    private void EnsureQueueBackground()
    {
        if (queueBackgroundRenderer != null)
            return;

        GameObject background = new GameObject("BirdQueueBackground");
        background.transform.SetParent(transform);

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        queueBackgroundRenderer = background.AddComponent<SpriteRenderer>();
        queueBackgroundRenderer.sprite = sprite;
        queueBackgroundRenderer.color = queueBackgroundColor;
        queueBackgroundRenderer.sortingOrder = 4;
    }

    private QueuedBirdType ParseBirdType(string birdName)
    {
        if (string.IsNullOrWhiteSpace(birdName))
            return QueuedBirdType.Red;

        string normalized = birdName.Trim().ToLowerInvariant();

        switch (normalized)
        {
            case "blue":
            case "moder":
            case "modra":
                return QueuedBirdType.Blue;
            case "orange":
            case "oranzna":
            case "oranzni":
            case "yellow":
            case "rumena":
            case "rumen":
                return QueuedBirdType.Orange;
            default:
                return QueuedBirdType.Red;
        }
    }

    /// <summary>
    /// Vzame naslednjo ptico iz vrste in jo postavi na fračo.
    /// </summary>
    private void LoadNextBird()
    {
        if (birdQueue.Count == 0)
        {
            Debug.Log("[BirdQueue] Ni več ptičev.");
            return;
        }

        currentBird = birdQueue.Dequeue();
        spawnedQueueBirds.Remove(currentBird);

        // Premakni na fračo in povrni velikost
        currentBird.transform.position   = slingshotSpawnPoint.position;
        currentBird.transform.localScale /= 0.7f;
        UpdateQueueBackground();

        Debug.Log($"[BirdQueue] Naslednja ptica naložena. Preostalo: {RemainingBirds()}");
    }

    /// <summary>
    /// Premakni čakajoče ptice en korak naprej (vizualno napredovanje vrste).
    /// </summary>
    private void AdvanceQueue()
    {
        for (int i = 0; i < spawnedQueueBirds.Count; i++)
        {
            if (spawnedQueueBirds[i] != null)
            {
                spawnedQueueBirds[i].transform.position =
                    GetQueuePosition(i);
            }
        }

        UpdateQueueBackground();
    }
}

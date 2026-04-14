using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Upravlja vrsto ptičev za en nivo.
/// SlingshotController pokliče GetNextBird() ko je prejšnja ptica izstreljena.
/// </summary>
public class BirdQueue : MonoBehaviour
{
    [Header("Ptiči v tem nivoju")]
    [Tooltip("Povleci Bird Prefabe sem v Inspector (po vrsti)")]
    public List<GameObject> birdPrefabs = new List<GameObject>();

    [Header("Pozicija za čakanje")]
    [Tooltip("Kje se spawna naslednja ptica (na frači)")]
    public Transform slingshotSpawnPoint;

    [Tooltip("Kje stojijo čakajoče ptice (levo od frače)")]
    public Transform queueStartPoint;
    public float queueSpacing = 1.2f;

    // Notranje stanje
    private Queue<GameObject> birdQueue = new Queue<GameObject>();
    private List<GameObject> spawnedQueueBirds = new List<GameObject>();
    private GameObject currentBird;

    void Start()
    {
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
    }

    /// <summary>
    /// Koliko ptičev je še preostalih (vključno s trenutno).
    /// </summary>
    public int RemainingBirds() => birdQueue.Count + (currentBird != null ? 1 : 0);

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
            Vector3 pos = queueStartPoint.position + Vector3.right * i * queueSpacing;
            GameObject bird = Instantiate(birdPrefabs[i], pos, Quaternion.identity);

            // Pomanjšaj ptice v vrsti
            bird.transform.localScale *= 0.7f;

            birdQueue.Enqueue(bird);
            spawnedQueueBirds.Add(bird);
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
                    queueStartPoint.position + Vector3.right * i * queueSpacing;
            }
        }
    }
}

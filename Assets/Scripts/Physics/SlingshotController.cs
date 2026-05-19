using UnityEngine;

/// <summary>
/// Upravlja fračo: vlečenje ptice z miško, vizualne gumice in izstrel.
///
/// SETUP V UNITY INSPECTORJU:
///   1. Ustvari prazen GameObject "Slingshot" in dodaj to skripto
///   2. Ustvari dva prazna GameObjecta kot otroke: "AnchorLeft", "AnchorRight"
///      (to sta konici frače kjer se pritrdi gumice)
///   3. Na "Slingshot" GameObjectu dodaj dva LineRenderer komponenti za gumice
///   4. Povleci BirdQueue referenco iz scene
/// </summary>
public class SlingshotController : MonoBehaviour
{
    // ── Nastavitve ─────────────────────────────────────────────────
    [Header("Točke frače")]
    [Tooltip("Leva kotica frače (kje se pritrdi gumice)")]
    public Transform anchorLeft;
    [Tooltip("Desna kotica frače")]
    public Transform anchorRight;

    [Header("Fizika izstrela")]
    [Tooltip("Maksimalna razdalja vleka od centra frače")]
    public float maxDragDistance = 2.2f;
    [Tooltip("Množilnik sile — višji = hitreje leti")]
    public float launchForceMultiplier = 8f;
    [Tooltip("Minimalna sila izstrela ne glede na pullback")]
    public float minLaunchForce = 5f;

    [Header("Gumice (LineRenderer)")]
    [Tooltip("LineRenderer za levo gumo (2 točki: anchor → ptica)")]
    public LineRenderer leftBand;
    [Tooltip("LineRenderer za desno gumo (2 točki: anchor → ptica)")]
    public LineRenderer rightBand;

    [Header("Reference")]
    public BirdQueue birdQueue;
    public Camera mainCamera;

    // ── Notranje stanje ────────────────────────────────────────────
    private TrajectoryPreview trajectory;
    private bool isDragging = false;
    private Vector2 dragPosition;                // trenutna pozicija vleka
    private Vector2 slingshotCenter;             // center frače (med ankorama)

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        slingshotCenter = (anchorLeft.position + anchorRight.position) / 2f;
        trajectory = GetComponent<TrajectoryPreview>();

        HideBands();
    }

    void Update()
    {
        GameObject currentBird = birdQueue?.GetCurrentBird();
        if (currentBird == null) return;

        Vector2 mouseWorld = ScreenToWorld(Input.mousePosition);

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            Collider2D col = currentBird.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(mouseWorld))
            {
                isDragging = true;
                ShowBands();
            }
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                DragBird(currentBird);
                UpdateBandVisuals(currentBird.transform.position);

                if (trajectory != null)
                {
                    Vector2 launchDir   = slingshotCenter - dragPosition;
                    Vector2 launchForce = launchDir * launchForceMultiplier;
                    if (launchForce.magnitude < minLaunchForce)
                        launchForce = launchForce.normalized * minLaunchForce;
                    float mass = currentBird.GetComponent<Rigidbody2D>()?.mass ?? 1f;
                    trajectory.Show();
                    trajectory.UpdatePreview(dragPosition, launchForce, mass);
                }
            }
            else
            {
                isDragging = false;
                trajectory?.Hide();
                LaunchBird(currentBird);
            }
        }
    }

    // ── Vlečenje ───────────────────────────────────────────────────

    private void DragBird(GameObject bird)
    {
        Vector2 mouseWorld = ScreenToWorld(Input.mousePosition);
        // Izračunaj offset od centra frače
        Vector2 offset    = mouseWorld - slingshotCenter;

        // Omeji razdaljo vleka na maxDragDistance
        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        // Ptica sme biti samo LEVO od centra (vlečemo nazaj, ne naprej)
        if (offset.x > 0f) offset.x = 0f;

        dragPosition          = slingshotCenter + offset;
        bird.transform.position = dragPosition;
    }

    // ── Izstrel ────────────────────────────────────────────────────

    private void LaunchBird(GameObject bird)
    {
        // Smer izstrela je nasprotna od vleka
        Vector2 launchDirection = slingshotCenter - dragPosition;
        Vector2 launchForce     = launchDirection * launchForceMultiplier;
        if (launchForce.magnitude < minLaunchForce)
            launchForce = launchForce.normalized * minLaunchForce;

        // Pokliči Launch na BirdController
        BirdController bc = bird.GetComponent<BirdController>();
        if (bc != null)
        {
            bc.Launch(launchForce);
        }
        else
        {
            Debug.LogWarning("[SlingshotController] BirdController ni najden na ptici!");
        }

        HideBands();
        birdQueue.OnBirdLaunched();

        Debug.Log($"[SlingshotController] Ptica izstreljena! Sila: {launchForce}, Kot: {Vector2.Angle(Vector2.right, launchDirection):F1}°");
    }

    // ── Vizualne gumice ────────────────────────────────────────────

    private void UpdateBandVisuals(Vector3 birdPos)
    {
        // Leva gumice: anchorLeft → ptica
        leftBand.SetPosition(0, anchorLeft.position);
        leftBand.SetPosition(1, birdPos);

        // Desna gumice: anchorRight → ptica
        rightBand.SetPosition(0, anchorRight.position);
        rightBand.SetPosition(1, birdPos);
    }

    private void ShowBands()
    {
        leftBand.enabled  = true;
        rightBand.enabled = true;
    }

    private void HideBands()
    {
        leftBand.enabled  = false;
        rightBand.enabled = false;
    }

    // ── Pomožne metode ─────────────────────────────────────────────

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z));
        return mainCamera.ScreenToWorldPoint(pos);
    }

    // ── Gizmos (vidno samo v Scene oknu med razvojem) ──────────────
    void OnDrawGizmosSelected()
    {
        if (anchorLeft == null || anchorRight == null) return;

        // Prikaži center frače
        Vector3 center = (anchorLeft.position + anchorRight.position) / 2f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, 0.1f);

        // Prikaži maksimalni krog vleka
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, maxDragDistance);
    }
}

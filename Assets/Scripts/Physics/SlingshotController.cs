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
///   5. (Opcijsko) Dodaj TrajectoryPreview na isti GameObject
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

    [Header("Gumice (LineRenderer)")]
    [Tooltip("LineRenderer za levo gumo (2 točki: anchor → ptica)")]
    public LineRenderer leftBand;
    [Tooltip("LineRenderer za desno gumo (2 točki: anchor → ptica)")]
    public LineRenderer rightBand;

    [Header("Reference")]
    public BirdQueue birdQueue;
    public Camera mainCamera;

    // ── Notranje stanje ────────────────────────────────────────────
    private bool isDragging = false;
    private Vector2 dragPosition;
    private Vector2 slingshotCenter;
    private TrajectoryPreview trajectoryPreview;   // opcijsko, najde se avtomatsko

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        slingshotCenter   = (anchorLeft.position + anchorRight.position) / 2f;
        trajectoryPreview = GetComponent<TrajectoryPreview>();

        HideBands();
    }

    void Update()
    {
        GameObject currentBird = birdQueue?.GetCurrentBird();
        if (currentBird == null) return;

        HandleInput(currentBird);

        if (isDragging)
        {
            UpdateBandVisuals(currentBird.transform.position);
            UpdateTrajectory(currentBird);
        }
    }

    // ── Input ──────────────────────────────────────────────────────

    private void HandleInput(GameObject bird)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorld = ScreenToWorld(Input.mousePosition);
            float clickRadius  = 0.8f;

            if (Vector2.Distance(mouseWorld, bird.transform.position) < clickRadius)
            {
                isDragging = true;
                ShowBands();
                trajectoryPreview?.Show();
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DragBird(bird);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            trajectoryPreview?.Hide();
            LaunchBird(bird);
        }
    }

    // ── Vlečenje ───────────────────────────────────────────────────

    private void DragBird(GameObject bird)
    {
        Vector2 mouseWorld = ScreenToWorld(Input.mousePosition);
        Vector2 offset     = mouseWorld - slingshotCenter;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        // Ptica sme biti samo LEVO od centra
        if (offset.x > 0f) offset.x = 0f;

        dragPosition            = slingshotCenter + offset;
        bird.transform.position = dragPosition;
    }

    // ── Trajektorija ───────────────────────────────────────────────

    private void UpdateTrajectory(GameObject bird)
    {
        if (trajectoryPreview == null) return;

        Vector2 launchDirection = slingshotCenter - dragPosition;
        Vector2 launchForce     = launchDirection * launchForceMultiplier;

        float mass = bird.GetComponent<Rigidbody2D>()?.mass ?? 1f;

        trajectoryPreview.UpdatePreview(dragPosition, launchForce, mass);
    }

    // ── Izstrel ────────────────────────────────────────────────────

    private void LaunchBird(GameObject bird)
    {
        Vector2 launchDirection = slingshotCenter - dragPosition;
        Vector2 launchForce     = launchDirection * launchForceMultiplier;

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
        leftBand.SetPosition(0, anchorLeft.position);
        leftBand.SetPosition(1, birdPos);

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

    private Vector2 ScreenToWorld(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    void OnDrawGizmosSelected()
    {
        if (anchorLeft == null || anchorRight == null) return;

        Vector3 center = (anchorLeft.position + anchorRight.position) / 2f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, maxDragDistance);
    }
}

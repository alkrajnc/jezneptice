using System.Collections;
using UnityEngine;

/// <summary>
/// Upravlja fračo: vlečenje ptice z miško, zakrivljene gumice in izstrel.
/// </summary>
public class SlingshotController : MonoBehaviour
{
    // ── Nastavitve ─────────────────────────────────────────────────
    [Header("Točke frače")]
    public Transform anchorLeft;
    public Transform anchorRight;

    [Header("Fizika izstrela")]
    public float maxDragDistance    = 2.2f;
    public float launchForceMultiplier = 8f;
    public float minLaunchForce     = 5f;

    [Header("Gumice (LineRenderer)")]
    public LineRenderer leftBand;
    public LineRenderer rightBand;

    [Header("Gumice — videz")]
    [Tooltip("Število segmentov krivulje (višje = glajše)")]
    public int   bandSegments   = 12;
    [Tooltip("Povešenost gumice v mirovanju")]
    public float bandSag        = 0.12f;
    [Tooltip("Debelina pri sidrišču")]
    public float widthAtAnchor  = 0.12f;
    [Tooltip("Debelina pri ptici")]
    public float widthAtBird    = 0.06f;
    [Tooltip("Barva sproščene gumice")]
    public Color colorRelaxed   = new Color(0.55f, 0.27f, 0.07f);
    [Tooltip("Barva napete gumice (max vlek)")]
    public Color colorTense     = new Color(0.95f, 0.25f, 0.05f);

    [Header("Animacija odskoka")]
    [Tooltip("Trajanje spring animacije po izstrelu")]
    public float snapDuration   = 0.3f;

    [Header("Reference")]
    public BirdQueue birdQueue;
    public Camera    mainCamera;
    public AudioClip launchSfx;

    // ── Notranje stanje ────────────────────────────────────────────
    private TrajectoryPreview trajectory;
    private bool    isDragging    = false;
    private Vector2 dragPosition;
    private Vector2 slingshotCenter;

    // Za snap animacijo
    private bool    isSnapping    = false;
    private float   snapTimer     = 0f;
    private Vector2 snapStartPos;

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        slingshotCenter = (anchorLeft.position + anchorRight.position) / 2f;
        trajectory      = GetComponent<TrajectoryPreview>();

        InitBand(leftBand);
        InitBand(rightBand);

        // Začni z gumicami pri centru
        dragPosition = slingshotCenter;
        UpdateBandVisuals(dragPosition, 0f);
        HideBands();
    }

    void Update()
    {
        // Snap animacija po izstrelu
        if (isSnapping)
        {
            snapTimer += Time.deltaTime;
            float t      = Mathf.Clamp01(snapTimer / snapDuration);
            float spring = Mathf.Exp(-t * 10f) * Mathf.Cos(t * 22f);
            Vector2 snapPos = slingshotCenter + (snapStartPos - slingshotCenter) * spring;
            UpdateBandVisuals(snapPos, 0f);

            if (snapTimer >= snapDuration)
            {
                isSnapping = false;
                HideBands();
            }
            return;
        }

        GameObject currentBird = birdQueue?.GetCurrentBird();

        if (currentBird == null)
        {
            if (!isSnapping) HideBands();
            return;
        }

        // Gumice vedno vidne ko je ptica na frači
        ShowBands();

        float tension = isDragging
            ? (slingshotCenter - dragPosition).magnitude / maxDragDistance
            : 0f;

        UpdateBandVisuals(currentBird.transform.position, tension);

        Vector2 mouseWorld = ScreenToWorld(Input.mousePosition);

        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            Collider2D col = currentBird.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(mouseWorld))
                isDragging = true;
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                DragBird(currentBird);

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
        Vector2 offset     = mouseWorld - slingshotCenter;

        if (offset.magnitude > maxDragDistance)
            offset = offset.normalized * maxDragDistance;

        if (offset.x > 0f) offset.x = 0f;

        dragPosition          = slingshotCenter + offset;
        bird.transform.position = dragPosition;
    }

    // ── Izstrel ────────────────────────────────────────────────────

    private void LaunchBird(GameObject bird)
    {
        Vector2 launchDirection = slingshotCenter - dragPosition;
        Vector2 launchForce     = launchDirection * launchForceMultiplier;
        if (launchForce.magnitude < minLaunchForce)
            launchForce = launchForce.normalized * minLaunchForce;

        BirdController bc = bird.GetComponent<BirdController>();
        if (bc != null)
            bc.Launch(launchForce);
        else
            Debug.LogWarning("[SlingshotController] BirdController ni najden!");

        if (launchSfx != null)
        {
            var src = GetComponent<AudioSource>();
            if (src != null) src.PlayOneShot(launchSfx);
        }

        // Sproži snap animacijo
        snapStartPos = dragPosition;
        snapTimer    = 0f;
        isSnapping   = true;
        ShowBands();

        birdQueue.OnBirdLaunched();

        Debug.Log($"[SlingshotController] Izstrel! Sila: {launchForce.magnitude:F1}, Kot: {Vector2.Angle(Vector2.right, launchDirection):F1}°");
    }

    // ── Zakrivljene gumice ─────────────────────────────────────────

    private void UpdateBandVisuals(Vector2 birdPos, float tension)
    {
        Color c = Color.Lerp(colorRelaxed, colorTense, tension);
        UpdateSingleBand(leftBand,  anchorLeft.position,  birdPos, tension, c);
        UpdateSingleBand(rightBand, anchorRight.position, birdPos, tension, c);
    }

    private void UpdateSingleBand(LineRenderer lr, Vector3 anchor, Vector2 birdPos, float tension, Color col)
    {
        if (lr == null) return;

        // Kontrolna točka za Bezier — povešenost upade z napetostjo
        Vector3 mid     = (anchor + (Vector3)birdPos) * 0.5f;
        float   sag     = Mathf.Lerp(bandSag, 0f, tension);
        Vector3 control = mid + Vector3.down * sag;

        // Nastavi krivuljo
        for (int i = 0; i < bandSegments; i++)
        {
            float   t   = i / (float)(bandSegments - 1);
            Vector3 pos = QuadBezier(anchor, control, birdPos, t);
            lr.SetPosition(i, pos);
        }

        // Barva
        lr.startColor = col;
        lr.endColor   = new Color(col.r * 0.65f, col.g * 0.65f, col.b * 0.65f, col.a);

        // Variabilna debelina (Bezier krivulja): debela pri sidru, tanka pri ptici
        AnimationCurve widthCurve = new AnimationCurve(
            new Keyframe(0f, widthAtAnchor),
            new Keyframe(0.4f, widthAtAnchor * 0.7f),
            new Keyframe(1f, widthAtBird)
        );
        lr.widthCurve      = widthCurve;
        lr.widthMultiplier = 1f;
    }

    private void ShowBands()
    {
        if (leftBand)  leftBand.enabled  = true;
        if (rightBand) rightBand.enabled = true;
    }

    private void HideBands()
    {
        if (leftBand)  leftBand.enabled  = false;
        if (rightBand) rightBand.enabled = false;
    }

    // ── Setup ──────────────────────────────────────────────────────

    private void InitBand(LineRenderer lr)
    {
        if (lr == null) return;
        lr.positionCount   = bandSegments;
        lr.widthMultiplier = 1f;
        lr.useWorldSpace   = true;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor      = colorRelaxed;
        lr.endColor        = colorRelaxed;
        lr.sortingOrder    = 10;
    }

    // ── Pomožne metode ─────────────────────────────────────────────

    private Vector3 QuadBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z));
        return mainCamera.ScreenToWorldPoint(pos);
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

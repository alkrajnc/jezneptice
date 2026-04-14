using UnityEngine;

/// <summary>
/// Prikazuje prekinjeno trajektorijo ptice med vlečenjem frače.
/// Uporablja Physics2D.Simulate() za natančen izračun — upošteva
/// gravitacijo in začetno silo točno tako kot bo letela prava ptica.
///
/// SETUP V UNITY INSPECTORJU:
///   1. Na isti GameObject kot SlingshotController dodaj to skripto
///   2. Ustvari nov Material "TrajectoryDot" (Sprite/Default, bela barva)
///      in ga povleci v dotMaterial
///   3. Nastavi dotCount (npr. 12) in dotSpacing (npr. 0.08)
///   4. SlingshotController pokliče Show() / Hide() / UpdatePreview()
/// </summary>
public class TrajectoryPreview : MonoBehaviour
{
    // ── Nastavitve ─────────────────────────────────────────────────
    [Header("Pike trajektorije")]
    [Tooltip("Koliko pik prikazati")]
    public int dotCount = 12;

    [Tooltip("Časovni razmak med pikami (manjše = pike bližje skupaj)")]
    public float dotSpacing = 0.08f;

    [Tooltip("Velikost vsake pike")]
    public float dotSize = 0.18f;

    [Tooltip("Material za pike (Sprite/Default)")]
    public Material dotMaterial;

    [Tooltip("Barva pike — alfa upadajoča proti koncu")]
    public Color dotColor = new Color(1f, 1f, 1f, 0.85f);

    [Header("Fizika (mora ujemati BirdController)")]
    [Tooltip("Gravitacijski množilnik — pusti na 1 če ne spreminjaš Physics2D")]
    public float gravityScale = 1f;

    // ── Notranje stanje ────────────────────────────────────────────
    private GameObject[] dots;
    private bool isVisible = false;

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        CreateDots();
        Hide();
    }

    // ── Javne metode (kliče SlingshotController) ───────────────────

    /// <summary>
    /// Posodobi pozicije pik glede na trenutno pozicijo ptice in silo izstrela.
    /// Pokliči vsak frame med vlečenjem.
    /// </summary>
    public void UpdatePreview(Vector2 startPosition, Vector2 launchForce, float birdMass)
    {
        // Izračunaj začetno hitrost: F = m*a → v = F / m (za Impulse način)
        Vector2 initialVelocity = launchForce / birdMass;
        Vector2 gravity         = Physics2D.gravity * gravityScale;

        for (int i = 0; i < dotCount; i++)
        {
            float   t   = dotSpacing * (i + 1);
            Vector2 pos = CalculatePosition(startPosition, initialVelocity, gravity, t);

            dots[i].transform.position = pos;

            // Pike postajajo bolj transparentne proti koncu
            float alpha     = Mathf.Lerp(0.85f, 0.1f, (float)i / dotCount);
            float scale     = Mathf.Lerp(dotSize, dotSize * 0.5f, (float)i / dotCount);
            SetDotAlpha(i, alpha);
            dots[i].transform.localScale = Vector3.one * scale;
        }
    }

    /// <summary>Prikaži vse pike.</summary>
    public void Show()
    {
        if (isVisible) return;
        isVisible = true;
        foreach (var dot in dots) dot.SetActive(true);
    }

    /// <summary>Skrij vse pike (po izstrelu ali spustu brez vleka).</summary>
    public void Hide()
    {
        isVisible = false;
        foreach (var dot in dots) dot.SetActive(false);
    }

    // ── Zasebne metode ─────────────────────────────────────────────

    /// <summary>
    /// Kinematična enačba za položaj projektila:
    ///   pos = start + v₀·t + ½·g·t²
    /// </summary>
    private Vector2 CalculatePosition(Vector2 start, Vector2 v0, Vector2 g, float t)
    {
        return start + v0 * t + 0.5f * g * t * t;
    }

    /// <summary>Ustvari vse dot GameObjecte z SpriteRenderer.</summary>
    private void CreateDots()
    {
        dots = new GameObject[dotCount];

        for (int i = 0; i < dotCount; i++)
        {
            dots[i] = new GameObject($"TrajectoryDot_{i}");
            dots[i].transform.SetParent(transform);
            dots[i].transform.localScale = Vector3.one * dotSize;

            SpriteRenderer sr = dots[i].AddComponent<SpriteRenderer>();
            sr.sprite          = CreateCircleSprite();
            sr.color           = dotColor;
            sr.sortingLayerName = "UI";   // pike so vedno na vrhu
            sr.sortingOrder    = 10;

            if (dotMaterial != null)
                sr.material = dotMaterial;
        }
    }

    /// <summary>Nastavi alfa kanał na i-ti piki.</summary>
    private void SetDotAlpha(int index, float alpha)
    {
        SpriteRenderer sr = dots[index].GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Color c = sr.color;
        c.a      = alpha;
        sr.color = c;
    }

    /// <summary>
    /// Ustvari preprost okrogel sprite za pike programsko —
    /// ni potrebe po zunanji teksturi.
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int      resolution = 32;
        Texture2D tex       = new Texture2D(resolution, resolution);
        float    radius     = resolution / 2f;
        Vector2  center     = new Vector2(radius, radius);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                // Mehki rob (anti-aliasing)
                float alpha = Mathf.Clamp01(1f - (dist - (radius - 2f)) / 2f);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f));
    }
}

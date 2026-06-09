using System.Collections;
using UnityEngine;

public enum BirdSpecialAbility
{
    EnergyBurst,
    SpeedBoost
}

/// <summary>
/// Upravlja stanje in obnašanje ptice.
/// Ptica ima 3 stanja: čaka na frači, leti, pristane/umre.
/// </summary>
public class BirdController : MonoBehaviour
{
    // ── Stanja ptice ──────────────────────────────────────────────
    public enum BirdState { OnSlingshot, Flying, Dead }
    public BirdState CurrentState { get; private set; } = BirdState.OnSlingshot;


    // ── Nastavitve ─────────────────────────────────────────────────
    [Header("Fizika")]
    [Tooltip("Koliko časa po pristanku se ptica uniči")]
    public float destroyDelay = 0.6f;
    public float maxFlightTime = 3.2f;

    [Header("Posebna moč")]
    [Tooltip("Ali ima ta ptica posebno moč (klik med letom)")]
    public bool hasSpecialAbility = false;
    public BirdSpecialAbility specialAbilityType = BirdSpecialAbility.EnergyBurst;

    [Header("Rdeca ptica - sunek")]
    [Tooltip("Radij kratkega sunka energije okoli ptice.")]
    public float energyBurstRadius = 2.2f;
    [Tooltip("Moc sunka. Nizka vrednost pomeni, da stvari samo zamaje.")]
    public float energyBurstImpulse = 2.8f;
    [Tooltip("Malo dvigne objekte, da se lazje prevrnejo.")]
    public float energyBurstLift = 0.35f;

    [Header("Rdeca ptica - vizual")]
    public float energyBurstVisualDuration = 0.35f;
    public float energyBurstRingWidth = 0.08f;
    public Color energyBurstColor = new Color(1f, 0.25f, 0.05f, 0.85f);

    [Header("Rumena ptica - pospesek")]
    [Tooltip("Kolikokrat hitreje leti po aktivaciji.")]
    public float speedBoostMultiplier = 1.9f;
    [Tooltip("Najmanjsa hitrost po boostu, da klik vedno nekaj naredi.")]
    public float speedBoostMinSpeed = 9f;

    [Header("Rumena ptica - vizual")]
    public float speedBoostVisualDuration = 0.28f;
    public float speedBoostStreakLength = 1.7f;
    public float speedBoostStreakWidth = 0.09f;
    public Color speedBoostColor = new Color(1f, 0.9f, 0.05f, 0.9f);
    [Tooltip("Ali je bila posebna moč že aktivirana")]
    private bool abilityUsed = false;

    // ── Reference ──────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Collider2D col;

    public AudioClip launchSfx;

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        SetPhysicsEnabled(false);
    }

    void Start()
    {
        // Ignoriraj trke z mejnimi zidovi — ptica leti skozi
        int wallLayer = LayerMask.NameToLayer("BoundWall");
        if (wallLayer < 0 || col == null) return;

        foreach (var wallCol in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (wallCol.gameObject.layer == wallLayer)
                Physics2D.IgnoreCollision(col, wallCol);
        }
    }

    void Update()
    {
        // Klik med letom aktivira posebno moč
        if (CurrentState == BirdState.Flying && hasSpecialAbility && !abilityUsed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ActivateSpecialAbility();
            }
        }
    }

    // ── Javne metode ───────────────────────────────────────────────

    /// <summary>
    /// Pokliče SlingshotController ko spusti ptico. Preda začetno hitrost.
    /// </summary>
    public void Launch(Vector2 force)
    {
        if (CurrentState != BirdState.OnSlingshot) return;

        CurrentState = BirdState.Flying;
        SetPhysicsEnabled(true);
        rb.AddForce(force, ForceMode2D.Impulse);
        GetComponent<AudioSource>().PlayOneShot(launchSfx);
        StartCoroutine(DieAfterMaxFlightTime());

        Debug.Log($"[BirdController] Ptica izstreljena s silo: {force}");
    }

    /// <summary>
    /// Ptica je pristala ali jo je kaj zadelo — prehod v Dead stanje.
    /// </summary>
    public void Die()
    {
        if (CurrentState == BirdState.Dead) return;

        CurrentState = BirdState.Dead;
        Debug.Log("[BirdController] Ptica je mrtva.");

        // Upočasni gibanje pri pristanku
        rb.linearDamping = 5f;
        rb.angularDamping = 5f;

        Destroy(gameObject, destroyDelay);
    }

    // ── Zasebne metode ─────────────────────────────────────────────

    /// <summary>
    /// Vklopi/izklopi fiziko (gravitacija + collider).
    /// Ko je ptica na frači, fizika ne sme vplivati nanjo.
    /// </summary>
    private void SetPhysicsEnabled(bool enabled)
    {
        rb.gravityScale = enabled ? 1f : 0f;
        rb.bodyType = enabled ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
    }

    private IEnumerator DieAfterMaxFlightTime()
    {
        yield return new WaitForSeconds(maxFlightTime);

        if (CurrentState == BirdState.Flying)
            Die();
    }

    /// <summary>
    /// Osnova za posebno moč — vsak tip ptice to prepiše (override).
    /// </summary>
    protected virtual void ActivateSpecialAbility()
    {
        abilityUsed = true;

        if (specialAbilityType == BirdSpecialAbility.SpeedBoost)
        {
            Vector2 boostDirection = ApplySpeedBoost();
            ShowSpeedBoostVisual(boostDirection);
            Debug.Log("[BirdController] Rumena ptica: pospesek!");
            return;
        }

        ApplyEnergyBurst();
        ShowEnergyBurstVisual();
        Debug.Log("[BirdController] Rdeca ptica: sunek energije!");
    }

    // ── Trki ───────────────────────────────────────────────────────

    private void ApplyEnergyBurst()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, energyBurstRadius);

        foreach (var hit in hits)
        {
            if (hit == null || hit.attachedRigidbody == null || hit.attachedRigidbody == rb)
                continue;

            Rigidbody2D targetRb = hit.attachedRigidbody;
            if (targetRb.bodyType != RigidbodyType2D.Dynamic)
                continue;

            Vector2 offset = targetRb.worldCenterOfMass - rb.worldCenterOfMass;
            float distance = Mathf.Max(offset.magnitude, 0.1f);
            float strength = 1f - Mathf.Clamp01(distance / energyBurstRadius);
            Vector2 direction = offset.normalized + Vector2.up * energyBurstLift;

            targetRb.AddForce(direction.normalized * energyBurstImpulse * strength, ForceMode2D.Impulse);
            targetRb.AddTorque(Random.Range(-energyBurstImpulse, energyBurstImpulse) * strength, ForceMode2D.Impulse);
        }
    }

    private void ShowEnergyBurstVisual()
    {
        StartCoroutine(AnimateEnergyBurstRing());
    }

    private Vector2 ApplySpeedBoost()
    {
        Vector2 velocity = rb.linearVelocity;
        Vector2 direction = velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : Vector2.right;

        float boostedSpeed = Mathf.Max(velocity.magnitude * speedBoostMultiplier, speedBoostMinSpeed);
        rb.linearVelocity = direction * boostedSpeed;

        return direction;
    }

    private void ShowSpeedBoostVisual(Vector2 direction)
    {
        StartCoroutine(AnimateSpeedBoostStreak(direction));
    }

    private IEnumerator AnimateSpeedBoostStreak(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        direction.Normalize();
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float duration = Mathf.Max(0.01f, speedBoostVisualDuration);

        GameObject effect = new GameObject("SpeedBoostStreak");
        Destroy(effect, duration + 0.1f);

        const int streakCount = 3;
        LineRenderer[] streaks = new LineRenderer[streakCount];
        float[] offsets = { -0.18f, 0f, 0.18f };

        for (int i = 0; i < streakCount; i++)
        {
            GameObject streakObject = new GameObject($"SpeedBoostLine_{i}");
            streakObject.transform.SetParent(effect.transform);

            LineRenderer line = streakObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.sortingOrder = 31;
            line.startWidth = speedBoostStreakWidth;
            line.endWidth = 0f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            streaks[i] = line;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Color color = speedBoostColor;
            color.a = Mathf.Lerp(speedBoostColor.a, 0f, t);

            for (int i = 0; i < streakCount; i++)
            {
                Vector3 offset = perpendicular * offsets[i];
                Vector3 origin = transform.position + offset;
                Vector3 tail = origin - (Vector3)direction * Mathf.Lerp(0.25f, speedBoostStreakLength, t);

                streaks[i].startColor = color;
                streaks[i].endColor = new Color(color.r, color.g, color.b, 0f);
                streaks[i].SetPosition(0, origin);
                streaks[i].SetPosition(1, tail);
            }

            yield return null;
        }

        Destroy(effect);
    }

    private IEnumerator AnimateEnergyBurstRing()
    {
        const int segments = 64;
        GameObject ring = new GameObject("EnergyBurstRing");
        LineRenderer line = ring.AddComponent<LineRenderer>();
        Destroy(ring, energyBurstVisualDuration + 0.1f);
        Vector3 origin = transform.position;
        float duration = Mathf.Max(0.01f, energyBurstVisualDuration);

        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = segments;
        line.sortingOrder = 30;
        line.startWidth = energyBurstRingWidth;
        line.endWidth = energyBurstRingWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float radius = Mathf.Lerp(0.25f, energyBurstRadius, t);
            Color color = energyBurstColor;
            color.a = Mathf.Lerp(energyBurstColor.a, 0f, t);
            line.startColor = color;
            line.endColor = color;

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                Vector3 pos = origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                line.SetPosition(i, pos);
            }

            yield return null;
        }

        Destroy(ring);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (CurrentState != BirdState.Flying) return;

        // Ob prvem trku ptica zakljuci let.
        if (collision.gameObject.CompareTag("Bird"))
            return;

        Die();
    }

    void OnDrawGizmosSelected()
    {
        if (!hasSpecialAbility) return;

        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, energyBurstRadius);
    }
}

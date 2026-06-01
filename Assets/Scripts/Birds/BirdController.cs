using System.Collections;
using UnityEngine;

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
    public float destroyDelay = 3f;

    [Header("Posebna moč")]
    [Tooltip("Ali ima ta ptica posebno moč (klik med letom)")]
    public bool hasSpecialAbility = false;

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
    [Tooltip("Ali je bila posebna moč že aktivirana")]
    private bool abilityUsed = false;

    // ── Reference ──────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Collider2D col;

    public AudioClip launchSfx;

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        SetPhysicsEnabled(false);
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

    /// <summary>
    /// Osnova za posebno moč — vsak tip ptice to prepiše (override).
    /// </summary>
    protected virtual void ActivateSpecialAbility()
    {
        abilityUsed = true;
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

        // Ob trku z bloki ali prašiči ptica "umre"
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Pig"))
        {
            Die();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!hasSpecialAbility) return;

        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, energyBurstRadius);
    }
}

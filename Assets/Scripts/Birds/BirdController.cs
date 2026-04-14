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
    [Tooltip("Ali je bila posebna moč že aktivirana")]
    private bool abilityUsed = false;

    // ── Reference ──────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Collider2D col;

    // ── Unity callbacks ────────────────────────────────────────────
    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Ko je ptica na frači, fizika ne sme delovati
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
        rb.linearDamping  = 5f;
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
        rb.gravityScale  = enabled ? 1f : 0f;
        rb.isKinematic   = !enabled;
        col.enabled      = enabled;
    }

    /// <summary>
    /// Osnova za posebno moč — vsak tip ptice to prepiše (override).
    /// </summary>
    protected virtual void ActivateSpecialAbility()
    {
        abilityUsed = true;
        Debug.Log("[BirdController] Posebna moč aktivirana! (base - override v podrazredu)");
    }

    // ── Trki ───────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (CurrentState != BirdState.Flying) return;

        // Ob trku z bloki ali prašiči ptica "umre"
        if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Pig"))
        {
            Die();
        }
    }
}

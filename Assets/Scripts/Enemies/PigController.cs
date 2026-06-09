using System.Collections;
using UnityEngine;

/// <summary>
/// Stanje prasice glede na zdravje.
/// </summary>
public enum PigState
{
    Healthy,    // 100 % HP
    Damaged,    // 50–99 % HP
    Critical,   // 1–49 % HP
    Dead        // 0 HP
}

/// <summary>
/// Krmilnik prasice – zdravje, poškodbe, animacije in točkovanje.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PigController : MonoBehaviour
{

    [Header("Zdravje")]
    [SerializeField] private float maxHealth = 70f;
    [SerializeField] private float damagedThreshold = 50f;  // Pod to vrednostjo → Damaged
    [SerializeField] private float criticalThreshold = 25f;  // Pod to vrednostjo → Critical
    [SerializeField] private readonly float minImpactDamage = 5f;

    [Header("Točke")]
    [SerializeField] private int killScore = 500;
    [SerializeField] private int damageScore = 100;

    [Header("Sprites – stanja")]
    [SerializeField] private Sprite spriteHealthy;
    [SerializeField] private Sprite spriteDamaged;
    [SerializeField] private Sprite spriteCritical;

    [Header("Vizualni efekti")]
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private readonly float flashDuration = 0.1f;

    [Header("Zvoki")]
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip damagedSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip idleSFX;
    [SerializeField] private AudioClip collisionSFX;

    [Header("Obnašanje")]
    [SerializeField] private readonly float idleSFXInterval = 8f;

    private float currentHealth;
    private PigState currentState = PigState.Healthy;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Rigidbody2D rb;

    private bool isDead = false;
    private float idleTimer;

    private Color originalColor;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public PigState State => currentState;
    public bool IsDead => isDead;
    public float HealthPercent => currentHealth / maxHealth;

    public void Configure(float health, int scoreForKill = 0, int scoreForDamage = 0)
    {
        if (health > 0f)
        {
            maxHealth = health;
            damagedThreshold = maxHealth * 0.5f;
            criticalThreshold = maxHealth * 0.25f;
        }

        if (scoreForKill > 0)
            killScore = scoreForKill;

        if (scoreForDamage > 0)
            damageScore = scoreForDamage;

        currentHealth = maxHealth;
        currentState = PigState.Healthy;
        isDead = false;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            originalColor = spriteRenderer.color;
            if (spriteHealthy != null)
                spriteRenderer.sprite = spriteHealthy;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        originalColor = spriteRenderer ? spriteRenderer.color : Color.white;
        idleTimer = Random.Range(idleSFXInterval * 0.5f, idleSFXInterval);
    }

    private void Update()
    {
        if (isDead) return;

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            PlaySound(idleSFX);
            idleTimer = Random.Range(idleSFXInterval * 0.5f, idleSFXInterval * 1.5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;

        PlaySound(collisionSFX);

        float impactSpeed = col.relativeVelocity.magnitude;
        float impactDamage = impactSpeed * 2f;

        if (impactDamage >= minImpactDamage)
            TakeDamage(impactDamage);
    }


    /// <summary>Prasica sprejme poškodbo.</summary>
    public void TakeDamage(float amount)
    {

        if (isDead || amount <= 0f) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        // Točke za poškodbo
        int scoreGain = Mathf.RoundToInt((previousHealth - currentHealth) / maxHealth * damageScore);
        GameManager.Instance?.AddScore(scoreGain);

        // Vizualni odziv
        SpawnHitVFX();
        StartCoroutine(FlashDamage());
        PlaySound(hitSFX);

        // Posodobi stanje
        UpdateState();

        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>Instantno uniči prasico (brez animacije).</summary>
    public void ForceKill()
    {
        currentHealth = 0f;
        Die();
    }

    /// <summary>Ozdravi prasico (za testiranje).</summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateState();
    }

    // ─────────────────────────────────────────────
    // Stanja in sprites
    // ─────────────────────────────────────────────
    private void UpdateState()
    {
        PigState newState;

        if (currentHealth <= 0f)
            newState = PigState.Dead;
        else if (currentHealth < criticalThreshold)
            newState = PigState.Critical;
        else if (currentHealth < damagedThreshold)
            newState = PigState.Damaged;
        else
            newState = PigState.Healthy;

        if (newState == currentState) return;

        currentState = newState;
        UpdateSprite();

        // Zvok ob prehodu v poškodovano stanje
        if (currentState == PigState.Damaged || currentState == PigState.Critical)
            PlaySound(damagedSFX);
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        switch (currentState)
        {
            case PigState.Healthy: spriteRenderer.sprite = spriteHealthy; break;
            case PigState.Damaged: spriteRenderer.sprite = spriteDamaged; break;
            case PigState.Critical: spriteRenderer.sprite = spriteCritical; break;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        PlaySound(deathSFX);

        var vfx = new GameObject("PigDeathVFX");
        vfx.transform.position = transform.position;
        vfx.AddComponent<PigDeathVFX>();

        GameManager.Instance?.AddScore(killScore);
        LevelManager.Instance?.OnPigDestroyed(this);
        if (spriteRenderer) spriteRenderer.enabled = false;
        Destroy(gameObject, 0.3f);
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void SpawnHitVFX()
    {
        if (hitVFXPrefab)
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
    }


    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.8f,
            $"HP: {currentHealth:F0}/{maxHealth:F0}\n{currentState}");
#endif
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// Material bloka določa odpornost in tip škode.
/// </summary>
public enum BlockMaterial
{
    Wood,   // Šibak, hitro gori
    Stone,  // Srednje odporen
    Ice,    // Nizka odpornost, drsi
    Metal   // Zelo odporen
}

/// <summary>
/// Poškodba in uničenje blokov.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BlockDamage : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private BlockMaterial material = BlockMaterial.Wood;

    [Header("Zdravje")]
    [SerializeField] private float maxHealth = 60f;
    [SerializeField] private float minImpactForce = 3f; // Šibkejši udarci se ignorirajo

    [Header("Sprites – poškodba")]
    [SerializeField] private Sprite spriteIntact;
    [SerializeField] private Sprite spriteCracked;
    [SerializeField] private Sprite spriteBroken;

    [Header("Vizualni efekti")]
    [SerializeField] private GameObject destroyVFXPrefab;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private float flashDuration = 0.08f;

    [Header("Zvoki")]
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip destroySFX;

    [Header("Točke")]
    [SerializeField] private int destroyScore = 200;

    // ─────────────────────────────────────────────
    // Interno
    // ─────────────────────────────────────────────
    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isDestroyed;

    // Odpornost glede na material (multiplikator poškodbe)
    private static readonly float[] MaterialResistance =
    {
        1.0f,  // Wood
        0.4f,  // Stone
        1.3f,  // Ice
        0.15f  // Metal
    };

    // ─────────────────────────────────────────────
    // Unity
    // ─────────────────────────────────────────────
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        if (spriteRenderer && spriteIntact)
            spriteRenderer.sprite = spriteIntact;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isDestroyed) return;

        float impact = col.relativeVelocity.magnitude;
        if (impact < minImpactForce) return;

        float damage = impact * 3f * MaterialResistance[(int)material];
        TakeDamage(damage);
    }

    // ─────────────────────────────────────────────
    // Javna metoda (kličeta jo BirdProjectile in eksplozija)
    // ─────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (isDestroyed || amount <= 0f) return;

        currentHealth -= amount * MaterialResistance[(int)material];
        currentHealth = Mathf.Max(0f, currentHealth);

        PlaySound(hitSFX);
        StartCoroutine(FlashDamage());
        UpdateSprite();

        if (currentHealth <= 0f)
            DestroyBlock();
    }

    // ─────────────────────────────────────────────
    // Sprite
    // ─────────────────────────────────────────────
    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        float pct = currentHealth / maxHealth;
        if (pct > 0.6f)
            spriteRenderer.sprite = spriteIntact;
        else if (pct > 0.25f)
            spriteRenderer.sprite = spriteCracked;
        else
            spriteRenderer.sprite = spriteBroken;
    }

    // ─────────────────────────────────────────────
    // Uničenje
    // ─────────────────────────────────────────────
    private void DestroyBlock()
    {
        isDestroyed = true;
        PlaySound(destroySFX);

        if (destroyVFXPrefab)
            Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);

        GameManager.Instance?.AddScore(destroyScore);
        Destroy(gameObject, 0.1f);
    }

    // ─────────────────────────────────────────────
    // Flash efekt
    // ─────────────────────────────────────────────
    private IEnumerator FlashDamage()
    {
        if (spriteRenderer == null) yield break;
        Color orig = spriteRenderer.color;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = orig;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }
}
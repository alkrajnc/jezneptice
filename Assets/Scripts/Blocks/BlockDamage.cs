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
    private Sprite fallbackSprite;

    // Odpornost glede na material (multiplikator poškodbe)
    private static readonly float[] MaterialResistance =
    {
        1.0f,  // Wood
        0.4f,  // Stone
        1.3f,  // Ice
        0.15f  // Metal
    };

    public void Configure(BlockMaterial materialType, int score, float health = -1f)
    {
        material = materialType;
        destroyScore = score > 0 ? score : GetDefaultScore(materialType);
        maxHealth = health > 0f ? health : GetDefaultHealth(materialType);
        currentHealth = maxHealth;
        isDestroyed = false;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            if (spriteIntact == null && spriteRenderer.sprite != null)
                spriteIntact = spriteRenderer.sprite;

            fallbackSprite = spriteIntact != null ? spriteIntact : spriteRenderer.sprite;
            spriteRenderer.enabled = true;
            spriteRenderer.color = GetMaterialColor(materialType);
            SetDamageSprite(spriteIntact);
        }

        foreach (var blockCollider in GetComponentsInChildren<Collider2D>())
            blockCollider.enabled = true;

        var blockRb = GetComponent<Rigidbody2D>();
        if (blockRb != null)
        {
            blockRb.simulated = true;
            blockRb.bodyType = RigidbodyType2D.Dynamic;
            blockRb.linearVelocity = Vector2.zero;
            blockRb.angularVelocity = 0f;
            blockRb.mass = GetDefaultMass(materialType);
        }
    }

    // ─────────────────────────────────────────────
    // Unity
    // ─────────────────────────────────────────────
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            if (spriteIntact == null && spriteRenderer.sprite != null)
                spriteIntact = spriteRenderer.sprite;

            fallbackSprite = spriteIntact != null ? spriteIntact : spriteRenderer.sprite;
            SetDamageSprite(spriteIntact);
        }
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
            SetDamageSprite(spriteIntact);
        else if (pct > 0.25f)
            SetDamageSprite(spriteCracked);
        else
            SetDamageSprite(spriteBroken);
    }

    // ─────────────────────────────────────────────
    // Uničenje
    // ─────────────────────────────────────────────
    private void DestroyBlock()
    {
        isDestroyed = true;

        if (destroySFX)
            AudioSource.PlayClipAtPoint(destroySFX, transform.position);

        foreach (var blockCollider in GetComponentsInChildren<Collider2D>())
            blockCollider.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (spriteRenderer)
            spriteRenderer.enabled = false;

        if (destroyVFXPrefab)
            Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);

        GameManager.Instance?.AddScore(destroyScore);
        Destroy(gameObject);
    }

    private void SetDamageSprite(Sprite nextSprite)
    {
        if (spriteRenderer == null) return;

        Sprite visibleSprite = nextSprite != null ? nextSprite : fallbackSprite;
        if (visibleSprite != null)
            spriteRenderer.sprite = visibleSprite;
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

    private int GetDefaultScore(BlockMaterial materialType)
    {
        switch (materialType)
        {
            case BlockMaterial.Stone: return 260;
            case BlockMaterial.Ice: return 180;
            case BlockMaterial.Metal: return 320;
            default: return 200;
        }
    }

    private float GetDefaultHealth(BlockMaterial materialType)
    {
        switch (materialType)
        {
            case BlockMaterial.Stone: return 95f;
            case BlockMaterial.Ice: return 45f;
            case BlockMaterial.Metal: return 130f;
            default: return 60f;
        }
    }

    private float GetDefaultMass(BlockMaterial materialType)
    {
        switch (materialType)
        {
            case BlockMaterial.Stone: return 1.8f;
            case BlockMaterial.Ice: return 0.75f;
            case BlockMaterial.Metal: return 2.4f;
            default: return 1f;
        }
    }

    private Color GetMaterialColor(BlockMaterial materialType)
    {
        switch (materialType)
        {
            case BlockMaterial.Stone: return new Color(0.48f, 0.48f, 0.52f, 1f);
            case BlockMaterial.Ice: return new Color(0.55f, 0.9f, 1f, 0.92f);
            case BlockMaterial.Metal: return new Color(0.42f, 0.43f, 0.46f, 1f);
            default: return new Color(0.72f, 0.42f, 0.2f, 1f);
        }
    }
}

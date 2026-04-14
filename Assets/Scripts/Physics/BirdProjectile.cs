using System.Collections;
using UnityEngine;

/// <summary>
/// Vrste ptic – vsaka ima svojo posebno sposobnost.
/// </summary>
public enum BirdType
{
    Red,        // Osnovna ptica – brez posebnosti
    Yellow,     // Pospeši v zraku
    Blue,       // Razdeli se na 3 ptice
    Black,      // Eksplodira
    White       // Odvrže jajce (bombo)
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BirdProjectile : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inšpektor
    // ─────────────────────────────────────────────
    [Header("Vrsta ptice")]
    [SerializeField] private BirdType birdType = BirdType.Red;

    [Header("Fizika")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float gravityScale = 1.5f;

    [Header("Pot leta (vizualizacija)")]
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.05f;
    [SerializeField] private GameObject trajectoryDotPrefab;

    [Header("Posebne sposobnosti")]
    // Yellow – boost
    [SerializeField] private float yellowBoostMultiplier = 2.5f;
    // Blue – clone
    [SerializeField] private GameObject blueBirdPrefab;
    [SerializeField] private float blueSpreadAngle = 15f;
    // Black – eksplozija
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private GameObject explosionVFXPrefab;
    // White – jajce
    [SerializeField] private GameObject eggBombPrefab;
    [SerializeField] private float eggDropForce = 300f;

    [Header("Zvoki")]
    [SerializeField] private AudioClip launchSFX;
    [SerializeField] private AudioClip abilitySFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip explosionSFX;

    [Header("Poškodba ob trku")]
    [SerializeField] private float baseDamage = 50f;
    [SerializeField] private float damageVelocityMultiplier = 1.5f;

    // ─────────────────────────────────────────────
    // Interno stanje
    // ─────────────────────────────────────────────
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private TrailRenderer trail;

    private bool isLaunched = false;
    private bool abilityUsed = false;
    private bool hasLanded = false;

    private Vector2 launchVelocity;

    // Točke poti leta
    private GameObject[] trajectoryDots;

    // ─────────────────────────────────────────────
    // Javne lastnosti
    // ─────────────────────────────────────────────
    public BirdType Type => birdType;
    public bool IsLaunched => isLaunched;
    public bool HasLanded => hasLanded;

    // ─────────────────────────────────────────────
    // Unity callbacks
    // ─────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        trail = GetComponent<TrailRenderer>();

        rb.mass = mass;
        rb.gravityScale = gravityScale;
        rb.isKinematic = true; // Miruje dokler ga ne izstrelimo

        // Skrij sled
        if (trail) trail.enabled = false;
    }

    private void Update()
    {
        if (!isLaunched || hasLanded) return;

        // Rotacija ptice skladno s smerjo leta
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, angle),
                Time.deltaTime * 10f);
        }

        // Posebna sposobnost ob kliku/dotiku
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            ActivateAbility();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!isLaunched || hasLanded) return;

        float impactSpeed = col.relativeVelocity.magnitude;
        ApplyContactDamage(col, impactSpeed);

        PlaySound(hitSFX);
        hasLanded = true;

        // Črna ptica eksplodira ob prvem trku
        if (birdType == BirdType.Black && !abilityUsed)
            StartCoroutine(ExplodeDelayed(0.1f));
        else
            StartCoroutine(DestroyAfterDelay(2f));
    }

    // ─────────────────────────────────────────────
    // Javne metode
    // ─────────────────────────────────────────────

    /// <summary>Izstreli ptico s podano hitrostjo.</summary>
    public void Launch(Vector2 velocity)
    {
        isLaunched = true;
        rb.isKinematic = false;
        rb.velocity = velocity;
        launchVelocity = velocity;

        if (trail) trail.enabled = true;

        PlaySound(launchSFX);
        HideTrajectory();
    }

    /// <summary>Izriše predvideno pot leta (kliče katapult pred izstrelom).</summary>
    public void ShowTrajectory(Vector2 startPos, Vector2 velocity)
    {
        if (trajectoryDotPrefab == null) return;

        if (trajectoryDots == null)
        {
            trajectoryDots = new GameObject[trajectoryPoints];
            for (int i = 0; i < trajectoryPoints; i++)
                trajectoryDots[i] = Instantiate(trajectoryDotPrefab);
        }

        Vector2 pos = startPos;
        Vector2 vel = velocity;
        Vector2 gravity = Physics2D.gravity * gravityScale;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            trajectoryDots[i].SetActive(true);
            trajectoryDots[i].transform.position = pos;

            vel += gravity * trajectoryTimeStep;
            pos += vel * trajectoryTimeStep;

            // Prosojnost se zmanjšuje z razdaljo
            var sr = trajectoryDots[i].GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(1f, 1f, 1f, 1f - (float)i / trajectoryPoints);
        }
    }

    public void HideTrajectory()
    {
        if (trajectoryDots == null) return;
        foreach (var dot in trajectoryDots)
            if (dot) dot.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // Posebne sposobnosti
    // ─────────────────────────────────────────────
    private void ActivateAbility()
    {
        if (abilityUsed || !isLaunched || hasLanded) return;
        abilityUsed = true;

        switch (birdType)
        {
            case BirdType.Red: AbilityRed(); break;
            case BirdType.Yellow: AbilityYellow(); break;
            case BirdType.Blue: AbilityBlue(); break;
            case BirdType.Black: AbilityBlack(); break;
            case BirdType.White: AbilityWhite(); break;
        }

        PlaySound(abilitySFX);
    }

    // Rdeča – krik (rahlo poveča poškodbo, brez fizičnega efekta)
    private void AbilityRed()
    {
        baseDamage *= 1.5f;
        // TODO: Predvajaj animacijo krika
        Debug.Log("[Bird] Rdeča ptica: KRIK!");
    }

    // Rumena – pospeši naprej
    private void AbilityYellow()
    {
        rb.velocity = rb.velocity.normalized * rb.velocity.magnitude * yellowBoostMultiplier;
        Debug.Log("[Bird] Rumena ptica: POSPEŠEK!");
    }

    // Modra – razdeli se na 3
    private void AbilityBlue()
    {
        if (blueBirdPrefab == null) return;

        Vector2 currentVel = rb.velocity;

        for (int i = -1; i <= 1; i += 2) // -1 in +1 (levo in desno)
        {
            float angle = blueSpreadAngle * i;
            Vector2 newVel = RotateVector(currentVel, angle);
            GameObject clone = Instantiate(blueBirdPrefab, transform.position, Quaternion.identity);
            var cloneBird = clone.GetComponent<BirdProjectile>();
            if (cloneBird != null)
            {
                cloneBird.birdType = BirdType.Red; // Kloni so navadni
                cloneBird.abilityUsed = true;         // Ne morejo spet aktivirati
                cloneBird.Launch(newVel);
            }
        }

        // Originalna ptica nadaljuje naravnost
        Debug.Log("[Bird] Modra ptica: RAZDELITEV!");
    }

    // Črna – zakasnjena eksplozija (aktivacija = takojna eksplozija)
    private void AbilityBlack()
    {
        StartCoroutine(ExplodeDelayed(0f));
        Debug.Log("[Bird] Črna ptica: EKSPLOZIJA!");
    }

    // Bela – odvrže jajce navzdol
    private void AbilityWhite()
    {
        if (eggBombPrefab == null) return;

        GameObject egg = Instantiate(eggBombPrefab, transform.position, Quaternion.identity);
        var eggRb = egg.GetComponent<Rigidbody2D>();
        if (eggRb != null)
            eggRb.AddForce(Vector2.down * eggDropForce);

        // Bela ptica se odbije navzgor
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Abs(rb.velocity.y) * 0.5f);
        Debug.Log("[Bird] Bela ptica: JAJCE ODVRŽENO!");
    }

    // ─────────────────────────────────────────────
    // Eksplozija
    // ─────────────────────────────────────────────
    private IEnumerator ExplodeDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    private void Explode()
    {
        // Vizualni efekt
        if (explosionVFXPrefab)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        PlaySound(explosionSFX);

        // Poišči vse objekte v radiju
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            // Fizična sila
            var hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float force = explosionForce * (1f - distance / explosionRadius);
                hitRb.AddForce(dir * force);
            }

            // Poškodba
            float distance2 = Vector2.Distance(transform.position, hit.transform.position);
            float damage = baseDamage * (1f - distance2 / explosionRadius);

            var pig = hit.GetComponent<PigController>();
            var block = hit.GetComponent<BlockDamage>();
            pig?.TakeDamage(damage);
            block?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    // Poškodba ob trku
    // ─────────────────────────────────────────────
    private void ApplyContactDamage(Collision2D col, float speed)
    {
        float damage = baseDamage + speed * damageVelocityMultiplier;

        var pig = col.gameObject.GetComponent<PigController>();
        var block = col.gameObject.GetComponent<BlockDamage>();
        pig?.TakeDamage(damage);
        block?.TakeDamage(damage);
    }

    // ─────────────────────────────────────────────
    // Pomožne metode
    // ─────────────────────────────────────────────
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTrajectory();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (birdType == BirdType.Black)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    private Vector2 velocity;
    private float lifetime, elapsed;
    private SpriteRenderer sr;

    public void Init(Vector2 velocity, float lifetime)
    {
        this.velocity = velocity;
        this.lifetime = lifetime;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        velocity += Vector2.down * 9.8f * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);

        float alpha = t > 0.8f ? Mathf.Lerp(1f, 0f, (t - 0.8f) / 0.2f) : 1f;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        if (elapsed >= lifetime) Destroy(gameObject);
    }
}

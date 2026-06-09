using UnityEngine;

public class SmokeParticle : MonoBehaviour
{
    private Vector2 velocity;
    private float lifetime, elapsed, startScale, endScale;
    private SpriteRenderer sr;

    public void Init(Vector2 velocity, float lifetime, float startScale, float endScale)
    {
        this.velocity = velocity;
        this.lifetime = lifetime;
        this.startScale = startScale;
        this.endScale = endScale;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);
        float smooth = t * t * (3f - 2f * t); // smoothstep

        velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 2f);
        transform.position += (Vector3)(velocity * Time.deltaTime);

        float s = Mathf.Lerp(startScale, endScale, smooth);
        transform.localScale = Vector3.one * s;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(0.85f, 0f, smooth));

        if (elapsed >= lifetime) Destroy(gameObject);
    }
}

using UnityEngine;

public static class PigDeathVFXFactory
{
    public static GameObject CreatePrefab()
    {
        var root = new GameObject("PigDeathVFX");
        root.AddComponent<PigDeathVFX>();
        return root;
    }
}

public class PigDeathVFX : MonoBehaviour
{
    private void Start()
    {
        SpawnSmoke();
        SpawnBlood();
        Destroy(gameObject, 3f);
    }


    private void SpawnSmoke()
    {
        int count = Random.Range(6, 10);
        for (int i = 0; i < count; i++)
        {
            var puff = new GameObject("Smoke_" + i);
            puff.transform.SetParent(transform);
            puff.transform.position = transform.position;

            var sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite = MakeCircleSprite(32, new Color(0.6f, 0.6f, 0.6f, 0.85f));
            sr.sortingOrder = 10;

            float scale = Random.Range(0.3f, 0.7f);
            puff.transform.localScale = Vector3.one * scale;

            puff.AddComponent<SmokeParticle>().Init(
                velocity: new Vector2(Random.Range(-1.8f, 1.8f), Random.Range(1.5f, 3.5f)),
                lifetime: Random.Range(1.3f, 2.1f),
                startScale: scale,
                endScale: scale * 3.0f
            );
        }
    }


    private void SpawnBlood()
    {
        int count = Random.Range(10, 16);
        for (int i = 0; i < count; i++)
        {
            var drop = new GameObject("Blood_" + i);
            drop.transform.SetParent(transform);
            drop.transform.position = transform.position;

            var sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = MakeCircleSprite(16, new Color(0.75f, 0.0f, 0.05f, 1f));
            sr.sortingOrder = 11;

            float scale = Random.Range(0.08f, 0.22f);
            drop.transform.localScale = Vector3.one * scale;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(2.5f, 6f);
            drop.AddComponent<BloodParticle>().Init(
                velocity: new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                lifetime: Random.Range(0.85f, 1.5f)
            );
        }
    }


    private static Sprite MakeCircleSprite(int size, Color color)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - half + 0.5f;
                float dy = y - half + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((half - dist) / (half * 0.15f));
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }
}

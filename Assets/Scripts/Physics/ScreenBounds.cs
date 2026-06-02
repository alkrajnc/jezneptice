using UnityEngine;

/// <summary>
/// Uniči ali ustavi objekte ki zapustijo meje zaslona.
/// Dodaj na GameManager ali kak prazen GameObject v sceni.
/// </summary>
public class ScreenBounds : MonoBehaviour
{
    [Header("Meje (world units od centra kamere)")]
    [Tooltip("Kako daleč levo/desno sme objekt iti preden se uniči")]
    public float horizontalLimit = 20f;
    [Tooltip("Kako nizko sme objekt pasti preden se uniči")]
    public float bottomLimit = -10f;

    void Awake()
    {
        // Avtomatično nastavi meje glede na kamero če niso ročno nastavljene
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            horizontalLimit = Mathf.Max(horizontalLimit, halfW + 5f);
        }

        // Programsko izklopi kolizijo Bird ↔ BoundWall
        int birdLayer      = LayerMask.NameToLayer("Bird");
        int boundWallLayer = LayerMask.NameToLayer("BoundWall");
        if (birdLayer >= 0 && boundWallLayer >= 0)
            Physics2D.IgnoreLayerCollision(birdLayer, boundWallLayer, true);
    }

    void Update()
    {
        // Poišči vse dinamične objekte in jih uniči če so zunaj mej
        foreach (var rb in FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) continue;

            Vector2 pos = rb.position;
            if (pos.x < -horizontalLimit || pos.x > horizontalLimit || pos.y < bottomLimit)
            {
                Debug.Log($"[ScreenBounds] Objekt {rb.name} je zapustil meje — uničen.");
                Destroy(rb.gameObject);
            }
        }
    }
}

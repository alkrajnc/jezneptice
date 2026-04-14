using UnityEngine;

/// <summary>
/// Kamera sledi ptici med letom in povratku k frači.
/// Lahko je tudi fiksna na zgornjo levo kot v klasičnem Angry Birds.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform birdTransform;
    [SerializeField] private Vector3 offset = new Vector3(-2f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 minBounds = new Vector3(-5f, -2f, -10f);
    [SerializeField] private Vector3 maxBounds = new Vector3(15f, 8f, -10f);

    private Vector3 targetPosition;

    private void Start()
    {
        if (birdTransform == null)
        {
            Debug.LogWarning("Bird Transform ni nastavljen v CameraController!");
        }

        targetPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (birdTransform != null)
        {
            // Cilja na ptico + offset
            targetPosition = birdTransform.position + offset;

            // Omeji kamero na območje
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            targetPosition.z = minBounds.z;

            // Gladka animacija
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }

    /// <summary>
    /// Nastavi novo pozicijo s kamero na ptico
    /// </summary>
    public void SetBirdTarget(Transform newBird)
    {
        birdTransform = newBird;
    }
}

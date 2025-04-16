using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.right;
    public float moveSpeed = 1f;
    public float cloudLifetime = 10f;
    public float fadeDuration = 2f;

    private float timer = 0f;
    private Material cloudMaterial;
    private Color originalColor;

    void Start()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // This creates a unique instance of the material
            cloudMaterial = renderer.material;
            originalColor = cloudMaterial.color;
        }
    }

    void Update()
    {
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        float timeLeft = cloudLifetime - timer;

        if (cloudMaterial != null && timeLeft <= fadeDuration)
        {
            float alpha = Mathf.Clamp01(timeLeft / fadeDuration);
            cloudMaterial.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha
            );
        }

        if (timer >= cloudLifetime)
        {
            Destroy(gameObject);
        }
    }
}

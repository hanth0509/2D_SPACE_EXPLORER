using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("LASER SETTINGS")]
    public float speed = 15f;
    public float lifetime = 2f;
    public int damage = 1;

    [Header("EFFECTS")]
    public GameObject hitEffect;

    private Rigidbody2D rb;

    // GỌI MỖI KHI LASER ĐƯỢC KÍCH HOẠT LẠI TỪ POOL
    private void OnEnable()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Reset vận tốc rồi gán mới
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearVelocity = transform.up * speed;

        // Huỷ Invoke cũ rồi đặt lại thời gian tự hủy
        CancelInvoke();
        Invoke(nameof(Deactivate), lifetime);

        // Debug.Log($"Laser {name} activated, lifetime {lifetime}s");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        Debug.Log($"Laser hit: {other.gameObject.name}");

        if (other.CompareTag("Enemy") || other.CompareTag("Asteroid"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($" {other.gameObject.name} took {damage} damage");
            }
        }

        SpawnHitEffect();
        Deactivate();
    }

    private void SpawnHitEffect()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, transform.rotation);
    }

    public void Deactivate()
    {
        CancelInvoke();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        gameObject.SetActive(false);
        // Debug.Log($"Laser {name} deactivated");
    }

    // Gọi từ script bắn để set vị trí và hướng
    public void Activate(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
    }
}

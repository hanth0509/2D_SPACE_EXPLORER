using UnityEngine;

public class EnemyDespawn : MonoBehaviour
{
    [Header("DESPAWN SETTINGS")]
    public float maxDistanceFromPlayer = 25f;
    public float maxLifetime = 45f;

    private Transform player;
    private float spawnTime;

    void Start()
    {
        //TỰ ĐỘNG TÌM PLAYER
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        //LƯU THỜI GIAN SPAWN
        spawnTime = Time.time;

        Debug.Log("EnemyDespawn initialized");
    }

    void Update()
    {
        CheckDespawn();
    }

    void CheckDespawn()
    {
        // 1.KIỂM TRA THỜI GIAN
        if (Time.time - spawnTime > maxLifetime)
        {
            Despawn("Lifetime expired");
            return;
        }

        // 2.KIỂM TRA KHOẢNG CÁCH
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > maxDistanceFromPlayer)
            {
                Despawn("Too far from player");
                return;
            }
        }
    }

    void Despawn(string reason)
    {
        Debug.Log($"Despawning: {reason}");
        Destroy(gameObject);
    }

    /// Reset thời gian tồn tại (dùng khi respawn từ pool)
    public void ResetLifetime()
    {
        spawnTime = Time.time;
        Debug.Log("Asteroid lifetime reset");
    }
}
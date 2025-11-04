using UnityEngine;
using System.Collections.Generic;

public class Asteroid : MonoBehaviour
{
    [Header("ASTEROID PROPERTIES")]
    [Tooltip("Kích thước asteroid (ảnh hưởng đến health, điểm)")]
    public AsteroidSize size = AsteroidSize.Medium;

    [Tooltip("Điểm khi phá hủy asteroid này")]
    public int scoreValue = 10;

    [Header("REFERENCES")]
    [Tooltip("Prefab fragment khi asteroid vỡ")]
    public GameObject fragmentPrefab;

    [Header(" VISUAL SETTINGS")]
    [Tooltip("Danh sách sprites cho asteroid - kéo tất cả sprite variants vào đây")]
    public Sprite[] asteroidSprites;

    [Tooltip("Tự động chọn sprite ngẫu nhiên khi spawn")]
    public bool randomSpriteOnStart = true;

    [Tooltip("Tự động chọn màu ngẫu nhiên")]
    public bool randomColorOnStart = false;

    [Tooltip("Màu sắc có thể random (nếu bật random color)")]
    public Color[] possibleColors;
    [Header(" DESTRUCTION EFFECTS")]
    [Tooltip("Explosion effect prefab")]
    public GameObject explosionEffect;

    [Tooltip("Kích thước explosion dựa trên asteroid size")]
    public float explosionSizeMultiplier = 1f;
    // PRIVATE VARIABLES
    private Rigidbody2D rb;
    private Health health;

    public enum AsteroidSize
    {
        Small,      // Dễ phá hủy, ít điểm
        Medium,     // Trung bình
        Large       // Khó phá hủy, nhiều điểm
    }

    void Start()
    {
        InitializeAsteroid();
    }

    /// Khởi tạo asteroid
    void InitializeAsteroid()
    {
        //  LẤY COMPONENTS
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        //  KHỞI TẠO VISUAL TRƯỚC
        InitializeVisuals();

        //  SETUP HEALTH THEO SIZE
        SetupHealthBySize();

        // KHỞI TẠO CHUYỂN ĐỘNG
        // InitializeMovement();

        // ĐĂNG KÝ SỰ KIỆN
        if (health != null)
        {
            health.OnDeath += OnAsteroidDestroyed;
        }

        Debug.Log($" Asteroid spawned: {size}, Health: {health.GetCurrentHealth()}");
    }

    /// Thiết lập health dựa trên kích thước asteroid
    void SetupHealthBySize()
    {
        if (health != null)
        {
            switch (size)
            {
                case AsteroidSize.Small:
                    health.SetMaxHealth(1);
                    scoreValue = 5;
                    break;
                case AsteroidSize.Medium:
                    health.SetMaxHealth(2);
                    scoreValue = 10;
                    break;
                case AsteroidSize.Large:
                    health.SetMaxHealth(3);
                    scoreValue = 20;
                    break;
            }
        }
    }
    /// Khởi tạo visual ngẫu nhiên cho asteroid
    void InitializeVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        //CHỌN SPRITE NGẪU NHIÊN
        if (randomSpriteOnStart && asteroidSprites != null && asteroidSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, asteroidSprites.Length);
            sr.sprite = asteroidSprites[randomIndex];
            // Debug.Log($"Asteroid sprite: {randomIndex + 1}/{asteroidSprites.Length}");
        }

        //CHỌN MÀU NGẪU NHIÊN
        if (randomColorOnStart && possibleColors != null && possibleColors.Length > 0)
        {
            Color randomColor = possibleColors[Random.Range(0, possibleColors.Length)];
            sr.color = randomColor;
        }

        // TỰ ĐỘNG ĐIỀU CHỈNH COLLIDER THEO SPRITE
        UpdateColliderForSprite();
    }

    /// Cập nhật collider để khớp với sprite mới
    void UpdateColliderForSprite()
    {
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (polyCollider != null && sr != null && sr.sprite != null)
        {
            Sprite sprite = sr.sprite;
            int shapeCount = sprite.GetPhysicsShapeCount();

            polyCollider.pathCount = shapeCount;

            List<Vector2> path = new List<Vector2>();
            for (int i = 0; i < shapeCount; i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                polyCollider.SetPath(i, path);
            }
        }
    }


    void Update()
    {
        // HandleMovement();
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        //XỬ LÝ VA CHẠM VỚI PLAYER
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Asteroid hit Player!");

            // GÂY SÁT THƯƠNG CHO PLAYER
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            // HIỆU ỨNG VA CHẠM
            SpawnCollisionEffect();
        }
    }

    /// Khi asteroid bị phá hủy
    void OnAsteroidDestroyed()
    {
        // Debug.Log($"Asteroid destroyed! Size: {size}, Score: {scoreValue}");

        //HIỆU ỨNG NỔ
        SpawnExplosionEffect();

        //SINH FRAGMENTS (nếu có)
        SpawnFragments();

        //THÊM ĐIỂM (sẽ implement scoring system sau)
        AddScore();

        // ÂM THANH (sẽ thêm sau)
    }

    /// Tạo hiệu ứng explosion
    void SpawnExplosionEffect()
    {
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            //  ĐIỀU CHỈNH KÍCH THƯỚC EXPLOSION THEO ASTEROID SIZE
            float sizeMultiplier = 1f;
            switch (size)
            {
                case AsteroidSize.Small: sizeMultiplier = 0.7f; break;
                case AsteroidSize.Medium: sizeMultiplier = 1f; break;
                case AsteroidSize.Large: sizeMultiplier = 1.5f; break;
            }

            explosion.transform.localScale = Vector3.one * sizeMultiplier * explosionSizeMultiplier;

            //  ĐIỀU CHỈNH MÀU SẮC THEO SIZE (tùy chọn)
            ExplosionEffect effectScript = explosion.GetComponent<ExplosionEffect>();
            if (effectScript != null)
            {
                effectScript.SetSize(sizeMultiplier);
            }
        }
        else
        {
            Debug.LogWarning(" No explosion effect assigned to asteroid!");
        }
    }

    /// Tạo fragments khi asteroid vỡ
    void SpawnFragments()
    {
        if (fragmentPrefab != null && size != AsteroidSize.Small)
        {
            int fragmentCount = size == AsteroidSize.Large ? 3 : 2;

            for (int i = 0; i < fragmentCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                Vector3 spawnPosition = transform.position + (Vector3)randomOffset;

                GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);

                // SET SIZE NHỎ HƠN CHO FRAGMENT
                Asteroid fragmentAsteroid = fragment.GetComponent<Asteroid>();
                if (fragmentAsteroid != null)
                {
                    fragmentAsteroid.size = size == AsteroidSize.Large ? AsteroidSize.Medium : AsteroidSize.Small;
                }
            }
        }
    }

    /// Tạo hiệu ứng va chạm
    void SpawnCollisionEffect()
    {
        // Có thể thêm spark effects hoặc small explosion
        Debug.Log("Collision effect spawned");
    }

    /// Thêm điểm cho player
    void AddScore()
    {
        // Sẽ implement với scoring system
        Debug.Log($"Score +{scoreValue}");
    }

    //PUBLIC METHODS

    /// Đặt kích thước asteroid
    public void SetSize(AsteroidSize newSize)
    {
        size = newSize;
        SetupHealthBySize();
    }

    /// Đẩy asteroid với lực
    public void Push(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    /// Lấy điểm số của asteroid
    public int GetScoreValue()
    {
        return scoreValue;
    }

    void OnDestroy()
    {
        // HỦY ĐĂNG KÝ SỰ KIỆN
        if (health != null)
        {
            health.OnDeath -= OnAsteroidDestroyed;
        }
    }
}
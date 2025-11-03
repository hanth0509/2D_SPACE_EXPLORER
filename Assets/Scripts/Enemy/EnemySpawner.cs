using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("SPAWN SETTINGS")]
    [Tooltip("Prefab asteroid để spawn")]
    public GameObject asteroidPrefab;

    [Tooltip("Số asteroid tối đa trên map")]
    public int maxAsteroids = 6;

    [Tooltip("Thời gian giữa các lần spawn (giây)")]
    public float spawnInterval = 3f;

    [Tooltip("Khoảng cách tối thiểu từ player khi spawn")]
    public float minDistanceFromPlayer = 8f;

    [Tooltip("Khoảng cách tối đa từ player khi spawn")]
    public float maxDistanceFromPlayer = 20f;

    [Header("SPAWN BOUNDARIES")]
    [Tooltip("Sử dụng map boundaries để spawn")]
    public bool useMapBoundaries = true;

    [Tooltip("Kích thước spawn area nếu không dùng map boundaries")]
    public float spawnAreaWidth = 30f;
    public float spawnAreaHeight = 20f;

    [Header("ASTEROID VARIETY")]
    [Tooltip("Tự động random size khi spawn")]
    public bool randomizeSize = true;

    [Tooltip("Tỷ lệ phần trăm cho mỗi size (Small, Medium, Large)")]
    public float[] sizeChances = new float[] { 30f, 50f, 20f }; // 30% small, 50% medium, 20% large

    [Header("REFERENCES")]
    [Tooltip("Player transform (tự động tìm)")]
    public Transform player;

    [Tooltip("Map boundary reference (tùy chọn)")]
    public MapBoundary mapBoundary;

    // PRIVATE VARIABLES
    private List<GameObject> activeAsteroids = new List<GameObject>();
    private Queue<GameObject> asteroidPool = new Queue<GameObject>();
    private float nextSpawnTime;
    private int poolSize = 20;

    void Start()
    {
        InitializeSpawner();
    }

    /// Khởi tạo spawn system
    void InitializeSpawner()
    {
        //TỰ ĐỘNG TÌM PLAYER
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        //TỰ ĐỘNG TÌM MAP BOUNDARY
        if (useMapBoundaries && mapBoundary == null)
        {
            mapBoundary = FindObjectOfType<MapBoundary>();
        }

        //KHỞI TẠO OBJECT POOL
        InitializeObjectPool();

        //SET THỜI GIAN SPAWN ĐẦU TIÊN
        nextSpawnTime = Time.time + spawnInterval;

        Debug.Log($"EnemySpawner initialized: MaxAsteroids={maxAsteroids}, Interval={spawnInterval}s");
        Debug.Log($" Player: {(player != null ? player.name : "None")}");
    }

    void Update()
    {
        HandleSpawning();
        CleanupDestroyedAsteroids();
    }

    /// Khởi tạo object pool cho asteroids
    void InitializeObjectPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject asteroid = Instantiate(asteroidPrefab);
            asteroid.SetActive(false);
            asteroidPool.Enqueue(asteroid);

            // Đặt tên cho dễ debug
            asteroid.name = $"Asteroid_Pool_{i:00}";
        }

        Debug.Log($" Object pool created: {poolSize} asteroids");
    }

    /// Xử lý spawn asteroids
    void HandleSpawning()
    {
        // KIỂM TRA THỜI GIAN SPAWN
        if (Time.time < nextSpawnTime) return;

        // KIỂM TRA SỐ LƯỢNG ASTEROIDS HIỆN TẠI
        if (activeAsteroids.Count >= maxAsteroids) return;

        // THỰC HIỆN SPAWN
        SpawnAsteroid();

        // SET THỜI GIAN SPAWN TIẾP THEO
        nextSpawnTime = Time.time + spawnInterval;
    }

    /// Spawn một asteroid mới
    void SpawnAsteroid()
    {
        // LẤY ASTEROID TỪ POOL
        GameObject asteroid = GetPooledAsteroid();
        if (asteroid == null) return;

        // TÍNH VỊ TRÍ SPAWN
        Vector3 spawnPosition = CalculateSpawnPosition();
        if (spawnPosition == Vector3.zero) return; // Không tìm được vị trí phù hợp

        // KÍCH HOẠT ASTEROID
        asteroid.transform.position = spawnPosition;
        asteroid.transform.rotation = Quaternion.identity;
        asteroid.SetActive(true);

        // SETUP ASTEROID PROPERTIES
        SetupAsteroid(asteroid);

        // THÊM VÀO DANH SÁCH ACTIVE
        activeAsteroids.Add(asteroid);

        // Debug.Log($"Spawned asteroid at {spawnPosition}. Active: {activeAsteroids.Count}/{maxAsteroids}");
    }

    /// Lấy asteroid từ pool
    GameObject GetPooledAsteroid()
    {
        if (asteroidPool.Count > 0)
        {
            return asteroidPool.Dequeue();
        }

        // NẾU HẾT POOL, TẠO THÊM
        GameObject newAsteroid = Instantiate(asteroidPrefab);
        newAsteroid.SetActive(false);
        Debug.Log("Expanded asteroid pool");

        return newAsteroid;
    }

    /// Tính vị trí spawn - CHỈ Ở RÌA PHẢI MÀN HÌNH
    Vector3 CalculateSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        Vector3 spawnPosition = Vector3.zero;
        int attempts = 0;
        int maxAttempts = 10;

        // THỬ NHIỀU LẦN ĐỂ TÌM VỊ TRÍ PHÙ HỢP
        while (attempts < maxAttempts)
        {
            attempts++;

            // SPAWN Ở RÌA PHẢI MÀN HÌNH
            spawnPosition = GetPositionAtRightEdge();

            // KIỂM TRA KHOẢNG CÁCH TỪ PLAYER
            float distanceToPlayer = Vector2.Distance(spawnPosition, player.position);
            if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= maxDistanceFromPlayer)
            {
                // KIỂM TRA THÊM: KHÔNG SPAWN QUÁ GẦN ASTEROID KHÁC
                if (!IsTooCloseToOtherAsteroids(spawnPosition))
                {
                    return spawnPosition;
                }
            }
        }

        Debug.LogWarning("Could not find valid spawn position after multiple attempts");
        return Vector3.zero;
    }

    /// Giữ vị trí trong boundaries
    Vector3 ClampPositionToBounds(Vector3 position)
    {
        if (useMapBoundaries)
        {
            // Sử dụng spawn area boundaries
            position.x = Mathf.Clamp(position.x, -spawnAreaWidth / 2 + 2f, spawnAreaWidth / 2 - 2f);
            position.y = Mathf.Clamp(position.y, -spawnAreaHeight / 2 + 2f, spawnAreaHeight / 2 - 2f);
        }
        return position;
    }

    /// Lấy vị trí spawn Ở RÌA PHẢI MÀN HÌNH
    Vector3 GetPositionAtRightEdge()
    {
        //  LẤY CAMERA ĐỂ TÍNH MÀN HÌNH
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return Vector3.zero;

        //  TÍNH RÌA PHẢI MÀN HÌNH THEO WORLD COORDINATES
        float screenRightEdge = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

        //  RANDOM VỊ TRÍ THEO CHIỀU DỌC
        float randomViewportY = Random.Range(0.1f, 0.9f); // Tránh spawn quá sát trên/dưới
        float spawnY = gameCamera.ViewportToWorldPoint(new Vector3(0, randomViewportY, 0)).y;

        // VỊ TRÍ SPAWN (rìa phải + offset nhỏ để không thấy spawn)
        Vector3 spawnPosition = new Vector3(screenRightEdge + 1f, spawnY, 0);

        return spawnPosition;
    }

    /// Kiểm tra không spawn quá gần asteroid khác
    bool IsTooCloseToOtherAsteroids(Vector3 position)
    {
        float minAsteroidDistance = 3f; // Khoảng cách tối thiểu giữa các asteroids

        foreach (var asteroid in activeAsteroids)
        {
            if (asteroid != null && asteroid.activeInHierarchy)
            {
                float distance = Vector2.Distance(position, asteroid.transform.position);
                if (distance < minAsteroidDistance)
                {
                    return true; // Quá gần
                }
            }
        }
        return false; // Đủ xa
    }

    /// Lấy vị trí ngẫu nhiên trong map boundaries
    Vector3 GetRandomPositionInBounds()
    {
        // GIẢ ĐỊNH MAP BOUNDARIES (có thể cải tiến sau)
        float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        float y = Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2);

        return new Vector3(x, y, 0);
    }

    /// Lấy vị trí ngẫu nhiên trong spawn area
    Vector3 GetRandomPositionInArea()
    {
        float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        float y = Random.Range(-spawnAreaHeight / 2, spawnAreaHeight / 2);

        return new Vector3(x, y, 0);
    }

    /// Thiết lập properties cho asteroid
    void SetupAsteroid(GameObject asteroid)
    {
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();
        if (asteroidScript != null && randomizeSize)
        {
            // CHỌN SIZE NGẪU NHIÊN THEO TỶ LỆ
            Asteroid.AsteroidSize randomSize = GetRandomSize();
            asteroidScript.SetSize(randomSize);
        }

        // RESET DESPAWN TIMER (nếu có)
        EnemyDespawn despawn = asteroid.GetComponent<EnemyDespawn>();
        if (despawn != null)
        {
            despawn.ResetLifetime();
        }
    }

    /// Lấy size ngẫu nhiên theo tỷ lệ
    Asteroid.AsteroidSize GetRandomSize()
    {
        float randomValue = Random.Range(0f, 100f);
        float currentSum = 0f;

        for (int i = 0; i < sizeChances.Length; i++)
        {
            currentSum += sizeChances[i];
            if (randomValue <= currentSum)
            {
                return (Asteroid.AsteroidSize)i;
            }
        }

        return Asteroid.AsteroidSize.Medium; // Fallback
    }

    /// Dọn dẹp asteroids đã bị destroy
    void CleanupDestroyedAsteroids()
    {
        for (int i = activeAsteroids.Count - 1; i >= 0; i--)
        {
            if (activeAsteroids[i] == null || !activeAsteroids[i].activeInHierarchy)
            {
                // TRẢ VỀ POOL NẾU CÒN TỒN TẠI
                if (activeAsteroids[i] != null)
                {
                    asteroidPool.Enqueue(activeAsteroids[i]);
                }

                activeAsteroids.RemoveAt(i);
            }
        }
    }

    // PUBLIC METHODS

    /// Spawn asteroid ngay lập tức
    public void SpawnImmediate()
    {
        SpawnAsteroid();
    }

    /// Thay đổi spawn rate
    public void SetSpawnRate(float newInterval, int newMaxAsteroids)
    {
        spawnInterval = newInterval;
        maxAsteroids = newMaxAsteroids;
        Debug.Log($"Spawn rate changed: Interval={newInterval}s, Max={newMaxAsteroids}");
    }

    /// Dừng spawn system
    public void StopSpawning()
    {
        spawnInterval = Mathf.Infinity;
        Debug.Log("Spawning stopped");
    }

    /// Tiếp tục spawn system
    public void ResumeSpawning(float interval = 3f)
    {
        spawnInterval = interval;
        nextSpawnTime = Time.time + interval;
        Debug.Log($"Spawning resumed: Interval={interval}s");
    }

    /// Lấy số asteroid đang active
    public int GetActiveAsteroidCount()
    {
        return activeAsteroids.Count;
    }

    //  DEBUG GIZMOS
    void OnDrawGizmosSelected()
    {
        // VÙNG SPAWN
        Gizmos.color = Color.cyan;
        if (useMapBoundaries && mapBoundary != null)
        {
            // Vẽ map boundaries nếu có
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnAreaWidth, spawnAreaHeight, 0));
        }
        else
        {
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnAreaWidth, spawnAreaHeight, 0));
        }

        // KHOẢNG CÁCH SPAWN TỪ PLAYER
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        }
    }
}
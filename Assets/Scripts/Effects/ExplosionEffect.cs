using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header(" EXPLOSION SETTINGS")]
    [Tooltip("Thời gian tồn tại của effect (giây)")]
    public float lifetime = 1.5f;
    
    [Tooltip("Kích thước tối đa của explosion")]
    public float maxSize = 2f;
    
    [Tooltip("Tốc độ phát triển của explosion")]
    public float growthSpeed = 3f;
    
    [Tooltip("Có tự động destroy sau lifetime không?")]
    public bool autoDestroy = true;

    [Header(" VISUAL EFFECTS")]
    [Tooltip("Particle systems cho explosion")]
    public ParticleSystem[] explosionParticles;
    
    [Tooltip("Light effect cho explosion")]
    public Light explosionLight;
    
    [Tooltip("Sprite renderer cho flash effect")]
    public SpriteRenderer flashSprite;

    //  PRIVATE VARIABLES
    private float spawnTime;
    private bool isGrowing = true;
    private Vector3 initialScale;

    void Start()
    {
        InitializeExplosion();
    }

    /// Khởi tạo explosion effect
    void InitializeExplosion()
    {
        spawnTime = Time.time;
        initialScale = transform.localScale;
        transform.localScale = Vector3.zero; // Bắt đầu từ size 0
        
        //  PLAY TẤT CẢ PARTICLE SYSTEMS
        if (explosionParticles != null)
        {
            foreach (ParticleSystem ps in explosionParticles)
            {
                if (ps != null) ps.Play();
            }
        }
        
        //  BẬT LIGHT EFFECT
        if (explosionLight != null)
        {
            explosionLight.enabled = true;
        }
        
        //  BẬT FLASH SPRITE
        if (flashSprite != null)
        {
            flashSprite.enabled = true;
        }
        
        Debug.Log(" Explosion effect initialized");
    }

    void Update()
    {
        HandleExplosionGrowth();
        
        // TỰ ĐỘNG DESTROY SAU LIFETIME
        if (autoDestroy && Time.time - spawnTime >= lifetime)
        {
            DestroyExplosion();
        }
    }

    /// Xử lý sự phát triển của explosion
    void HandleExplosionGrowth()
    {
        if (isGrowing)
        {
            //  PHÓNG TO EXPLOSION
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale * maxSize, growthSpeed * Time.deltaTime);
            
            //  KIỂM TRA ĐẠT KÍCH THƯỚC TỐI ĐA
            if (transform.localScale.magnitude >= initialScale.magnitude * maxSize * 0.9f)
            {
                isGrowing = false;
                StartShrinkPhase();
            }
        }
    }

    /// Bắt đầu phase thu nhỏ
    void StartShrinkPhase()
    {
        //  TẮT LIGHT SAU KHI ĐẠT PEAK
        if (explosionLight != null)
        {
            explosionLight.enabled = false;
        }
        
        //  TẮT FLASH SPRITE
        if (flashSprite != null)
        {
            flashSprite.enabled = false;
        }
    }

    /// Hủy explosion effect
    void DestroyExplosion()
    {
        // STOP TẤT CẢ PARTICLE SYSTEMS
        if (explosionParticles != null)
        {
            foreach (ParticleSystem ps in explosionParticles)
            {
                if (ps != null) ps.Stop();
            }
        }
        
        // DESTROY GAMEOBJECT
        Destroy(gameObject);
    }

    // PUBLIC METHODS

    /// Kích hoạt explosion tại vị trí
    public static void CreateExplosion(Vector3 position, float size = 1f)
    {
        // Tìm explosion prefab trong Resources folder
        GameObject explosionPrefab = Resources.Load<GameObject>("Explosion");
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * size;
        }
        else
        {
            Debug.LogWarning(" Explosion prefab not found in Resources folder!");
        }
    }

    /// Đặt kích thước explosion
    public void SetSize(float size)
    {
        maxSize = size;
        initialScale = Vector3.one * size;
    }

    /// Thêm screen shake (sẽ implement với camera system)
    public void AddScreenShake(float intensity = 0.5f)
    {
        // Có thể integrate với Cinemachine hoặc custom camera shake
        Debug.Log($" Screen shake intensity: {intensity}");
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerShooting : MonoBehaviour
{
    [Header("SHOOTING SETTINGS")]
    [Tooltip("Prefab laser để bắn - kéo LaserProjectile prefab vào đây")]
    public GameObject laserPrefab;

    [Tooltip("Tốc độ bắn (số viên mỗi giây)")]
    public float fireRate = 5f;

    [Tooltip("Điểm bắn - nơi laser xuất hiện")]
    public Transform firePoint;

    [Header("OBJECT POOLING SETTINGS")]
    [Tooltip("Số lượng laser tối đa trong pool")]
    public int poolSize = 20;

    [Header("AUDIO & EFFECTS")]
    [Tooltip("Audio Source cho âm thanh bắn")]
    public AudioSource shootAudioSource;

    [Tooltip("Clip âm thanh khi bắn")]
    public AudioClip shootSound;

    [Tooltip("Hiệu ứng khi bắn (tùy chọn)")]
    public ParticleSystem shootEffect;

    // PRIVATE VARIABLES
    private List<GameObject> laserPool;          // Danh sách laser trong pool
    private float nextFireTime;                  // Thời gian được phép bắn tiếp
    private bool isFiring;                       // Đang bắn hay không

    void Start()
    {
        InitializeShootingSystem();
    }

    /// Khởi tạo toàn bộ hệ thống bắn
    void InitializeShootingSystem()
    {
        // TẠO FIREPOINT NẾU CHƯA CÓ
        if (firePoint == null)
        {
            CreateFirePoint();
        }

        // KHỞI TẠO OBJECT POOL
        InitializeLaserPool();

        // SETUP AUDIO
        InitializeAudio();

        //  XÁC NHẬN HOÀN THÀNH
        Debug.Log("Player shooting system initialized!");
        Debug.Log($"Laser pool: {poolSize} lasers");
        Debug.Log($"Fire rate: {fireRate} shots/second");
    }

    /// <summary>
    /// Tạo điểm bắn tự động nếu chưa có
    /// </summary>
    void CreateFirePoint()
    {
        // TẠO GAMEOBJECT MỚI CHO FIREPOINT
        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(transform); // Đặt làm con của Player

        //  VỊ TRÍ: phía trước tàu spaceship
        firePointObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        firePointObj.transform.localRotation = Quaternion.identity;

        firePoint = firePointObj.transform;

        Debug.Log(" Auto-created FirePoint at: " + firePoint.localPosition);
    }

    /// Khởi tạo Object Pool cho laser
    void InitializeLaserPool()
    {
        laserPool = new List<GameObject>();

        // TẠO TRƯỚC TẤT CẢ LASER TRONG POOL
        for (int i = 0; i < poolSize; i++)
        {
            GameObject laser = Instantiate(laserPrefab);
            laser.SetActive(false); // BAN ĐẦU ẨN
            laserPool.Add(laser);

            // ĐẶT TÊN CHO DỄ DEBUG
            laser.name = $"Laser_{i:00}";
        }

        Debug.Log($"Created laser pool with {laserPool.Count} lasers");
    }

    /// Khởi tạo hệ thống âm thanh
    void InitializeAudio()
    {
        if (shootAudioSource != null)
        {
            shootAudioSource.playOnAwake = false;
            shootAudioSource.loop = false;

            // 🎵 SET CLIP NẾU CÓ
            if (shootSound != null)
            {
                shootAudioSource.clip = shootSound;
            }
        }
        else
        {
            Debug.LogWarning(" Shoot Audio Source reference is missing!");
        }
    }

    // INPUT SYSTEM: Nhận input bắn từ người chơi
    public void OnFire(InputAction.CallbackContext context)
    {
        // CHỈ BẮN KHI NHẤN XUỐNG (KHÔNG PHẢI THẢ RA)
        if (context.performed)
        {
            isFiring = true;
            TryShoot();
        }
        else if (context.canceled)
        {
            isFiring = false;
        }
    }

    void Update()
    {
        // AUTO-FIRE KHI GIỮ PHÍM
        HandleAutoFire();
    }

    /// Xử lý bắn tự động khi giữ phím
    void HandleAutoFire()
    {
        if (isFiring && Time.time >= nextFireTime)
        {
            TryShoot();
        }
    }

    /// Thử bắn (kiểm tra cooldown trước)
    void TryShoot()
    {
        // KIỂM TRA COOLDOWN
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate; // Tính thời gian bắn tiếp
        }
    }

    /// Thực hiện bắn laser
    void Shoot()
    {
        // LẤY LASER TỪ POOL
        GameObject laser = GetPooledLaser();

        if (laser != null)
        {
            // KÍCH HOẠT LASER
            LaserProjectile laserScript = laser.GetComponent<LaserProjectile>();
            if (laserScript != null)
            {
                laserScript.Activate(firePoint.position, firePoint.rotation);
            }
            else
            {
                // FALLBACK NẾU KHÔNG CÓ SCRIPT
                laser.transform.position = firePoint.position;
                laser.transform.rotation = firePoint.rotation;
                laser.SetActive(true);
            }
            Debug.Log($"🔫 Laser fired! Active lasers: {GetActiveLaserCount()}/{GetTotalLaserCount()}");
            // ÂM THANH BẮN
            PlayShootSound();

            // HIỆU ỨNG BẮN (nếu có)
            PlayShootEffect();

            Debug.Log("Laser fired!");
        }
        else
        {
            Debug.LogWarning("No available lasers in pool!");
        }
    }

    /// Lấy laser từ pool (hoặc tạo mới nếu cần)
    GameObject GetPooledLaser()
    {
        // TÌM LASER ĐANG KHÔNG ACTIVE
        foreach (GameObject laser in laserPool)
        {
            if (!laser.activeInHierarchy)
                return laser;
        }

        // NẾU KHÔNG CÒN LASER, TẠO THÊM (AUTO-EXPAND)
        GameObject newLaser = Instantiate(laserPrefab);
        newLaser.SetActive(false);
        laserPool.Add(newLaser);
        newLaser.name = $"Laser_Extra_{laserPool.Count}";

        Debug.Log($"Laser pool expanded to {laserPool.Count} lasers");
        return newLaser;
    }

    /// Phát âm thanh khi bắn
    void PlayShootSound()
    {
        if (shootAudioSource != null && shootSound != null)
        {
            shootAudioSource.PlayOneShot(shootSound);
        }
        else if (shootAudioSource != null && shootAudioSource.clip != null)
        {
            shootAudioSource.Play();
        }
        else
        {
            // FALLBACK: Tạo audio source tạm thời
            AudioSource.PlayClipAtPoint(shootSound, transform.position, 0.3f);
        }
    }

    /// Hiệu ứng khi bắn (particles)
    void PlayShootEffect()
    {
        if (shootEffect != null)
        {
            shootEffect.Play();
        }
    }


    /// Nâng cấp tốc độ bắn
    public void UpgradeFireRate(float multiplier)
    {
        float oldRate = fireRate;
        fireRate *= multiplier;
        Debug.Log($"Fire rate upgraded: {oldRate} → {fireRate}");
    }

    /// Đổi loại đạn (sau này)
    public void ChangeWeapon(GameObject newLaserPrefab)
    {
        laserPrefab = newLaserPrefab;
        // Có thể reset pool ở đây nếu cần
        Debug.Log("Weapon changed!");
    }

    /// Lấy số laser đang active
    public int GetActiveLaserCount()
    {
        int count = 0;
        foreach (GameObject laser in laserPool)
        {
            if (laser.activeInHierarchy)
                count++;
        }
        return count;
    }

    /// Lấy tổng số laser trong pool
    public int GetTotalLaserCount()
    {
        return laserPool.Count;
    }

    /// Reset hệ thống bắn (cho restart game)
    public void ResetShooting()
    {
        // DEACTIVATE TẤT CẢ LASER
        foreach (GameObject laser in laserPool)
        {
            laser.SetActive(false);
        }

        // RESET COOLDOWN
        nextFireTime = 0f;
        isFiring = false;

        Debug.Log("Shooting system reset!");
    }
}
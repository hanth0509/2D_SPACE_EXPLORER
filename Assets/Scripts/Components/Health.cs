using UnityEngine;

public class Health : MonoBehaviour
{
    [Header(" HEALTH SETTINGS")]
    [Tooltip("Máu tối đa của GameObject")]
    public int maxHealth = 3;

    [Tooltip("Máu hiện tại")]
    public int currentHealth;

    [Header(" DEATH SETTINGS")]
    [Tooltip("Có hủy GameObject khi chết không?")]
    public bool destroyOnDeath = true;

    [Tooltip("Thời gian trước khi hủy (giây)")]
    public float deathDelay = 0f;

    // SỰ KIỆN - để các script khác lắng nghe
    public System.Action OnDamageTaken;    // Khi nhận sát thương
    public System.Action OnDeath;          // Khi chết
    public System.Action OnHealthChanged;  // Khi máu thay đổi

    void Start()
    {
        InitializeHealth();
    }

    /// Khởi tạo máu khi bắt đầu
    void InitializeHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($" {gameObject.name} health initialized: {currentHealth}/{maxHealth}");
    }

    /// Nhận sát thương
    public void TakeDamage(int damageAmount)
    {
        // KIỂM TRA ĐÃ CHẾT CHƯA
        if (currentHealth <= 0) return;

        //  TRỪ MÁU
        int previousHealth = currentHealth;
        currentHealth -= damageAmount;

        // ĐẢM BẢO MÁU KHÔNG ÂM
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        //  THÔNG BÁO SỰ KIỆN
        OnDamageTaken?.Invoke();
        OnHealthChanged?.Invoke();

        //  KIỂM TRA CHẾT
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// Hồi máu
    public void Heal(int healAmount)
    {
        // TĂNG MÁU
        int previousHealth = currentHealth;
        currentHealth += healAmount;

        // GIỚI HẠN MÁU TỐI ĐA
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");

        // THÔNG BÁO SỰ KIỆN
        OnHealthChanged?.Invoke();
    }

    /// Chết
    void Die()
    {
        Debug.Log($"{gameObject.name} destroyed!");
        if (CompareTag("Enemy"))
        {
            GameManager.Instance.AddScore(50);
        }
        //  THÔNG BÁO SỰ KIỆN CHẾT
        OnDeath?.Invoke();

        //  XỬ LÝ HỦY GAMEOBJECT
        if (destroyOnDeath)
        {
            if (deathDelay > 0)
            {
                // HỦY SAU KHOẢNG THỜI GIAN
                Invoke("DestroyGameObject", deathDelay);
            }
            else
            {
                // HỦY NGAY LẬP TỨC
                DestroyGameObject();
            }
        }
        else
        {
            // CHỈ VÔ HIỆU HÓA (cho object pooling)
            gameObject.SetActive(false);
        }
    }

    /// Hủy GameObject
    void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    // PUBLIC METHODS - CÁC SCRIPT KHÁC CÓ THỂ GỌI

    /// Đặt lại máu về tối đa

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        Debug.Log($" {gameObject.name} health reset to {currentHealth}/{maxHealth}");
    }

    /// Đặt máu tối đa mới
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        OnHealthChanged?.Invoke();
    }

    /// Kiểm tra còn sống không
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    /// Lấy phần trăm máu (0-1)
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    /// Lấy máu hiện tại
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// Lấy máu tối đa
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
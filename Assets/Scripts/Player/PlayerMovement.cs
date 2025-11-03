using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("MOVEMENT SETTINGS")]
    [Tooltip("Tốc độ di chuyển của spaceship")]
    public float moveSpeed = 8f;

    [Tooltip("Tốc độ xoay của spaceship")]
    public float rotationSpeed = 180f;

    [Tooltip("Vận tốc tối đa - không vượt quá giá trị này")]
    public float maxVelocity = 10f;

    [Header("COMPONENT REFERENCES")]
    [Tooltip("Tham chiếu đến Rigidbody2D - kéo thả từ Inspector")]
    public Rigidbody2D rb;

    [Header("BOOSTER EFFECTS")]
    [Tooltip("Particle System cho hiệu ứng booster")]
    public ParticleSystem boosterParticles;

    [Tooltip("Tốc độ phát hạt tối đa khi boost")]
    public float boosterEmissionRate = 50f;

    [Tooltip("Tốc độ phát hạt tối thiểu")]
    public float minBoosterRate = 5f;

    [Header("AUDIO SETTINGS")]
    [Tooltip("Audio Source cho âm thanh động cơ")]
    public AudioSource engineAudioSource;

    [Tooltip("Volume tối đa của động cơ")]
    public float maxEngineVolume = 0.3f;

    [Tooltip("Volume tối thiểu khi di chuyển")]
    public float minEngineVolume = 0.1f;

    [Tooltip("Volume khi không di chuyển")]
    public float idleEngineVolume = 0.05f;

    [Tooltip("Độ thay đổi pitch của động cơ")]
    public float enginePitchRange = 0.5f;

    // Biến theo dõi trạng thái audio
    private bool wasMoving = false;

    // Biến private để điều khiển emission
    private ParticleSystem.EmissionModule boosterEmission;
    // BIẾN NỘI BỘ
    private Vector2 movementInput;    // Lưu input từ người chơi
    private bool isMoving;           // Theo dõi trạng thái di chuyển

    void Start()
    {
        InitializeComponents();
        InitializeBoosterEffects();
        InitializeAudio();
    }

    /// Khởi tạo các components - chạy một lần khi game bắt đầu
    void InitializeComponents()
    {
        // TỰ ĐỘNG TÌM RIGIDBODY2D NẾU CHƯA GÁN
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            Debug.Log("Auto-found Rigidbody2D component");
        }

        // KIỂM TRA XEM CÓ RIGIDBODY2D KHÔNG
        if (rb == null)
        {
            Debug.LogError("PlayerMovement: Không tìm thấy Rigidbody2D!");
        }
        else
        {
            Debug.Log("PlayerMovement initialized successfully!");
        }
    }
    void InitializeBoosterEffects()
    {
        // SETUP BOOSTER PARTICLES
        if (boosterParticles != null)
        {
            boosterEmission = boosterParticles.emission;
            boosterEmission.rateOverTime = 0f; // Tắt ban đầu
            boosterParticles.transform.localRotation = Quaternion.Euler(0, 0, -90f);  // Xoay booster cho phụt sang trái
            Debug.Log("Booster effects initialized");
        }
        else
        {
            Debug.LogWarning("BoosterParticles reference is missing!");
        }
    }

    void InitializeAudio()
    {
        if (engineAudioSource != null)
        {
            // SETUP AUDIO - KHÔNG TỰ PLAY
            engineAudioSource.volume = 0f;
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;

            // BAN ĐẦU KHÔNG DI CHUYỂN
            wasMoving = false;

            Debug.Log("Engine audio initialized - will play only when moving");
        }
        else
        {
            Debug.LogWarning("EngineAudioSource reference is missing!");
        }
    }

    /// INPUT SYSTEM: Nhận input movement từ người chơi
    /// Được gọi tự động bởi Input System
    public void OnMovement(InputAction.CallbackContext context)
    {
        // ĐỌC GIÁ TRỊ INPUT (Vector2: x,y từ -1 đến 1)
        movementInput = context.ReadValue<Vector2>();

        // Debug để kiểm tra input
        // Debug.Log($"Movement Input: ({movementInput.x}, {movementInput.y})");
    }

    /// FixedUpdate: Xử lý physics - chạy với tần số cố định
    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        UpdateMovementState();
        UpdateBoosterEffects();
        UpdateEngineSound();
    }

    /// Xử lý di chuyển vật lý của spaceship
    void HandleMovement()
    {
        // KIỂM TRA CÓ INPUT DI CHUYỂN KHÔNG
        if (movementInput.magnitude > 0.1f)
        {
            // TÍNH LỰC DI CHUYỂN DỰA TRÊN INPUT VÀ TỐC ĐỘ
            Vector2 force = movementInput * moveSpeed;

            // ÁP DỤNG LỰC VÀO RIGIDBODY2D
            rb.AddForce(force);

            // GIỚI HẠN VẬN TỐC TỐI ĐA
            if (rb.linearVelocity.magnitude > maxVelocity)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
            }

            isMoving = true;
        }
        else
        {
            // KHÔNG CÓ INPUT: GIẢM DẦN VẬN TỐC
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.1f);
            isMoving = false;
        }
    }

    /// Xử lý xoay spaceship theo hướng di chuyển
    void HandleRotation()
    {
        
    }

    /// Cập nhật trạng thái di chuyển
    void UpdateMovementState()
    {
        // Có thể thêm logic khác ở đây nếu cần
    }

    // === PUBLIC METHODS - CÁC SCRIPT KHÁC CÓ THỂ GỌI ===

    /// Trả về vận tốc hiện tại của player
    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    /// Kiểm tra player có đang di chuyển không
    public bool IsMoving()
    {
        return isMoving;
    }

    /// Trả về tốc độ hiện tại dưới dạng phần trăm (0-1)
    public float GetSpeedPercentage()
    {
        return rb.linearVelocity.magnitude / maxVelocity;
    }

    /// Reset vận tốc và vị trí player (cho restart game)
    public void ResetPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    /// Cập nhật hiệu ứng booster dựa trên tốc độ
    void UpdateBoosterEffects()
    {
        if (boosterParticles != null)
        {
            if (isMoving)
            {
                // TÍNH TỐC ĐỘ PHÁT HẠT DỰA TRÊN TỐC ĐỘ DI CHUYỂN
                float speedFactor = rb.linearVelocity.magnitude / maxVelocity;
                float emissionRate = Mathf.Lerp(minBoosterRate, boosterEmissionRate, speedFactor);

                // ÁP DỤNG TỐC ĐỘ PHÁT HẠT
                boosterEmission.rateOverTime = emissionRate;

                // THAY ĐỔI MÀU BOOSTER THEO HƯỚNG DI CHUYỂN
                UpdateBoosterColor();
            }
            else
            {
                // TẮT BOOSTER KHI KHÔNG DI CHUYỂN
                boosterEmission.rateOverTime = 0f;
            }
        }
    }

    /// Thay đổi màu booster dựa trên hướng di chuyển
    void UpdateBoosterColor()
    {
        var main = boosterParticles.main;

        // MÀU XANH DƯƠNG KHI DI CHUYỂN THẲNG
        // MÀU XANH LÁ KHI DI CHUYỂN NGANG
        Color boosterColor = Color.Lerp(Color.blue, Color.green, Mathf.Abs(movementInput.x));
        main.startColor = boosterColor;
    }

    /// Cập nhật âm thanh động cơ dựa trên tốc độ
    void UpdateEngineSound()
    {
        if (engineAudioSource != null)
        {
            if (isMoving)
            {
                // BẮT ĐẦU DI CHUYỂN - BẬT AUDIO NẾU CHƯA PLAY
                if (!wasMoving)
                {
                    if (!engineAudioSource.isPlaying)
                    {
                        engineAudioSource.Play();
                    }
                    wasMoving = true;
                }

                // VOLUME VÀ PITCH THEO TỐC ĐỘ
                float speedFactor = rb.linearVelocity.magnitude / maxVelocity;
                engineAudioSource.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, speedFactor);
                engineAudioSource.pitch = Mathf.Lerp(1f, 1f + enginePitchRange, speedFactor);
            }
            else
            {
                // NGỪNG DI CHUYỂN - GIẢM VOLUME DẦN
                engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, idleEngineVolume, 0.1f);
                engineAudioSource.pitch = 1f;

                // NẾU ĐÃ DỪNG LÂU - TẮT AUDIO
                if (engineAudioSource.volume < 0.03f && engineAudioSource.isPlaying)
                {
                    engineAudioSource.Stop();
                    wasMoving = false;
                }
            }
        }
    }
}
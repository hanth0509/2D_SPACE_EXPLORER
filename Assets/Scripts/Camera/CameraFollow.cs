using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("TARGET SETTINGS")]
    [Tooltip("Player transform để camera follow - kéo Player từ Hierarchy vào đây")]
    public Transform target;
    
    [Header("CAMERA MOVEMENT")]
    [Tooltip("Tốc độ follow (0 = instant, 1 = rất chậm) - giá trị nhỏ = mượt hơn")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;
    
    [Tooltip("Offset từ player - chỉnh để camera không bị che player")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("CAMERA BOUNDS")]
    [Tooltip("Bật giới hạn di chuyển camera")]
    public bool useBounds = false;
    
    [Tooltip("Giới hạn trái/phải cho camera")]
    public float minX = -10f, maxX = 10f;
    
    [Tooltip("Giới hạn trên/dưới cho camera")]
    public float minY = -10f, maxY = 10f;
    
    [Header("MOVEMENT PREDICTION")]
    [Tooltip("Dự đoán chuyển động player để camera mượt hơn")]
    public bool usePrediction = true;
    
    [Tooltip("Độ mạnh của prediction - cao hơn = dự đoán xa hơn")]
    [Range(0f, 1f)]
    public float predictionStrength = 0.1f;

    // PRIVATE VARIABLES
    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Start()
    {
        InitializeCamera();
    }

    /// Khởi tạo camera system
    void InitializeCamera()
    {
        // LẤY CAMERA COMPONENT
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No Camera component found!");
            return;
        }

        // TỰ ĐỘNG TÌM PLAYER NẾU CHƯA GÁN
        if (target == null)
        {
            FindPlayerTarget();
        }

        // SET VỊ TRÍ BAN ĐẦU
        if (target != null)
        {
            Vector3 desiredPosition = CalculateTargetPosition();
            transform.position = desiredPosition;
        }

        Debug.Log("Camera Follow System initialized");
        Debug.Log($"Target: {(target != null ? target.name : "None")}");
        Debug.Log($"Smooth Speed: {smoothSpeed}");
        Debug.Log($"Prediction: {(usePrediction ? "ON" : "OFF")}");
    }

    /// Tự động tìm player trong scene
    void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("Auto-found player target: " + target.name);
        }
        else
        {
            Debug.LogWarning("No player found with tag 'Player'. Please assign target manually.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        FollowTarget();
    }

    /// Thực hiện follow target
    void FollowTarget()
    {
        // TÍNH VỊ TRÍ MỤC TIÊU
        Vector3 targetPosition = CalculateTargetPosition();
        
        // ÁP DỤNG SMOOTH FOLLOW
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothSpeed
        );
        
        transform.position = smoothedPosition;
    }

    /// Tính toán vị trí mục tiêu cho camera
    Vector3 CalculateTargetPosition()
    {
        Vector3 targetPosition = target.position + offset;
        
        // THÊM PREDICTION NẾU BẬT
        if (usePrediction)
        {
            targetPosition = ApplyPrediction(targetPosition);
        }
        
        // ÁP DỤNG BOUNDS NẾU BẬT
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        return targetPosition;
    }

    /// Áp dụng movement prediction
    Vector3 ApplyPrediction(Vector3 targetPosition)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // DỰ ĐOÁN VỊ TRÍ DỰA TRÊN VẬN TỐC
            Vector3 prediction = (Vector3)targetRb.linearVelocity * predictionStrength;
            targetPosition += prediction;
        }
        return targetPosition;
    }

    /// Áp dụng giới hạn camera
    Vector3 ApplyBounds(Vector3 targetPosition)
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        return targetPosition;
    }

    // PUBLIC METHODS - CÁC SCRIPT KHÁC CÓ THỂ GỌI

    /// Thay đổi target cho camera
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"Camera target changed to: {newTarget.name}");
    }

    /// Đặt giới hạn camera mới
    public void SetBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
        useBounds = true;
        
        Debug.Log($"Camera bounds set: X({minX},{maxX}) Y({minY},{maxY})");
    }

    /// Tắt giới hạn camera
    public void DisableBounds()
    {
        useBounds = false;
        Debug.Log("Camera bounds disabled");
    }

    /// Thay đổi tốc độ follow
    public void SetSmoothSpeed(float newSpeed)
    {
        smoothSpeed = Mathf.Clamp01(newSpeed);
        Debug.Log($"Smooth speed changed to: {smoothSpeed}");
    }

    /// Bật/tắt prediction
    public void SetPrediction(bool enabled, float strength = 0.1f)
    {
        usePrediction = enabled;
        predictionStrength = Mathf.Clamp01(strength);
        Debug.Log($"Prediction: {(enabled ? "ON" : "OFF")}, Strength: {predictionStrength}");
    }
}
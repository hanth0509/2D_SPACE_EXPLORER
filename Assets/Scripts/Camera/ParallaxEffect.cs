using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("PARALLAX SETTINGS")]
    [Tooltip("Cường độ parallax (0 = background tĩnh, 1 = di chuyển bằng camera)")]
    [Range(0f, 1f)]
    public float parallaxStrength = 0.5f;
    
    [Tooltip("Layer này có di chuyển theo trục X không?")]
    public bool moveHorizontal = true;
    
    [Tooltip("Layer này có di chuyển theo trục Y không?")]
    public bool moveVertical = false;
    
    [Header("INFINITE SCROLLING")]
    [Tooltip("Bật infinite scrolling cho background")]
    public bool infiniteScrolling = true;
    
    [Tooltip("Background có lặp lại không?")]
    public bool tilingEnabled = true;
    
    [Header("REFERENCES")]
    [Tooltip("Camera transform (tự động tìm nếu để trống)")]
    public Transform cameraTransform;
    
    [Tooltip("Player transform cho advanced effects (tùy chọn)")]
    public Transform playerTransform;

    // PRIVATE VARIABLES
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InitializeParallax();
    }

    /// Khởi tạo hệ thống parallax
    void InitializeParallax()
    {
        // TỰ ĐỘNG TÌM CAMERA
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError(" ParallaxEffect: No camera found!");
                return;
            }
        }
        
        // LẤY SPRITE RENDERER
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ParallaxEffect: No SpriteRenderer found!");
            return;
        }

        // TÍNH KÍCH THƯỚC TEXTURE CHO INFINITE SCROLLING
        if (spriteRenderer.sprite != null)
        {
            Texture2D texture = spriteRenderer.sprite.texture;
            textureUnitSizeX = texture.width / spriteRenderer.sprite.pixelsPerUnit;
            textureUnitSizeY = texture.height / spriteRenderer.sprite.pixelsPerUnit;
        }

        // LƯU VỊ TRÍ CAMERA BAN ĐẦU
        lastCameraPosition = cameraTransform.position;

        Debug.Log($"Parallax initialized: {gameObject.name}");
        Debug.Log($"Texture Size: ({textureUnitSizeX}, {textureUnitSizeY})");
        Debug.Log($"Strength: {parallaxStrength}");
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;
        
        UpdateParallax();
    }

    /// Cập nhật hiệu ứng parallax mỗi frame
    void UpdateParallax()
    {
        // TÍNH KHOẢNG DI CHUYỂN CỦA CAMERA
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // TÍNH PARALLAX MOVEMENT DỰA TRÊN STRENGTH
        float parallaxX = moveHorizontal ? deltaMovement.x * parallaxStrength : 0f;
        float parallaxY = moveVertical ? deltaMovement.y * parallaxStrength : 0f;
        
        // DI CHUYỂN BACKGROUND
        transform.position += new Vector3(parallaxX, parallaxY, 0f);
        
        // CẬP NHẬT VỊ TRÍ CAMERA
        lastCameraPosition = cameraTransform.position;

        // XỬ LÝ INFINITE SCROLLING
        if (infiniteScrolling && tilingEnabled)
        {
            HandleInfiniteScrolling();
        }
    }

    /// Xử lý infinite scrolling cho background
    void HandleInfiniteScrolling()
    {
        if (moveHorizontal)
        {
            // SCROLLING VÔ HẠN THEO TRỤC X
            float deltaX = cameraTransform.position.x - transform.position.x;
            if (Mathf.Abs(deltaX) >= textureUnitSizeX)
            {
                float offsetPositionX = (deltaX > 0) ? textureUnitSizeX : -textureUnitSizeX;
                transform.position = new Vector3(
                    transform.position.x + offsetPositionX, 
                    transform.position.y, 
                    transform.position.z
                );
            }
        }
        
        if (moveVertical)
        {
            // SCROLLING VÔ HẠN THEO TRỤC Y
            float deltaY = cameraTransform.position.y - transform.position.y;
            if (Mathf.Abs(deltaY) >= textureUnitSizeY)
            {
                float offsetPositionY = (deltaY > 0) ? textureUnitSizeY : -textureUnitSizeY;
                transform.position = new Vector3(
                    transform.position.x, 
                    transform.position.y + offsetPositionY, 
                    transform.position.z
                );
            }
        }
    }

    // PUBLIC METHODS

    /// Thay đổi cường độ parallax
    public void SetParallaxStrength(float newStrength)
    {
        parallaxStrength = Mathf.Clamp01(newStrength);
        Debug.Log($"Parallax strength changed to: {parallaxStrength}");
    }

    /// Bật/tắt infinite scrolling
    public void SetInfiniteScrolling(bool enabled)
    {
        infiniteScrolling = enabled;
        Debug.Log($"Infinite scrolling: {(enabled ? "ON" : "OFF")}");
    }

    /// Đặt camera target mới
    public void SetCameraTarget(Transform newCamera)
    {
        cameraTransform = newCamera;
        lastCameraPosition = cameraTransform.position;
        Debug.Log($"Camera target changed to: {newCamera.name}");
    }

    /// Lấy thông tin parallax
    public string GetParallaxInfo()
    {
        return $"Strength: {parallaxStrength}, Infinite: {infiniteScrolling}, Size: ({textureUnitSizeX}, {textureUnitSizeY})";
    }
}
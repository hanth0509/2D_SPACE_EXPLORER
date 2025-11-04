using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapBoundary : MonoBehaviour
{
    [Header("MAP SETTINGS")]
    [Tooltip("Sprite background đại diện vùng bản đồ.")]
    public SpriteRenderer backgroundSprite;

    [Header("PLAYER SETTINGS")]
    [Tooltip("Player cần giới hạn di chuyển.")]
    public Transform player;
    public enum BoundaryMode { Clamp, Loop }
    [Header("BOUNDARY MODE")]
    [Tooltip("Clamp = chặn biên | Loop = đi hết rìa sẽ quay lại đầu")]
    public BoundaryMode boundaryMode = BoundaryMode.Clamp;

    [Tooltip("Sử dụng Rigidbody2D để di chuyển vật lý.")]
    public bool usePhysicsMove = true;

    [Tooltip("Thêm khoảng trống nhỏ để tránh kẹt.")]
    public float margin = 0.05f;

    [Header("DEBUG SETTINGS")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;

    private Rigidbody2D playerRb;
    private Vector2 playerHalfSize;
    private float leftBound, rightBound, topBound, bottomBound;
    private bool initialized = false;

    // === KHỞI TẠO ===
    private IEnumerator Start()
    {
        if (backgroundSprite == null)
        {
            backgroundSprite = GetComponent<SpriteRenderer>();
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        // Đợi 1 frame để camera & player ổn định trước khi clamp
        yield return null;

        InitBoundaries();
        initialized = true;
    }

    // === TÍNH BIÊN MAP ===
    void InitBoundaries()
    {
        if (backgroundSprite == null)
        {
            Debug.LogError("MapBoundary: backgroundSprite chưa được gán!");
            return;
        }

        // Biên dựa trên Sprite Bounds
        Bounds b = backgroundSprite.bounds;
        leftBound = b.min.x;
        rightBound = b.max.x;
        bottomBound = b.min.y;
        topBound = b.max.y;

        // Lấy kích thước half-size của player (dựa trên Collider nếu có)
        playerHalfSize = Vector2.one * 0.5f;
        if (player != null)
        {
            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null)
                playerHalfSize = col.bounds.extents;
        }

        if (showDebugLogs)
        {
            Debug.Log($"Boundaries: L:{leftBound:F2}, R:{rightBound:F2}, B:{bottomBound:F2}, T:{topBound:F2}");
            Debug.Log($"Player Half Size: ({playerHalfSize.x:F2}, {playerHalfSize.y:F2})");
        }

        // Clamp vị trí ban đầu của player (chỉ 1 lần)
        ClampPlayerImmediate();
    }

    // === CLAMP PLAYER ===
    void LateUpdate()
    {
        if (!initialized || player == null) return;
        ClampPlayerPosition();
    }

    void ClampPlayerPosition()
    {
        Vector3 pos = player.position;
        Vector3 clampedPos = pos;

        // Tính biên thực (có trừ half-size player)
        float minX = leftBound + playerHalfSize.x + margin;
        float maxX = rightBound - playerHalfSize.x - margin;
        float minY = bottomBound + playerHalfSize.y + margin;
        float maxY = topBound - playerHalfSize.y - margin;

        clampedPos.x = Mathf.Clamp(pos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(pos.y, minY, maxY);

        if (clampedPos != pos)
        {
            if (playerRb != null && usePhysicsMove)
                playerRb.MovePosition(clampedPos);
            else
                player.position = clampedPos;
        }

        if (showDebugLogs && Mathf.Abs(pos.y - clampedPos.y) > 0.01f)
        {
            Debug.Log($"[ClampY] PlayerY:{pos.y:F2} -> Clamped:{clampedPos.y:F2} | Bottom:{minY:F2} | Top:{maxY:F2}");
        }
    }

    // === CLAMP NGAY FRAME ĐẦU ===
    void ClampPlayerImmediate()
    {
        if (player == null) return;

        Vector3 pos = player.position;

        float minX = leftBound + playerHalfSize.x + margin;
        float maxX = rightBound - playerHalfSize.x - margin;
        float minY = bottomBound + playerHalfSize.y + margin;
        float maxY = topBound - playerHalfSize.y - margin;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        player.position = pos;
    }

    // === VẼ BIÊN TRÊN SCENE ===
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || backgroundSprite == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(backgroundSprite.bounds.center, backgroundSprite.bounds.size);
    }
    //     // ======== LOOP MODE =========
    // void LoopPlayerPosition()
    // {
    //     Vector3 pos = player.position;
    //     float width = (rightBound - leftBound);
    //     float height = (topBound - bottomBound);

    //     // Check nếu vượt qua biên thì dịch sang đầu bên kia
    //     if (pos.x > rightBound) pos.x = leftBound + margin;
    //     else if (pos.x < leftBound) pos.x = rightBound - margin;

    //     if (pos.y > topBound) pos.y = bottomBound + margin;
    //     else if (pos.y < bottomBound) pos.y = topBound - margin;

    //     MovePlayer(pos);
    // }

    // // ======== HELPER MOVE =========
    // void MovePlayer(Vector3 targetPos)
    // {
    //     if (playerRb != null && usePhysicsMove)
    //         playerRb.MovePosition(targetPos);
    //     else
    //         player.position = targetPos;
    // }
}










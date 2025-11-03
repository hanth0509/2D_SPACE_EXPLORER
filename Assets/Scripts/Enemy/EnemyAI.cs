using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("AI BEHAVIOR SETTINGS")]
    [Tooltip("Loại hành vi AI của enemy")]
    public AIBehavior behavior = AIBehavior.RandomWander;

    [Tooltip("Khoảng cách phát hiện player")]
    public float detectionRange = 12f;

    [Tooltip("Khoảng cách tấn công")]
    public float attackRange = 8f;

    [Tooltip("Khoảng cách né tránh (nếu behavior là Avoid)")]
    public float avoidDistance = 3f;

    [Header("MOVEMENT SETTINGS")]
    [Tooltip("Tốc độ di chuyển bình thường")]
    public float moveSpeed = 3f;

    [Tooltip("Tốc độ khi chase/avoid player")]
    public float chaseSpeed = 5f;

    [Tooltip("Lực đẩy tối đa")]
    public float maxForce = 8f;

    [Tooltip("Thời gian giữa các lần đổi hướng (giây)")]
    public float directionChangeInterval = 2f;

    [Header("TARGET SETTINGS")]
    [Tooltip("Player transform (tự động tìm)")]
    public Transform playerTarget;

    [Tooltip("Layer mask để raycast detection")]
    public LayerMask obstacleLayers = 1; // Default layer

    [Header("MOVEMENT DIRECTION")]
    [Tooltip("Hướng di chuyển chính (cho right-edge spawn)")]
    public Vector2 primaryMovementDirection = new Vector2(-1, 0); // Sang trái

    // PRIVATE VARIABLES
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private float nextDirectionChangeTime;
    private bool hasLineOfSight = false;

    public enum AIBehavior
    {
        RandomWander,   // Di chuyển ngẫu nhiên
        ChasePlayer,    // Đuổi theo player
        AvoidPlayer,    // Né tránh player
        PatrolPoints    // Tuần tra điểm (nâng cao)
    }

    void Start()
    {
        InitializeAI();
    }

    /// Khởi tạo hệ thống AI
    void InitializeAI()
    {
        // LẤY COMPONENTS
        rb = GetComponent<Rigidbody2D>();

        // TỰ ĐỘNG TÌM PLAYER
        if (playerTarget == null)
        {
            FindPlayer();
        }

        // KHỞI TẠO HƯỚNG NGẪU NHIÊN
        currentDirection = Random.insideUnitCircle.normalized;
        nextDirectionChangeTime = Time.time + directionChangeInterval;

        Debug.Log($" AI Initialized: {behavior}");
        Debug.Log($" Player target: {(playerTarget != null ? playerTarget.name : "None")}");
    }

    void Update()
    {
        if (playerTarget != null)
        {
            UpdatePlayerDetection();
        }

        HandleAIBehavior();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    /// Tự động tìm player trong scene
    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
            Debug.Log(" Found player: " + playerTarget.name);
        }
        else
        {
            Debug.LogWarning(" No player found with tag 'Player'");
        }
    }

    /// Cập nhật trạng thái phát hiện player
    void UpdatePlayerDetection()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        // KIỂM TRA KHOẢNG CÁCH
        if (distanceToPlayer <= detectionRange)
        {
            // KIỂM TRA LINE OF SIGHT
            hasLineOfSight = CheckLineOfSight();

            //  DEBUG: IN RA TRẠNG THÁI
            if (hasLineOfSight && behavior == AIBehavior.RandomWander)
            {
                Debug.Log($"Player detected! Distance: {distanceToPlayer:F1}, Behavior: {behavior}");
            }
        }
        else
        {
            hasLineOfSight = false;
        }
    }

    /// <summary>
    /// Kiểm tra có nhìn thấy player không (không bị vật cản) - DỄ DÀNG HƠN
    /// </summary>
    bool CheckLineOfSight()
    {
        if (playerTarget == null) return false;

        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        // RAYCAST ĐỂ KIỂM TRA VẬT CẢN
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayers
        );

        // VẼ DEBUG RAY
        Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer,
                     hit.collider != null ? Color.red : Color.green, 0.1f);

        // DỄ DETECT HƠN: CHỈ CẦN KHÔNG CÓ VẬT CẢN LÀ ĐƯỢC
        if (hit.collider == null)
        {
            return true;
        }

        // DEBUG THÔNG TIN VẬT CẢN
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            Debug.Log($"Line of sight blocked by: {hit.collider.name}");
        }

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    /// <summary>
    /// Xử lý hành vi AI dựa trên behavior type
    /// </summary>
    void HandleAIBehavior()
    {
        switch (behavior)
        {
            case AIBehavior.RandomWander:
                HandleRandomWander();
                break;

            case AIBehavior.ChasePlayer:
                HandleChasePlayer();
                break;

            case AIBehavior.AvoidPlayer:
                HandleAvoidPlayer();
                break;

            case AIBehavior.PatrolPoints:
                HandlePatrolPoints();
                break;
        }
    }

    /// <summary>
    /// Hành vi: Di chuyển ngẫu nhiên
    /// </summary>
    void HandleRandomWander()
    {
        // ĐỔI HƯỚNG THEO INTERVAL
        if (Time.time >= nextDirectionChangeTime)
        {
            // CHỦ YẾU DI CHUYỂN SANG TRÁI, THÊM CHÚT VARIATION
            Vector2 baseDirection = primaryMovementDirection;
            Vector2 randomVariation = Random.insideUnitCircle * 0.3f; // 30% variation
            currentDirection = (baseDirection + randomVariation).normalized;

            nextDirectionChangeTime = Time.time + directionChangeInterval;
        }

        // PLAYER DETECTION - DỄ TRIGGER HƠN
        if (hasLineOfSight)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            Debug.Log($"Player in sight! Distance: {distanceToPlayer:F1}, Attack Range: {attackRange}");

            // LUÔN CHASE KHI PHÁT HIỆN PLAYER TRONG ATTACK RANGE
            if (distanceToPlayer < attackRange)
            {
                behavior = AIBehavior.ChasePlayer;
                Debug.Log($"Switching to CHASE behavior! Distance: {distanceToPlayer:F1}");
            }
        }
    }

    /// Hành vi: TẤN CÔNG player - KHÔNG NÉ TRÁNH
    void HandleChasePlayer()
    {
        if (playerTarget != null && hasLineOfSight)
        {
            // TÍNH TOÁN HƯỚNG TẤN CÔNG (dự đoán vị trí player)
            Vector2 playerPosition = playerTarget.position;
            Vector2 playerVelocity = Vector2.zero;

            // Nếu player có Rigidbody, dự đoán vị trí tương lai
            Rigidbody2D playerRb = playerTarget.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerVelocity = playerRb.linearVelocity;
            }

            // DỰ ĐOÁN VỊ TRÍ TIẾP THEO CỦA PLAYER (lead target)
            float predictionTime = 0.5f; // Dự đoán 0.5 giây tiếp theo
            Vector2 predictedPosition = playerPosition + playerVelocity * predictionTime;

            // HƯỚNG VỀ VỊ TRÍ DỰ ĐOÁN
            Vector2 directionToPredictedPosition = (predictedPosition - (Vector2)transform.position).normalized;
            currentDirection = directionToPredictedPosition;

            Debug.Log($" ATTACKING player! Speed: {rb.linearVelocity.magnitude:F1}, Predicted position");
        }
        else
        {
            //  MẤT TẦM NHÌN → QUAY LẠI RANDOM WANDER
            behavior = AIBehavior.RandomWander;
            Debug.Log("🔄 Lost sight of player, returning to wander");
        }
    }

    /// Hành vi: Né tránh player
    void HandleAvoidPlayer()
    {
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer < avoidDistance && hasLineOfSight)
            {
                // HƯỚNG NGƯỢC LẠI PLAYER
                Vector2 directionAwayFromPlayer = (transform.position - playerTarget.position).normalized;
                currentDirection = directionAwayFromPlayer;
            }
            else
            {
                // ĐỦ XA → QUAY LẠI RANDOM WANDER
                behavior = AIBehavior.RandomWander;
                Debug.Log(" Safe distance from player, returning to wander");
            }
        }
    }

    /// Hành vi: Tuần tra điểm (placeholder)
    void HandlePatrolPoints()
    {
        // CÓ THỂ IMPLEMENT PATROL POINTS SAU
        currentDirection = GetRandomDirection();
    }

    /// Áp dụng chuyển động vật lý
    void ApplyMovement()
    {
        if (rb == null) return;

        // TÍNH TỐC ĐỘ DỰA TRÊN BEHAVIOR
        float currentSpeed = moveSpeed;
        if (behavior == AIBehavior.ChasePlayer || behavior == AIBehavior.AvoidPlayer)
        {
            currentSpeed = chaseSpeed;
        }

        //  TĂNG CƯỜNG FORCE KHI CHASE
        float forceMultiplier = 1f;
        if (behavior == AIBehavior.ChasePlayer)
        {
            forceMultiplier = 2f; // Force mạnh hơn khi chase
        }

        // ÁP DỤNG LỰC
        Vector2 desiredVelocity = currentDirection * currentSpeed;
        Vector2 steeringForce = (desiredVelocity - rb.linearVelocity) * maxForce * forceMultiplier;

        rb.AddForce(steeringForce);

        // GIỚI HẠN TỐC ĐỘ TỐI ĐA
        if (rb.linearVelocity.magnitude > currentSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }

        // XOAY THEO HƯỚNG DI CHUYỂN (tùy chọn)
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    /// Lấy hướng ngẫu nhiên
    Vector2 GetRandomDirection()
    {
        return Random.insideUnitCircle.normalized;
    }

    /// Lấy hướng tránh vật cản
    Vector2 GetObstacleAvoidanceDirection()
    {
        // KIỂM TRA VẬT CẢN PHÍA TRƯỚC
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, 2f, obstacleLayers);

        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            // CÓ VẬT CẢN → ĐỔI HƯỚNG
            return Vector2.Perpendicular(currentDirection) * (Random.value > 0.5f ? 1 : -1);
        }

        return currentDirection;
    }

    // PUBLIC METHODS

    /// Thay đổi hành vi AI
    public void SetBehavior(AIBehavior newBehavior)
    {
        behavior = newBehavior;
        Debug.Log($" AI Behavior changed to: {newBehavior}");
    }

    /// Đặt target mới cho AI
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
        Debug.Log($" AI Target changed to: {newTarget.name}");
    }

    /// Kích hoạt chế độ hoảng sợ (né tránh)
    public void TriggerPanic()
    {
        behavior = AIBehavior.AvoidPlayer;
        nextDirectionChangeTime = Time.time + 1f; // Ngắn hơn để phản ứng nhanh
        Debug.Log(" AI Panic triggered!");
    }

    /// Kiểm tra AI có nhìn thấy player không
    public bool CanSeePlayer()
    {
        return hasLineOfSight;
    }

    /// Lấy khoảng cách đến player
    public float GetDistanceToPlayer()
    {
        if (playerTarget == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, playerTarget.position);
    }

    // DEBUG GIZMOS
    void OnDrawGizmosSelected()
    {
        // VÒNG PHÁT HIỆN
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // VÒNG TẤN CÔNG
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // VÒNG NÉ TRÁNH
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidDistance);

        // HƯỚNG HIỆN TẠI
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
    }
}






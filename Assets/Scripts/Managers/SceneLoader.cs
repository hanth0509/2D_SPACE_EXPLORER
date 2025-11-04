using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsPanel;
    public GameObject firstFocusElement; // Thêm reference đến element đầu tiên cần focus

    [Header("Input Settings")]
    public PlayerInput playerInput;

    // Biến để kiểm soát trạng thái panel
    private bool isInstructionsShowing = false;
    private GameObject lastSelectedElement; // Lưu element được chọn trước đó

    // Sự kiện Unity để kết nối trong Inspector
    public UnityEvent onEscapePressed;

    void Start()
    {
        // Đảm bảo InstructionsPanel ẩn khi game bắt đầu
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            isInstructionsShowing = false;
        }

        // Tự động tìm PlayerInput nếu chưa gán
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning("PlayerInput component not found. Adding one...");
                playerInput = gameObject.AddComponent<PlayerInput>();
            }
        }

        // Cấu hình PlayerInput
        if (playerInput != null)
        {
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            playerInput.defaultActionMap = "UI";
        }

        Debug.Log("SceneLoader initialized with Input System - Ready to manage scenes!");
    }

    // === INPUT SYSTEM EVENT HANDLERS ===

    /// Xử lý khi nhấn ESC - được gọi bởi Input System
    public void OnEscape(InputAction.CallbackContext context)
    {
        // Chỉ xử lý khi button được nhấn (không phải released)
        if (context.performed)
        {
            Debug.Log("Escape key pressed via Input System");

            if (isInstructionsShowing)
            {
                HideInstructions();
            }

            // Kích hoạt sự kiện Unity (nếu có kết nối)
            onEscapePressed?.Invoke();
        }
    }

    // === PUBLIC METHODS FOR BUTTONS ===

    /// Hiển thị Instructions Panel với focus management
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            // Lưu element đang được chọn trước đó
            lastSelectedElement = EventSystem.current.currentSelectedGameObject;

            instructionsPanel.SetActive(true);
            isInstructionsShowing = true;
            Debug.Log("Instructions panel shown");

            // Set focus đến element đầu tiên trong panel
            StartCoroutine(SetFocusToFirstElement());
        }
        else
        {
            Debug.LogWarning("InstructionsPanel reference is missing!");
        }
    }

    /// Ẩn Instructions Panel và restore focus
    public void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            isInstructionsShowing = false;
            Debug.Log("Instructions panel hidden");

            // Restore focus về element trước đó
            StartCoroutine(RestoreFocus());
        }
    }

    // === FOCUS MANAGEMENT COROUTINES ===

    /// Coroutine để set focus đến element đầu tiên trong panel
    private IEnumerator SetFocusToFirstElement()
    {
        // Chờ 1 frame để UI update
        yield return null;

        if (firstFocusElement != null)
        {
            EventSystem.current.SetSelectedGameObject(firstFocusElement);
            Debug.Log("Focus set to: " + firstFocusElement.name);
        }
        else
        {
            // Tự động tìm Close button nếu không chỉ định
            GameObject closeButton = FindCloseButton();
            if (closeButton != null)
            {
                EventSystem.current.SetSelectedGameObject(closeButton);
                Debug.Log("Auto-focus set to Close button");
            }
        }
    }

    /// Coroutine để restore focus về element trước đó
    private IEnumerator RestoreFocus()
    {
        // Chờ 1 frame để UI update
        yield return null;

        if (lastSelectedElement != null && lastSelectedElement.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedElement);
            Debug.Log("Focus restored to: " + lastSelectedElement.name);
        }
        else
        {
            // Nếu không có element trước đó, focus về Instructions button
            GameObject instructionsButton = GameObject.Find("InstructionsButton");
            if (instructionsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(instructionsButton);
                Debug.Log("Focus set to Instructions button");
            }
        }
    }

    /// Tự động tìm Close button trong panel
    private GameObject FindCloseButton()
    {
        if (instructionsPanel != null)
        {
            // Tìm button có tên chứa "Close"
            foreach (Transform child in instructionsPanel.transform)
            {
                if (child.name.Contains("Close") || child.name.Contains("close"))
                {
                    return child.gameObject;
                }

                // Tìm trong children của children
                foreach (Transform grandchild in child)
                {
                    if (grandchild.name.Contains("Close") || grandchild.name.Contains("close"))
                    {
                        return grandchild.gameObject;
                    }
                }
            }
        }
        return null;
    }

    // === CÁC METHODS KHÁC GIỮ NGUYÊN ===

    public void LoadGameplay()
    {
        Debug.Log("Loading Gameplay scene...");
        SceneManager.LoadScene("Gameplay");
    }

    public void ToggleInstructions()
    {
        if (instructionsPanel != null)
        {
            isInstructionsShowing = !isInstructionsShowing;
            instructionsPanel.SetActive(isInstructionsShowing);

            if (isInstructionsShowing)
            {
                StartCoroutine(SetFocusToFirstElement());
            }
            else
            {
                StartCoroutine(RestoreFocus());
            }

            Debug.Log("Instructions panel toggled: " + isInstructionsShowing);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked - Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void LoadSceneByName(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Loading scene index: " + sceneIndex);
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError("Invalid scene index: " + sceneIndex);
        }
    }

    // === INPUT SYSTEM MANAGEMENT ===

    public void EnableInput()
    {
        if (playerInput != null)
        {
            playerInput.enabled = true;
            playerInput.ActivateInput();
        }
    }

    public void DisableInput()
    {
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            playerInput.enabled = false;
        }
    }
    // Hàm load scene EndGame
    public void LoadEndGameScene()
    {
        // Kiểm tra scene có trong Build Settings
        if (Application.CanStreamedLevelBeLoaded("EndGame"))
        {
            SceneManager.LoadScene("EndGame");
        }
        else
        {
            Debug.LogError("EndGame scene chưa được thêm vào Build Settings!");
        }
    }
}

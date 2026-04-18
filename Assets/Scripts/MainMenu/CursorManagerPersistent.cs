using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManagerPersistent : MonoBehaviour
{
    public static CursorManagerPersistent Instance { get; private set; }

    public string gameSceneName = "GameScene";

    private bool isPausedExtern = false;
    private Vector3 lastMousePosition;
    private bool usingController = false;
    private SteffScript cachedSteff;
    private string cachedScene = "";

    void Awake()
    {
        // Singleton: sicherstellen, dass nur eine Instanz existiert
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        lastMousePosition = Input.mousePosition;
        UpdateCursorVisibility();
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameSceneName)
        {
            HandleInGameScene();
        }
        else
        {
            HandleDefaultScene();
        }
    }

    void HandleInGameScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (cachedScene != currentScene)
        {
            cachedScene = currentScene;
            cachedSteff = null;
        }
        if (cachedSteff == null)
            cachedSteff = FindFirstObjectByType<SteffScript>();

        isPausedExtern = (cachedSteff != null) && cachedSteff.IsPaused();
        bool isDead = (cachedSteff != null) && !cachedSteff.steffIsAlive;

        if (isPausedExtern || isDead)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void HandleDefaultScene()
    {
        Vector3 currentMousePosition = Input.mousePosition;

        if ((currentMousePosition - lastMousePosition).sqrMagnitude > 1f)
        {
            if (usingController)
            {
                usingController = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (IsControllerInput())
        {
            if (!usingController)
            {
                usingController = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        lastMousePosition = currentMousePosition;
    }

    bool IsControllerInput()
    {
        return Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
               Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ||
               Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel");
    }

    void UpdateCursorVisibility()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameSceneName && !isPausedExtern)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

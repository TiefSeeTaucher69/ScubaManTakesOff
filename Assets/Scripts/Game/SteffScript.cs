using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteffScript : MonoBehaviour
{
    public Rigidbody2D myRigitbody;
    public float flapStrength;
    public LogicScript logic;
    public bool steffIsAlive = true;
    private AudioSource hitAudioSource;

    public GameObject escapeInGameScreen;
    public GameObject settingsOnPauseScreen;
    private bool isPaused = false;
    private bool settingsManuallyOpened = false;
    public float runTime = 0f;
    public WeeklyMissionManager weeklyMissionManager;
    public Transform jointOffset;
    public ParticleSystem jointSmokeParticles;


    private SpriteRenderer spriteRenderer;

    void Start()
    {
        Cursor.visible = false;
        logic = GameObject.FindGameObjectsWithTag("Logic")[0].GetComponent<LogicScript>();
        hitAudioSource = GetComponent<AudioSource>();
        weeklyMissionManager = FindObjectOfType<WeeklyMissionManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // === Lade Skin ===
        string selectedSkin = PlayerPrefs.GetString("ActiveSkin", "benjo-bird");
        Sprite skinSprite = Resources.Load<Sprite>("Skins/" + selectedSkin);

        if (skinSprite != null)
        {
            spriteRenderer.sprite = skinSprite;

            // Skalierung abh�ngig vom Skin
            if (selectedSkin == "tom-bird")
            {
                Debug.Log("Skin '" + selectedSkin + "' gefunden. Skalierung 0.8x0.7x1");
                transform.localScale = new Vector3(0.8f, 0.7f, 1f); // Tom-Bird Gr��e
            }
            else if (selectedSkin == "paulaner-bird")
            {
                transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }
            else if (selectedSkin == "ginger-bird")
            {
                transform.localScale = new Vector3(0.8f, 0.7f, 1f); // Benjo-Bird Gr��e
            }
            else if (selectedSkin == "bennet-bird")
            {
                transform.localScale = new Vector3(0.8f, 0.79f, 1f); // Bennet-Bird Gr��e
            }
            else if (selectedSkin == "jan-bird")
            {
                transform.localScale = new Vector3(0.8f, 0.7f, 1f); // Jan-Bird Gr��e
            }
            else
            {
                Debug.Log("Standard-Skin 'benjo-bird' verwendet. Skalierung 0.8x0.8x1");
                transform.localScale = new Vector3(0.8f, 0.8f, 1f); // Standardgröße für steff-bird
            }
        }
        else
        {
            Debug.LogWarning("Sprite nicht gefunden f�r Skin: " + selectedSkin + ". Verwende benjo-bird als Fallback.");
            spriteRenderer.sprite = Resources.Load<Sprite>("Skins/benjo-bird");
            transform.localScale = new Vector3(0.8f, 0.7f, 1f);
        }

        // Joint Offset setzen
        if (jointOffset != null)
        {
            switch (selectedSkin)
            {
                case "benjo-bird":
                    jointOffset.localPosition = new Vector3(1.57f, -0.19f, -0.1f);
                    break;
                case "tom-bird":
                    jointOffset.localPosition = new Vector3(1.75f, -0.15f, -0.1f);
                    break;
                case "paulaner-bird":
                    jointOffset.localPosition = new Vector3(1.81f, -0.31f, -0.1f);
                    break;
                case "ginger-bird":
                    jointOffset.localPosition = new Vector3(1.7f, -0.5f, -0.1f);
                    break;
                case "bennet-bird":
                    jointOffset.localPosition = new Vector3(1.9f, -0.35f, -0.1f);
                    break;
                case "jan-bird":
                    jointOffset.localPosition = new Vector3(1.8f, -0.19f, -0.1f);
                    break;
                default:
                    jointOffset.localPosition = new Vector3(1.57f, -0.19f, -0.1f);
                    break;
            }
        }



        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(false);

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(false);
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7)) && steffIsAlive)
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else if (isPaused && settingsManuallyOpened)
            {
                CloseSettingsOnPause();
            }
            else
            {
                ResumeGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton1) && steffIsAlive)
        {
            if (isPaused && settingsManuallyOpened)
            {
                CloseSettingsOnPause();
            }
            else
            {
                ResumeGame();
            }
        }

        if (steffIsAlive)
        {
            runTime += Time.deltaTime;
        }

        if (!steffIsAlive && logic != null && logic.gameOverScreen != null && logic.gameOverScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                logic.restartGame();
            }
        }

        if (isPaused) return;

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && steffIsAlive)
        {
            myRigitbody.linearVelocity = Vector2.up * flapStrength;
            if (weeklyMissionManager != null)
            {
                weeklyMissionManager.UpdateMission(MissionType.TotalJumps, 1);
            }
        }

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            logic.gameOver();
            steffIsAlive = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hitAudioSource.isPlaying && steffIsAlive)
        {
            hitAudioSource.Play();
        }
        logic.gameOver();
        steffIsAlive = false;
    }

    private void PauseGame()
    {
        Cursor.visible = true;
        isPaused = true;
        settingsManuallyOpened = false;
        Time.timeScale = 0f;

        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(true);

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(false);
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(false);

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(false);

        Cursor.visible = false;
    }

    public void OpenSettingsOnPause()
    {
        Cursor.visible = true;
        settingsManuallyOpened = true;

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(true);

        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(false);
    }

    public void CloseSettingsOnPause()
    {
        settingsManuallyOpened = false;

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(false);

        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(true);
    }

    public void FromPauseToMenu()
    {
        ResumeGame();
        steffIsAlive = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitFromPause()
    {
        Application.Quit();
        Debug.Log("Application quit requested from pause menu");
    }

    public float GetRunTime()
    {
        return runTime;
    }

    public bool DidSurviveAtLeast(float seconds)
    {
        return runTime >= seconds;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void PlaySmokeEffect()
    {
        if (jointSmokeParticles != null)
        {
            jointSmokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            jointSmokeParticles.Play();
        }
    }
}

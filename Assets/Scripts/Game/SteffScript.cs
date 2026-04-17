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
    private float _baseJointX;
    private ShieldManager shieldManager;
    private float _originalGravityScale;

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
            _baseJointX = jointOffset.localPosition.x; // positiver Basiswert für Richtungs-Flip
        }



        if (escapeInGameScreen != null)
            escapeInGameScreen.SetActive(false);

        if (settingsOnPauseScreen != null)
            settingsOnPauseScreen.SetActive(false);

        shieldManager = FindObjectOfType<ShieldManager>();
        _originalGravityScale = myRigitbody.gravityScale;
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

        // Vogel kippt vor/zurück je nach Rigidbody-Geschwindigkeit + Richtung
        if (steffIsAlive)
        {
            // Sprite-Richtung und Joint-Position spiegeln
            spriteRenderer.flipX = DirectionFlipManager.IsFlipped;
            if (jointOffset != null)
            {
                float sign = DirectionFlipManager.IsFlipped ? -1f : 1f;
                jointOffset.localPosition = new Vector3(
                    _baseJointX * sign,
                    jointOffset.localPosition.y,
                    jointOffset.localPosition.z);
                // Y-Rotation spiegeln damit angehängte Partikel in die richtige Richtung zeigen
                jointOffset.localEulerAngles = new Vector3(
                    jointOffset.localEulerAngles.x,
                    DirectionFlipManager.IsFlipped ? 180f : 0f,
                    jointOffset.localEulerAngles.z);
            }

            // Rotationsfaktor: Flip und Gravity-Inversion jeweils negieren
            bool negated = GravityInversionManager.IsInverted ^ DirectionFlipManager.IsFlipped;
            float rotFactor = negated ? -5f : 5f;
            float targetAngle = Mathf.Clamp(myRigitbody.linearVelocity.y * rotFactor, -80f, 30f);
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }

        float gravSign = GravityInversionManager.IsInverted
            ? -(RemoteConfigManager.Instance != null ? RemoteConfigManager.Instance.GravityInvertStrength : 0.75f)
            : 1f;
        myRigitbody.gravityScale = _originalGravityScale * gravSign * SpeedManager.SlowMoMultiplier;

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && steffIsAlive)
        {
            Vector2 flapDir = GravityInversionManager.IsInverted ? Vector2.down : Vector2.up;
            float flapMult = GravityInversionManager.IsInverted
                ? (RemoteConfigManager.Instance != null ? RemoteConfigManager.Instance.GravityFlapStrength : 0.75f)
                : 1f;
            myRigitbody.linearVelocity = flapDir * flapStrength * flapMult * SpeedManager.SlowMoMultiplier;
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
        if (ShieldManager.IsShieldActive && steffIsAlive)
        {
            shieldManager?.AbsorbHit(collision.gameObject);
            return;
        }

        if (!hitAudioSource.isPlaying && steffIsAlive)
        {
            hitAudioSource.Play();
        }
        if (CameraShakeScript.Instance != null)
            CameraShakeScript.Instance.Shake(0.4f, 0.4f, playDeathSound: true);
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

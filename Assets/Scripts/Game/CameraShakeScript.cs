using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class CameraShakeScript : MonoBehaviour
{
    public static CameraShakeScript Instance;

    [Header("Death Sound")]
    public AudioClip deathSound;
    public AudioMixerGroup sfxMixerGroup;

    private Vector3 originalLocalPos;
    private float originalOrthoSize;
    private Camera cam;
    private Coroutine activeShake;

    void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
        cam = GetComponent<Camera>();
        if (cam != null)
            originalOrthoSize = cam.orthographicSize;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Shake(float duration, float magnitude, bool playDeathSound = false)
    {
        if (activeShake != null)
            StopCoroutine(activeShake);
        activeShake = StartCoroutine(DoShake(duration, magnitude));

        if (playDeathSound && deathSound != null)
        {
            var go = new GameObject("DeathSound_OneShot");
            var src = go.AddComponent<AudioSource>();
            src.clip = deathSound;
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.Play();
            Destroy(go, deathSound.length + 0.1f);
        }
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = magnitude * (1f - elapsed / duration);
            transform.localPosition = originalLocalPos + (Vector3)(Random.insideUnitCircle * strength);
            if (cam != null)
                cam.orthographicSize = originalOrthoSize + strength;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        if (cam != null)
            cam.orthographicSize = originalOrthoSize;
        activeShake = null;
    }
}

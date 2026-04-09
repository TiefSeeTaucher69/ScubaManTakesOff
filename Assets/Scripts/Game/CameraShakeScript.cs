using System.Collections;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
    public static CameraShakeScript Instance;

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

    public void Shake(float duration, float magnitude)
    {
        if (activeShake != null)
            StopCoroutine(activeShake);
        activeShake = StartCoroutine(DoShake(duration, magnitude));
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

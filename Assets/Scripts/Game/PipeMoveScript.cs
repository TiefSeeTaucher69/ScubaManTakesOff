using UnityEngine;

public class PipeMoveScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float deadZone = -40f;

    void Update()
    {
        float speed = SpeedManager.currentSpeed * SpeedManager.SlowMoMultiplier;
        float dir = DirectionFlipManager.IsFlipped ? 1f : -1f;
        transform.position += Vector3.right * dir * speed * Time.deltaTime;

        bool offscreen = DirectionFlipManager.IsFlipped
            ? transform.position.x > 40f
            : transform.position.x < deadZone;
        if (offscreen)
        {
            Destroy(gameObject);
        }
    }
}

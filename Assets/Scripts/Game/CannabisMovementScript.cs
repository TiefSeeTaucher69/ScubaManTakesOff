using UnityEngine;

public class CannabisMovementScript : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float deadZone = -40f;
    void Update()
    {
        float speed = SpeedManagerCannabisScript.currentSpeed * SpeedManagerCannabisScript.SlowMoMultiplier;
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

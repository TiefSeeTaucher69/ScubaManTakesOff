using UnityEngine;

public class CannabisMovementScript : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float deadZone = -40f;
    void Update()
    {
        float speed = SpeedManagerCannabisScript.currentSpeed;
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }


}

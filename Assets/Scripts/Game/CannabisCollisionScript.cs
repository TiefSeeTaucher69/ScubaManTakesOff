using UnityEngine;
using System.Collections;

public class CannabisCollisionScript : MonoBehaviour
{
    public static readonly System.Collections.Generic.HashSet<CannabisCollisionScript> All = new();

    public LogicScript logic;
    public SteffScript steffReference;
    private bool canTrigger = true;

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Start()
    {
        logic = FindFirstObjectByType<LogicScript>();
        steffReference = FindFirstObjectByType<SteffScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canTrigger) return;

        if (collision.gameObject.layer == 3 && steffReference.steffIsAlive)
        {
            canTrigger = false;
            logic.addCannabisScore(1);
            // Destroy root so the CannabisMovementScript on the parent is also removed
            Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
        }
    }

    private IEnumerator ResetTriggerCooldown()
    {
        yield return new WaitForSeconds(0.5f); // 0.5 Sek. Pause
        canTrigger = true;
    }
}

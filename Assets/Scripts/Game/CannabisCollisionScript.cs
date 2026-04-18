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
        logic = GameObject.FindGameObjectsWithTag("Logic")[0].GetComponent<LogicScript>();
        steffReference = GameObject.FindGameObjectWithTag("Steff").GetComponent<SteffScript>();
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
            Destroy(gameObject); // Cannabis-Objekt zerst�ren
        }
    }

    private IEnumerator ResetTriggerCooldown()
    {
        yield return new WaitForSeconds(0.5f); // 0.5 Sek. Pause
        canTrigger = true;
    }
}

using UnityEngine;

public class PetManager : MonoBehaviour
{
    [System.Serializable]
    public struct PetEntry
    {
        public string     petName; // muss exakt mit activeValue im Shop übereinstimmen
        public GameObject prefab;
    }

    [SerializeField] private PetEntry[] petEntries;
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float seekSpeed       = 28f;

    void Start()
    {
        if (RankedManager.IsRanked) return;

        string active = PlayerPrefs.GetString("ActivePet", "");
        if (string.IsNullOrEmpty(active)) return;

        GameObject prefab = null;
        foreach (var e in petEntries)
            if (e.petName == active) { prefab = e.prefab; break; }
        if (prefab == null) return;

        var steff = FindObjectOfType<SteffScript>();
        var logic = FindObjectOfType<LogicScript>();
        if (steff == null || logic == null) return;

        var pet = Instantiate(prefab, steff.transform.position + new Vector3(-1.8f, 0f, 0f), Quaternion.identity);
        pet.AddComponent<PetCompanionScript>().Init(steff, logic, detectionRadius, seekSpeed);
    }
}

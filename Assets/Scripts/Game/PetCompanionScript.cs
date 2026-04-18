using Assets.FantasyMonsters.Scripts;
using UnityEngine;

public class PetCompanionScript : MonoBehaviour
{
    private enum State { Following, Seeking, Attacking, Returning, Mourning }

    private float _detectionRadius = 12f;
    private float _seekSpeed       = 28f;
    private float _smoothTime      = 0.25f;
    private float _attackDuration  = 0.6f;
    private float _scale           = 0.25f;
    // Offset wird per-frame anhand von IsFlipped berechnet
    private Vector3 FollowOffset => DirectionFlipManager.IsFlipped
        ? new Vector3(1.8f, 0f, 0f)   // Pet rechts vom Vogel wenn nach links geflippt
        : new Vector3(-1.8f, 0f, 0f); // Pet links vom Vogel (normal)

    private Monster    _monster;
    private LogicScript _logic;
    private SteffScript _steff;
    private Transform               _leafTransform;
    private GameObject              _leafGO;
    private CannabisCollisionScript _leafCollision; // gecacht um GetComponent in Attack zu vermeiden
    private State      _state;
    private float      _attackTimer;
    private Vector3    _velocity;
    private Vector3    _deathTarget;
    private bool       _deathAnimPlayed;
    private bool       _sinking;
    private float      _sinkSpeed;

    public void Init(SteffScript steff, LogicScript logic, float detectionRadius, float seekSpeed)
    {
        _steff           = steff;
        _logic           = logic;
        _detectionRadius = detectionRadius;
        _seekSpeed       = seekSpeed;
        _monster         = GetComponent<Monster>();

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        transform.localScale = new Vector3(-_scale, _scale, 1f); // negatives X = nach rechts
        _monster?.SetState(MonsterState.Walk);
    }

    void Update()
    {
        if (_steff == null) return;

        if (!_steff.steffIsAlive)
        {
            if (_state != State.Mourning)
            {
                // Einmalig: Todesposition merken und hinlaufen
                _deathTarget      = _steff.transform.position;
                _deathAnimPlayed  = false;
                _leafGO           = null;
                _leafTransform    = null;
                _leafCollision    = null;
                _monster?.SetState(MonsterState.Run);
                _state = State.Mourning;
            }
            Mourn();
            return;
        }

        // Blickrichtung je nach Spielrichtung aktualisieren
        float scaleX = DirectionFlipManager.IsFlipped ? _scale : -_scale;
        transform.localScale = new Vector3(scaleX, _scale, 1f);

        switch (_state)
        {
            case State.Following: Follow(); Scan(); break;
            case State.Seeking:   Seek();           break;
            case State.Attacking: Attack();         break;
            case State.Returning: Return();         break;
        }
    }

    void Mourn()
    {
        // Phase 2: nach Death-Animation langsam nach unten gleiten
        if (_sinking)
        {
            _sinkSpeed = Mathf.MoveTowards(_sinkSpeed, 4f, 1.5f * Time.deltaTime);
            transform.position += Vector3.down * _sinkSpeed * Time.deltaTime;
            return;
        }

        if (_deathAnimPlayed) return;

        // Phase 1: flüssig zur Todesposition laufen
        transform.position = Vector3.SmoothDamp(
            transform.position, _deathTarget, ref _velocity, 0.35f, _seekSpeed);

        if (Near(_deathTarget, 0.4f))
        {
            _monster?.SetState(MonsterState.Death);
            _deathAnimPlayed = true;
            // Kurz warten dann sinken starten
            StartCoroutine(StartSinking());
        }
    }

    System.Collections.IEnumerator StartSinking()
    {
        yield return new WaitForSeconds(1.2f);
        _sinkSpeed = 0f;
        _sinking   = true;
    }

    void Follow() => SmoothMove(_steff.transform.position + FollowOffset);

    void Return()
    {
        SmoothMove(_steff.transform.position + FollowOffset);
        if (Near(_steff.transform.position + FollowOffset, 0.5f))
            _state = State.Following;
    }

    void Seek()
    {
        // Blatt wurde von Spieler eingesammelt oder ist verschwunden
        if (_leafGO == null) { _state = State.Following; return; }

        // Jedes Frame aktuelle Weltposition lesen — Blatt bewegt sich
        Vector3 leafPos = _leafTransform.position;

        transform.position = Vector3.MoveTowards(transform.position, leafPos, _seekSpeed * Time.deltaTime);

        if (Near(leafPos, 1.2f))
        {
            _attackTimer = _attackDuration;
            _monster?.Attack();
            _state = State.Attacking;
        }
    }

    void Attack()
    {
        // Blatt mitverfolgen während Animation — sonst bleibt Pet stehen während Blatt weiterfliegt
        if (_leafGO != null && _leafTransform != null)
            transform.position = Vector3.MoveTowards(transform.position, _leafTransform.position, _seekSpeed * Time.deltaTime);

        _attackTimer -= Time.deltaTime;
        if (_attackTimer > 0f) return;

        if (_leafGO != null)
        {
            // CannabisCollisionScript deaktivieren damit Spieler nicht gleichzeitig auch scored
            if (_leafCollision != null) _leafCollision.enabled = false;

            _logic.addCannabisScore(1);
            // Root (Parent des Child) zerstören → killt Root + Child komplett
            var root = _leafGO.transform.parent != null
                ? _leafGO.transform.parent.gameObject
                : _leafGO;
            Destroy(root);
        }
        _leafGO        = null;
        _leafTransform = null;
        _leafCollision = null;

        _monster?.SetState(MonsterState.Walk);
        _state = State.Returning;
    }

    void Scan()
    {
        GameObject bestGO = null;
        Transform  bestT  = null;
        float      min    = _detectionRadius;

        // CannabisCollisionScript sitzt auf dem Child (cannabis_0) — das ist die sichtbare
        // Blatt-Position. CannabisMovementScript sitzt auf dem Root, der ~10 Einheiten daneben liegt.
        foreach (var l in CannabisCollisionScript.All)
        {
            // Nur Blätter die noch VOR dem Pet sind (noch nicht vorbeigeflogen)
            bool passedPet = DirectionFlipManager.IsFlipped
                ? l.transform.position.x > transform.position.x + 2f  // flipped: Blätter kommen von links
                : l.transform.position.x < transform.position.x - 2f; // normal: Blätter kommen von rechts
            if (passedPet) continue;

            float d = Vector3.Distance(transform.position, l.transform.position);
            if (d < min)
            {
                min    = d;
                bestGO = l.gameObject;  // Child-GO → wird beim Spieler-Einsammeln zerstört
                bestT  = l.transform;   // Child-Transform → korrekte Weltposition
            }
        }

        if (bestGO == null) return;

        _leafGO        = bestGO;
        _leafTransform = bestT;
        _leafCollision = bestGO.GetComponent<CannabisCollisionScript>();
        _monster?.SetState(MonsterState.Run);
        _state = State.Seeking;
    }

    void SmoothMove(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, _smoothTime);
    }

    bool Near(Vector3 p, float r) => Vector3.Distance(transform.position, p) < r;
}

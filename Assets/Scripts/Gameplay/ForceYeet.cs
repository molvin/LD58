using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForceYeet : MonoBehaviour
{
    enum ScuffedState
    {
        Upkeep,
        Playing,
        Yeeting,
    }

    struct CollisionPair
    {
        public int FirstID;
        public float FirstDmg;
        public int SecondID;
        public float SecondDmg;
    }

    public bool Debugging = false;

    public float YeetForce = 20.0f;
    public float DistForMaxForce = 5.0f;
    public float Deadzone = 1.0f;
    public Collider GroundCollider;

    [HideInInspector] public List<Pawn> Pawns;
    private Dictionary<CollisionPair, (float time, float impact)> forcePairs = new();

    private ScuffedState activeState = ScuffedState.Upkeep;
    private int activeTeam;

    private Pawn whoToYeet;
    private Vector3 lastYeetPoint;
    private Vector3 originalYeetPos;
    private Quaternion originalYeetRot;

    private LineRenderer forceArrowRend;
    private Coroutine upkeep;
    private Coroutine debugPlaying;

    private void Update()
    {
        if (Debugging && debugPlaying == null)
        {
            List<Pawn> pawns = new(FindObjectsByType<Pawn>(FindObjectsSortMode.None));

            debugPlaying = StartCoroutine(DebugPlay(pawns.Where(p => p.Team == 0).ToList(), pawns.Where(p => p.Team == 1).ToList()));
        }
    }

    public async Awaitable DebugPlay(List<Pawn> playerTeam, List<Pawn> opponentTeam)
    {
        await Play(playerTeam, opponentTeam);
    }


    public async Awaitable<GameState> Play(List<Pawn> playerTeam, List<Pawn> opponentTeam)
    {
        activeTeam = Random.Range(0, 2);

        foreach (Pawn pawn in playerTeam)
        {
            pawn.Team = 0;
        }
        foreach(Pawn pawn in opponentTeam)
        {
            pawn.Team = 1;
        }

        Initialize(playerTeam.Union(opponentTeam).ToList());

        GameState state = GameState.Playing;
        while (state == GameState.Playing)
        {
            ResolveForces();

            switch (activeState)
            {
                case ScuffedState.Upkeep:
                    {
                        Upkeeep();
                    }
                    break;
                case ScuffedState.Playing:
                    {
                        HandlePlayerInput();
                    }
                    break;
                case ScuffedState.Yeeting:
                    {
                        ResolveCollisionResponses();
                    }
                    break;
            }
            state = GetGameState();
            await Awaitable.NextFrameAsync();
        }

        foreach(Pawn pawn in Pawns)
        {
            if(pawn != null)
            {
                Destroy(pawn.gameObject);
            }
        }
        Pawns.Clear();

        return state;
    }

    private void Initialize(List<Pawn> pawns)
    {
        Pawns = pawns;
        foreach(Pawn pawn in Pawns)
        {
            pawn.enabled = true;
            pawn.Manager = this;
            pawn.rigidbody.isKinematic = false;
            pawn.rigidbody.angularVelocity = Vector3.zero;
            pawn.rigidbody.linearVelocity = Vector3.zero;
            pawn.initialStartPosition = pawn.transform.position;
            if(GroundCollider != null)
            {
                Ray ray = new Ray(pawn.transform.position + Vector3.up, Vector3.down);
                if (GroundCollider.Raycast(ray, out RaycastHit hitInfo, 10000.0f))
                {
                    pawn.transform.position = hitInfo.point + Vector3.up * 0.01f;
                }
            }
            else
            {
                Debug.LogWarning("Ground collider not set");
            }
            
        }
        forceArrowRend = GetComponent<LineRenderer>();
        forceArrowRend.enabled = false;
    }

    public enum GameState
    {
        Playing,
        Draw,
        PlayerWon,
        OpponentWon
    }

    private GameState GetGameState()
    {
        bool teamOneHas = false;
        bool teamTwoHas = false;

        foreach (Pawn pawn in Pawns)
        {
            if (pawn != null)
            {
                teamOneHas = teamOneHas || pawn.Team == 0;
                teamTwoHas = teamTwoHas || pawn.Team == 1;
            }
        }
        if (teamOneHas && teamTwoHas)
        {
            return GameState.Playing;
        }
        if (!teamOneHas && !teamTwoHas)
        {
            return GameState.Draw;
        }
        if(teamOneHas)
        {
            return GameState.PlayerWon;
        }
        return GameState.OpponentWon;
    }

    private void ResolveForces()
    {
        List<CollisionPair> consumed = new();

        foreach (var it in forcePairs)
        {
            if (it.Value.time + 0.07 > Time.time)
            {
                continue;
            }
            consumed.Add(it.Key);

            Pawn first = Pawns[it.Key.FirstID];
            Pawn second = Pawns[it.Key.SecondID];

            float magnitude = Mathf.Log(1.0f + it.Value.impact * 0.5f) * 0.1f; // Added both ways

            if (first != null)
            {
                float teamDamage = first.Team == activeTeam ? 0.5f : 1.0f;

                first.AddDamage(it.Key.SecondDmg * magnitude * teamDamage);
            }
            if (second != null)
            {
                float teamDamage = second.Team == activeTeam ? 0.5f : 1.0f;

                second.AddDamage(it.Key.FirstDmg * magnitude * teamDamage);
            }

            if (first != null || second != null)
            {
                Vector3 pos = first == null ? second.transform.position : second == null ? first.transform.position : (first.transform.position + second.transform.position) * 0.5f;
                PlayCollisionEffects(pos, magnitude);
            }
        }

        foreach (CollisionPair pair in consumed)
        {
            forcePairs.Remove(pair);
        }
    }

    private void PlayCollisionEffects(Vector3 position, float magnitude)
    {

    }

    private void Upkeeep()
    {
        if (upkeep != null)
        {
            return;
        }

        foreach (Pawn pawn in Pawns)
        {
            if (pawn != null && pawn.Team == activeTeam && !pawn.IsStill)
            {
                return;
            }
        }

        foreach (Pawn pawn in Pawns)
        {
            if (pawn != null && pawn.Team == activeTeam && pawn.IsReadyToYeet)
            {
                activeState++;
                return;
            }
        }

        upkeep = StartCoroutine(TurnUpTeam());
    }

    private IEnumerator TurnUpTeam()
    {
        foreach (Pawn pawn in Pawns)
        {
            if (pawn != null && pawn.Team == activeTeam && !pawn.IsReadyToYeet)
            {
                pawn.FlipUp();
            }
        }
        yield return new WaitForSecondsRealtime(2.0f);
        upkeep = null;
        activeState++;
    }

    private void HandlePlayerInput()
    {
        bool canPlay = false;
        foreach (Pawn pawn in Pawns)
        {
            if (pawn != null && pawn.Team == activeTeam && (!pawn.IsStill || pawn.IsReadyToYeet))
            {
                canPlay = true;
                break;
            }
        }

        if (!canPlay)
        {
            activeState++;
            return;
        }

        if (activeTeam == 1)
        {
            foreach (Pawn pawn in Pawns)
            {
                if (pawn != null && pawn.Team == activeTeam && !pawn.IsStill)
                {
                    return;
                }
            }
            EnemyAI.Yeet(this);
            activeState++;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                whoToYeet = hit.transform.GetComponentInParent<Pawn>();

                if (whoToYeet && whoToYeet.Team == activeTeam && whoToYeet.IsReadyToYeet)
                {
                    originalYeetPos = whoToYeet.transform.position;
                    originalYeetRot = whoToYeet.transform.rotation;
                }
                else
                {
                    whoToYeet = null;
                }
            }
        }

        if (Input.GetMouseButton(0) && whoToYeet)
        {
            whoToYeet.GetComponent<Rigidbody>().isKinematic = true;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                lastYeetPoint = hit.point;
            }

            Vector3 yeetToPawn = originalYeetPos - lastYeetPoint;
            yeetToPawn.y = 0.0f;
            float dist = yeetToPawn.magnitude;

            float chargeFactor = Mathf.Clamp01((dist - Deadzone) / DistForMaxForce);

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            whoToYeet.transform.position = originalYeetPos + randomDir * chargeFactor * 0.2f;

            forceArrowRend.enabled = true;
            Vector3 startPos = whoToYeet.transform.position;
            startPos.y = 0.1f;
            forceArrowRend.SetPosition(0, startPos);
            Vector3 endPos = originalYeetPos + yeetToPawn.normalized * chargeFactor * DistForMaxForce;
            endPos.y = 0.1f;
            forceArrowRend.SetPosition(1, endPos);
            forceArrowRend.endColor = Color.Lerp(new Color(0.6f, 1.0f, 0.0f), new Color(1.0f, 0.0f, 0.6f), chargeFactor);
        }

        if (Input.GetMouseButtonUp(0) && whoToYeet)
        {
            whoToYeet.transform.position = originalYeetPos;
            whoToYeet.transform.rotation = originalYeetRot;

            whoToYeet.GetComponent<Rigidbody>().isKinematic = false;

            Vector3 yeetDirection = whoToYeet.transform.position - lastYeetPoint;
            yeetDirection.y = 0.0f;
            float forceFactor = Mathf.Clamp01((yeetDirection.magnitude - Deadzone) / DistForMaxForce);
            whoToYeet.Charging.Play();

            if (forceFactor > 0.01f)
            {
                whoToYeet.Yeet(yeetDirection.normalized * forceFactor * YeetForce);

                activeState++;
            }

            forceArrowRend.enabled = false;
            whoToYeet = null;
        }
    }

    private void ResolveCollisionResponses()
    {
        if (forcePairs.Count > 0)
        {
            return;
        }

        foreach (Pawn p in Pawns)
        {
            if (p != null && p.beingYeeted)
            {
                return;
            }
        }

        activeState = ScuffedState.Upkeep;
        activeTeam = (activeTeam + 1) % 2;
    }

    public void AddForce(Pawn first, Pawn second, float impulse)
    {
        int f = Pawns.IndexOf(first);
        int s = Pawns.IndexOf(second);

        if (f > s)
        {
            f ^= s;
            s ^= f;
            f ^= s;
        }

        CollisionPair collision = new()
        {
            FirstID = f,
            FirstDmg = Pawns[f].EffectiveCollisionDamage,
            SecondID = s,
            SecondDmg = Pawns[s].EffectiveCollisionDamage,
        };

        if (!forcePairs.ContainsKey(collision))
        {
            forcePairs.Add(collision, (Time.time, impulse));
        }
        else
        {
            var (time, forc) = forcePairs[collision];
            forcePairs[collision] = (time, forc + impulse);
        }
    }
}

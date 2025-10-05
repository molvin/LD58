using System.Collections.Generic;
using UnityEngine;

public class ForceYeet : MonoBehaviour
{
    struct CollisionPair
    {
        public int FirstID;
        public float FirstDmg;
        public int SecondID;
        public float SecondDmg;
        public int Yeeter;
    }

    public static ForceYeet Instance;

    public float YeetForce = 25.0f;
    public float DistForMaxForce = 5.0f;

    public List<Pawn> Pawns;
    private Dictionary<CollisionPair, float> forcePairs = new();

    private Pawn whoToYeet;
    private Vector3 lastYeetPoint;
    private Vector3 originalYeetPos;
    private Quaternion originalYeetRot;

    private void Update()
    {
        // NOTE: For hot-reloading
        if (Instance == null)
        {
            Initialize();
        }

        HandlePlayerInput();

        ResolveCollisionResponses();
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (Instance != null)
        {
            Debug.LogError("Singelton");
        }

        Instance = this;

        Pawns = new(FindObjectsByType<Pawn>(FindObjectsSortMode.None));
    }

    private void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                whoToYeet = hit.transform.GetComponentInParent<Pawn>();

                if (whoToYeet)
                {
                    originalYeetPos = whoToYeet.transform.position;
                    originalYeetRot = whoToYeet.transform.rotation;
                }
            }
        }

        if (Input.GetMouseButton(0) && whoToYeet)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                lastYeetPoint = hit.point;
            }

            float dist = Vector3.Distance(whoToYeet.transform.position, lastYeetPoint);

            float chargeFactor = Mathf.Clamp01(dist / DistForMaxForce);

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            whoToYeet.transform.position = originalYeetPos + randomDir * chargeFactor * 0.2f;
        }

        if (Input.GetMouseButtonUp(0) && whoToYeet)
        {
            whoToYeet.transform.position = originalYeetPos;
            whoToYeet.transform.rotation = originalYeetRot;

            Vector3 yeetDirection = whoToYeet.transform.position - lastYeetPoint;
            float forceFactor = Mathf.Clamp01(yeetDirection.magnitude / DistForMaxForce);
            yeetDirection = new Vector3(yeetDirection.x, 0.0f, yeetDirection.z).normalized;
            whoToYeet.Yeet(yeetDirection * forceFactor * YeetForce);
        }
    }

    private void ResolveCollisionResponses()
    {
        if (forcePairs.Count > 0)
        {
            bool resolve = true;
            foreach (Pawn pawn in Pawns)
            {
                if (pawn != null && !pawn.IsStill)
                {
                    resolve = false;
                    break;
                }
            }

            if (resolve)
            {
                foreach (var it in forcePairs)
                {
                    Pawn first = Pawns[it.Key.FirstID];
                    Pawn second = Pawns[it.Key.SecondID];

                    float magnitude = Mathf.Log(1.0f + it.Value) * 0.1f;

                    if (first != null)
                    {
                        if (it.Key.Yeeter == 0)
                        {
                            first.AddDamage(it.Key.SecondDmg * magnitude * 0.2f);
                        }
                        else
                        {
                            first.AddDamage(it.Key.SecondDmg * magnitude);
                        }
                    }
                    if (second != null)
                    {
                        if (it.Key.Yeeter == 1)
                        {
                            second.AddDamage(it.Key.FirstDmg * magnitude * 0.2f);
                        }
                        else
                        {
                            second.AddDamage(it.Key.FirstDmg * magnitude);
                        }
                    }
                }

                forcePairs.Clear();

                for (int i = Pawns.Count- 1; i >= 0; --i)
                {
                    if (Pawns[i] == null)
                    {
                        Pawns.RemoveAt(i);
                        continue;
                    }

                    if (Pawns[i].IsStill && Vector3.Dot(Pawns[i].transform.up, Vector3.up) < 0.99f)
                    {
                        Pawns[i].FlipUp();
                    }
                }
            }
        }
    }

    public void AddForce(Pawn first, Pawn second, float impulse, bool firstIsYeeter)
    {
        int f = Pawns.IndexOf(first);
        int s = Pawns.IndexOf(second);

        int yeet = firstIsYeeter ? 0 : -1;

        if (f > s)
        {
            f ^= s;
            s ^= f;
            f ^= s;

            if (yeet >= 0)
            {
                yeet = 1;
            }
        }

        CollisionPair collision = new()
        {
            FirstID = f,
            FirstDmg = Pawns[f].CollisionForce,
            SecondID = s,
            SecondDmg = Pawns[s].CollisionForce,
            Yeeter = yeet
        };

        if (!forcePairs.ContainsKey(collision))
        {
            forcePairs.Add(collision, impulse);
        }
        else
        {
            forcePairs[collision] += impulse;
        }
    }
}

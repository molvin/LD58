using System.Collections.Generic;
using UnityEngine;

public class ForceCollector : MonoBehaviour
{
    struct CollisionPair
    {
        public int FirstID;
        public int SecondID;
        public int Yeeter;
    }

    public static ForceCollector Instance;

    private List<Pawn> Pawns;
    private Dictionary<CollisionPair, float> forcePairs = new();

    private void Awake()
    {
        Initialize();
    }

    private void FixedUpdate()
    {
        // NOTE: For hot-reloading
        if (Instance == null)
        {
            Initialize();
        }
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

        CollisionPair collision = new() { FirstID = f, SecondID = s, Yeeter = yeet };

        if (!forcePairs.ContainsKey(collision))
        {
            forcePairs.Add(collision, impulse);
        }
        else
        {
            forcePairs[collision] += impulse;
        }
    }

    private void Update()
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
                            first.AddDamage(magnitude * 0.2f);
                        }
                        else
                        {
                            first.AddDamage(magnitude);
                        }
                    }
                    if (second != null)
                    {
                        if (it.Key.Yeeter == 1)
                        {
                            second.AddDamage(magnitude * 0.2f);
                        }
                        else
                        {
                            second.AddDamage(magnitude);
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

}

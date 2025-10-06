using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyAI
{
    struct ActionUtility
    {
        public Pawn pawn;
        public Vector3 vector;
        public float score;
    }

    public static void Yeet(ForceYeet manager)
    {
        List<Pawn> choosable = manager.Pawns.Where(p => p != null && p.Team == 1 && p.IsReadyToYeet).ToList();
        List<Pawn> targets = manager.Pawns.Where(p => p != null && p.Team == 0).ToList();

        List<ActionUtility> actions = new();
        actions.Add(SafePlay(choosable, targets));
        actions.Add(KnockoutShot(choosable, targets));
        actions.Add(PowerPlay(choosable, targets, manager));
        //Debug.LogFormat($"Safey: {actions[0].score}");
        //Debug.LogFormat($"Edging: {actions[1].score}");
        //Debug.LogFormat($"Power: {actions[2].score}");

        int best = 0;
        for (int i = 1; i < actions.Count; i++)
        {
            if (actions[i].score > actions[best].score)
            {
                best = i;
            }
        }

        actions[best].pawn.Yeet(actions[best].vector * manager.YeetForce);
    }

    private static ActionUtility SafePlay(List<Pawn> team, List<Pawn> targets)
    {
        int pawn = 0;
        float score = 0;

        for (int i = 0; i < team.Count; i++)
        {
            Vector3 orig = team[i].transform.position;

            float dist = orig.magnitude / 46.0f;

            RaycastHit hit;
            if (Physics.Raycast(orig, -orig.normalized, out hit, orig.magnitude * 0.9f))
            {
                Pawn p = hit.transform.GetComponent<Pawn>();
                if (p != null && p.Team == 1)
                {
                    dist *= 0.2f;
                }
            }

            if (dist > score)
            {
                pawn = i;
                score = dist;
            }
        }

        ActionUtility utility = new()
        {
            pawn = team[pawn],
            vector = -team[pawn].transform.position.normalized * Mathf.Clamp(0.2f, 0.9f, score * 2.3f),
            score = score,
        };
        return utility;
    }
    private static ActionUtility KnockoutShot(List<Pawn> team, List<Pawn> targets)
    {
        Boundaries bounds = GameObject.FindFirstObjectByType<Boundaries>();

        int bestPawn = -1;
        int bestTarget = -1;
        float bestEdge = 0.0f;

        for (int i = 0; i < team.Count; i++)
        {
            for (int j = 0; j < targets.Count; j++)
            {
                Vector3 origin = team[i].transform.position + Vector3.up * 0.3f;
                Vector3 dir = targets[j].transform.position + Vector3.up * 0.3f - origin;

                RaycastHit hit;
                if (!Physics.Raycast(origin, dir, out hit) || hit.transform == targets[j].transform)
                {
                    float boundsDist = bounds.CheckBoundary(origin, dir) - dir.magnitude;
                    boundsDist += dir.magnitude * 0.2f;

                    if (bestPawn < 0 || bestTarget < 0 || boundsDist < bestEdge)
                    {
                        bestPawn = i;
                        bestTarget = j;
                        bestEdge = boundsDist;
                    }
                }
            }
        }

        if (bestPawn < 0 || bestTarget < 0)
        {
            return new();
        }

        float score = 1.0f - Mathf.Clamp01(bestEdge / 10.0f);
        Vector3 vector = targets[bestTarget].transform.position - team[bestPawn].transform.position;

        ActionUtility utility = new()
        {
            pawn = team[bestPawn],
            vector = vector.normalized,
            score = score
        };
        return utility;
    }

    private static ActionUtility PowerPlay(List<Pawn> team, List<Pawn> targets, ForceYeet manager)
    {
        float Strongest = manager.Pawns.Where(p => p != null && p.Team == 1).Max(p => p.EffectiveAttackDamage);

        if (Strongest == 0)
        {
            return new();
        }

        Pawn bestPawn = null;
        Pawn bestTarget = null;
        float bestScore = 0.0f;

        foreach (Pawn p in team)
        {
            float baseScore = Mathf.Lerp(0.75f, 1.0f, p.EffectiveAttackDamage / Strongest);

            foreach (Pawn target in targets)
            {
                Vector3 toTarget = target.transform.position - p.transform.position;
                float dist = toTarget.magnitude;
                float score = baseScore * (1.0f - Mathf.Clamp01(dist / 10.0f));

                float dirScore = Vector3.Dot(-p.transform.position.normalized, toTarget.normalized);
                dirScore = dirScore * 0.5f + 0.5f;
                dirScore = Mathf.Lerp(0.65f, 1.0f, dirScore);
                score *= dirScore;

                RaycastHit hit;
                if (Physics.Raycast(p.transform.position, toTarget.normalized, out hit, toTarget.magnitude * 0.9f))
                {
                    Pawn hitPawn = hit.transform.GetComponent<Pawn>();
                    if (hitPawn != null && hitPawn.Team == 1)
                    {
                        score *= 0.2f;
                    }
                }

                if (bestPawn == null || score > bestScore)
                {
                    bestPawn = p;
                    bestTarget = target;
                    bestScore = score;
                }
            }
        }

        if (bestPawn == null)
        {
            return new();
        }

        ActionUtility utility = new()
        {
            pawn = bestPawn,
            vector = (bestTarget.transform.position - bestPawn.transform.position).normalized,
            score = bestScore,
        };
        return utility;
    }
}

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
            float dist = team[i].transform.position.magnitude / 30.0f;
            if (dist > score)
            {
                pawn = i;
                score = dist;
            }
        }

        ActionUtility utility = new()
        {
            pawn = team[pawn],
            vector = -team[pawn].transform.position.normalized * Mathf.Clamp01(score * 1.7f),
            score = score,
        };
        return utility;
    }
    private static ActionUtility KnockoutShot(List<Pawn> team, List<Pawn> targets)
    {
        return new();
    }
}

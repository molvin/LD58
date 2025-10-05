using UnityEngine;

[System.Serializable]
public abstract class PawnPrototype
{
    public virtual bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse) { return true; }
}
public class Basic : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        float attackForce = target.DamagePercentage * owner.EffectiveAttackForce;
        target.rigidbody.AddForce(impulse.normalized * attackForce, ForceMode.Impulse);

        target.AddDamage(impulse.magnitude * 0.1f * owner.EffectiveAttackDamage);

        return true;
    }
}

public class Explosion : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn _, Vector3 impulse)
    {
        foreach (Pawn p in owner.Manager.Pawns)
        {
            if (p == null || p.Team == owner.Team)
            {
                continue;
            }

            float radius = 25.0f * owner.RarityFactor;
            float distanceToPawn = Vector3.Distance(owner.transform.position, p.transform.position);
            if (distanceToPawn > radius)
                continue;

            float ratio = 1.0f - Mathf.Clamp01(distanceToPawn / radius);

            p.GetComponent<Rigidbody>().AddExplosionForce(p.DamagePercentage * owner.EffectiveAttackForce, owner.transform.position, radius);
            p.AddDamage(owner.EffectiveAttackDamage * ratio);
        }

        return true;
    }
}

public class Chain : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        float attackForce = target.DamagePercentage * owner.EffectiveAttackForce;
        target.rigidbody.AddForce(impulse.normalized * attackForce, ForceMode.Impulse);

        target.AddDamage(impulse.magnitude * 0.1f * owner.EffectiveAttackDamage);

        // Find new target
        Pawn newTarget = null;
        float bestDist = 0;
        foreach (Pawn p in owner.Manager.Pawns)
        {
            if (p == null || p.Team == owner.Team || p == target)
            {
                continue;
            }

            Vector3 toTarget = p.transform.position - owner.transform.position;

            float dist = toTarget.magnitude;
            RaycastHit hit;
            if (!Physics.Raycast(owner.transform.position, toTarget, out hit) || hit.transform == p.transform)
            {
                dist *= 0.5f;
            }
            if (newTarget == null || dist < bestDist)
            {
                newTarget = p;
                bestDist = dist;
            }
        }

        if (newTarget != null)
        {
            owner.rigidbody.linearVelocity = (newTarget.transform.position - owner.transform.position).normalized * (impulse.magnitude + owner.rigidbody.linearVelocity.magnitude);
        }

        return newTarget != null;
    }
}

public class Libero : PawnPrototype
{
}


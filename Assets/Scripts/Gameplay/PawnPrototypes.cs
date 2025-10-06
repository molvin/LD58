using UnityEngine;

[System.Serializable]
public abstract class PawnPrototype
{
    public virtual bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        float attackForce = target.DamagePercentage * owner.EffectiveAttackForce;
        target.prototype.ApplyAttackForce(target, owner, impulse.normalized * attackForce);

        target.AddDamage(impulse.magnitude * 0.1f * owner.EffectiveAttackDamage);

        return true;
    }

    public virtual void ApplyAttackForce(Pawn owner, Pawn instigator, Vector3 force)
    {
        Vector3 modifiedForce = force;
        foreach (Pawn pawn in owner.Manager.Pawns)
        {
            if (pawn != null)
            {
                modifiedForce = pawn.prototype.ModidyReceiveForce(pawn, owner, modifiedForce);
            }
        }
        owner.rigidbody.AddForce(modifiedForce, ForceMode.Impulse);
    }

    public virtual Vector3 ModidyReceiveForce(Pawn self, Pawn target, Vector3 incomingForce)
    {
        return incomingForce;
    }
    public virtual float ModidyReceiveDamage(Pawn self, Pawn target, float incomingDamage)
    {
        return incomingDamage;
    }

    public virtual void OnDamageTagen(Pawn self) { }
}
public class Basic : PawnPrototype { }

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

            float radius = 8.0f * owner.RarityFactor;
            Vector3 distanceToPawn = p.transform.position - owner.transform.position;
            if (distanceToPawn.magnitude > radius)
                continue;

            float ratio = 1.0f - Mathf.Clamp01(distanceToPawn.magnitude / radius);

            p.prototype.ApplyAttackForce(p, owner, distanceToPawn.normalized * p.DamagePercentage * owner.EffectiveAttackForce * ratio);

            p.AddDamage(owner.EffectiveAttackDamage * ratio);
        }

        return true;
    }
}

public class Chain : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        base.PrimaryYeet(owner, target, impulse);

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

        return newTarget == null;
    }
}

public class Libero : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        return base.PrimaryYeet(owner, target, impulse);
    }
}

public class Boomerang : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        base.PrimaryYeet(owner, target, impulse);

        Vector3 toPreYeet = owner.preYeetPosition - owner.transform.position;
        float yeetForce = Mathf.Clamp01(toPreYeet.magnitude / 6.0f) * 0.7f;
        Vector3 velocity = (toPreYeet * yeetForce) - owner.rigidbody.linearVelocity * 0.4f;
        owner.rigidbody.AddForce(velocity * owner.EffectiveMass, ForceMode.Impulse);

        return true;
    }
}
public class HomeSick : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        base.PrimaryYeet(owner, target, impulse);

        Vector3 toPreYeet = owner.initialStartPosition - owner.transform.position;
        float yeetForce = Mathf.Clamp01(toPreYeet.magnitude / 6.0f) * 0.6f;
        Vector3 velocity = (toPreYeet * yeetForce) - owner.rigidbody.linearVelocity * 0.4f;
        owner.rigidbody.AddForce(velocity * owner.EffectiveMass, ForceMode.Impulse);

        return true;
    }
}

public class Tether : PawnPrototype
{
    public override Vector3 ModidyReceiveForce(Pawn self, Pawn target, Vector3 incomingForce)
    {
        if (self.Team == target.Team && self != target)
        {
            float dist = Vector3.Distance(self.transform.position, target.transform.position);

            float radius = 5.0f * self.RarityFactor;

            if (dist < radius)
            {
                self.rigidbody.AddForce(incomingForce, ForceMode.Impulse);
                incomingForce = Vector3.zero;
            }
        }

        return incomingForce;
    }

    public override float ModidyReceiveDamage(Pawn self, Pawn target, float incomingDamage)
    {
        if (self.Team == target.Team && self != target)
        {
            float dist = Vector3.Distance(self.transform.position, target.transform.position);

            float radius = 5.0f * self.RarityFactor;

            if (dist < radius)
            {
                self.AddDamage(incomingDamage, false);
                incomingDamage = 0.0f;
            }
        }

        return incomingDamage;
    }
}
public class Edging : PawnPrototype
{
    public override bool PrimaryYeet(Pawn owner, Pawn target, Vector3 impulse)
    {
        float attackForce = target.DamagePercentage * owner.EffectiveAttackForce;

        Boundaries bounds = GameObject.FindFirstObjectByType<Boundaries>();
        float boundsDist = bounds.CheckBoundary(Vector3.zero, target.transform.position);
        float distFactor = Mathf.Clamp01(target.transform.position.magnitude / boundsDist);

        target.prototype.ApplyAttackForce(target, owner, impulse.normalized * attackForce * distFactor);

        target.AddDamage(impulse.magnitude * 0.1f * owner.EffectiveAttackDamage);

        return true;
    }
}

public class Monarch : PawnPrototype
{
    public override void OnDamageTagen(Pawn self)
    {
        foreach (Pawn p in self.Manager.Pawns)
        {
            if (p != null && p != self && p.Team == self.Team && !p.IsReadyToYeet)
            {
                p.FlipUp();
            }
        }
    }
}

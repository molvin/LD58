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
        float attackForce = target.DamagePercentage * owner.AttackForce;
        target.rigidbody.AddForce(impulse.normalized * attackForce, ForceMode.Impulse);

        target.AddDamage(impulse.magnitude * 0.1f * owner.AttackDamage);

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

            float radius = 30.0f;
            float distanceToPawn = Vector3.Distance(owner.transform.position, p.transform.position);
            if (distanceToPawn > radius)
                continue;

            float ratio = Mathf.Clamp01(distanceToPawn / radius);

            p.GetComponent<Rigidbody>().AddExplosionForce(owner.AttackForce, owner.transform.position, radius);
            p.AddDamage(owner.AttackDamage * ratio);
        }

        return true;
    }
}

public class Libero : PawnPrototype
{
}


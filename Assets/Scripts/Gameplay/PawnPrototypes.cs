using UnityEngine;

[System.Serializable]
public abstract class PawnPrototype
{
    public virtual void PrimaryYeet(Pawn owner, Pawn trigger) { }
}
public class Basic : PawnPrototype
{
}
public class Power : PawnPrototype
{
}

public class Bomb : PawnPrototype
{
    public override void PrimaryYeet(Pawn owner, Pawn trigger)
    {
        foreach (Pawn p in ForceCollector.Instance.Pawns)
        {
            if (p == null || p == trigger || p.Team == owner.Team)
            {
                continue;
            }

            p.GetComponent<Rigidbody>().AddExplosionForce(100.0f, owner.transform.position, 50.0f);
        }
    }
}

public class Libero : PawnPrototype
{
}


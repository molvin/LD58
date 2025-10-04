using UnityEngine;

[System.Serializable]
public abstract class PawnPrototype
{
    public virtual void Dang() => Debug.Log("Base class");
}
public class BasicPawn : PawnPrototype
{
    public override void Dang()
    {
        Debug.Log("Basic biatch");
    }
}
public class CoolPawn : PawnPrototype
{
    public override void Dang()
    {
        Debug.Log("Super cool stuff!");
    }
}


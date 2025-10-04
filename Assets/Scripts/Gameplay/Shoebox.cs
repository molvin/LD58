using System.Collections.Generic;
using UnityEngine;

public class Shoebox : MonoBehaviour
{
    public List<Pawn> Collection = new();
    public float PawnScale = 0.05f;

    public BoxCollider SpawnArea;

    private List<Pawn> spawned = new();

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            RespawnAll();
        }
    }

    public void RespawnAll()
    {
        foreach(Pawn pawn in spawned)
        {
            Destroy(pawn.gameObject);
        }
        spawned = new();

        foreach(Pawn prefab in Collection)
        {
            Pawn pawn = Instantiate(prefab, transform);
            pawn.enabled = false;
            pawn.transform.position = RandomPointInBounds(SpawnArea.bounds);
            pawn.transform.rotation = Random.rotation;
            pawn.transform.localScale = Vector3.one * PawnScale;
            spawned.Add(pawn);
        }
    }
    
    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}

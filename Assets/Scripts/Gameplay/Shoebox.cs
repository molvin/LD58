using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Shoebox : MonoBehaviour
{
    public List<int> Collection = new();
    public float PawnScale = 0.05f;

    public BoxCollider SpawnArea;
    public Animator Anim;
    public Transform HoverPlanePoint;
    public PlaceableAreas PlaceableAreas;

    private List<Pawn> spawned = new();

    public List<Pawn> Team;

    public GachaMachine GachaMachine;

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
            if(pawn != null)
            {
                Destroy(pawn.gameObject);
            }
        }
        spawned = new();

        foreach(int index in Collection)
        {
            Pawn prefab = GachaMachine.Prefabs[index];
            Pawn pawn = Instantiate(prefab, transform);
            pawn.PrefabId = index;
            pawn.enabled = false;
            pawn.transform.position = RandomPointInBounds(SpawnArea.bounds);
            pawn.transform.rotation = Random.rotation;
            pawn.transform.localScale = Vector3.one * PawnScale;
            spawned.Add(pawn);
        }
    }
        
    public async Awaitable PickTeam()
    {
        Anim.SetBool("Shown", true);

        Team = new();
        Pawn pickup = null;
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                break;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, HoverPlanePoint.position);

            if (Input.GetMouseButtonDown(0))
            {
                foreach (Pawn pawn in spawned)
                {
                    bool hit = pawn.PickupCollider.Raycast(ray, out RaycastHit hitInfo, 1000.0f);
                    if(hit)
                    {
                        pickup = pawn;

                        pickup.PickUp();
                        if (Team.Contains(pickup))
                        {
                            Team.Remove(pickup);
                            pickup.transform.SetParent(transform);
                        }
                        break;
                    }
                }
            }

            if (pickup)
            {
                if(!Input.GetMouseButton(0))
                {
                    // TODO: make sure place is unoccupied
                    (bool valid, Vector3 point) = PlaceableAreas.Valid(pickup.transform.position);
                    if (valid && Team.Count < 5)
                    {
                        pickup.transform.SetParent(null);
                        pickup.transform.rotation = Quaternion.identity;
                        pickup.transform.position = point;
                        Team.Add(pickup);
                    }
                    else
                    {
                        pickup.transform.position = HoverPlanePoint.position;
                    }
                    pickup.Drop();
                    pickup = null;
                }
                else
                {
                    plane.Raycast(ray, out float enter);
                    Vector3 pos = ray.GetPoint(enter);

                    pickup.transform.position = pos;
                }
            }

            await Awaitable.NextFrameAsync();
        }

        Anim.SetBool("Shown", false);
        await Awaitable.WaitForSecondsAsync(1.0f);
    }

    private static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}

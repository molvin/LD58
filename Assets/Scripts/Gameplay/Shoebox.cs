using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shoebox : MonoBehaviour
{
    public List<Database.PawnDB> Collection = new();
    public float PawnScale = 0.05f;

    public BoxCollider SpawnArea;
    public Animator Anim;
    public Transform HoverPlanePoint;
    public PlaceableAreas PlaceableAreas;
    public float InspectMaxHoldTime = 0.1f;
    public PawnInspector Inspector;

    private List<Pawn> spawned = new();

    public List<Pawn> Team;

    public GachaMachine GachaMachine;
    public Button ReadyButton;
    public BoxCollider Bounds;

    public Collider TrashCan;
    public int MaxPawns = 10;

    public float SmoothTime = 0.05f;
    private Vector3 velocity;

    private void Start()
    {
        ReadyButton.gameObject.SetActive(false);
        TrashCan.gameObject.SetActive(false);
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

        for (int i = 0; i < Collection.Count; i++)
        {
            Database.PawnDB dbPawn = Collection[i];
            Pawn prefab = GachaMachine.Prefabs[dbPawn.PawnType];
            Pawn pawn = Instantiate(prefab, transform);
            pawn.PrefabId = dbPawn.PawnType;

            pawn.Rarity = (PawnRarity)dbPawn.Rarity;
            pawn.ColorValue = dbPawn.Color;
            pawn.InitializeVisuals();

            pawn.enabled = false;
            pawn.transform.position = SpawnArea.bounds.RandomPointInBounds();
            pawn.transform.rotation = Random.rotation;
            pawn.transform.localScale = Vector3.one * PawnScale;
            pawn.CollectionIdRef = i;
            spawned.Add(pawn);
        }
    }

    private void ReturnIfOutside()
    {
        foreach(Pawn pawn in spawned)
        {
            if (Team.Contains(pawn))
                continue;
            if (!Bounds.bounds.Contains(pawn.transform.position))
            {
                if(pawn.TimeOutsideShoebox > 3)
                {
                    pawn.transform.position = HoverPlanePoint.position;
                    pawn.Drop();
                    pawn.rigidbody.angularVelocity = Vector3.zero;
                    pawn.rigidbody.linearVelocity = Vector3.zero;
                    pawn.TimeOutsideShoebox = 0.0f;
                }
                pawn.TimeOutsideShoebox += Time.deltaTime;
            }
            else
            {
                pawn.TimeOutsideShoebox = 0.0f;
            }
        }
    }
        
    private async Awaitable PruneTeam()
    {
        TrashCan.gameObject.SetActive(true);

        int delta = Collection.Count - MaxPawns;
        Pawn pickup = null;
        while (delta > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new (Vector3.up, HoverPlanePoint.position);
                      
            if (pickup != null)
            {
                if (!Input.GetMouseButton(0))
                {
                    if (TrashCan.Raycast(ray, out RaycastHit _, 1000.0f))
                    {
                        Destroy(pickup.gameObject);
                        spawned.Remove(pickup);
                        Collection[pickup.CollectionIdRef] = null;
                        delta--;
                    }
                    else
                    {
                        pickup.transform.position = HoverPlanePoint.position;
                        pickup.Drop();
                        pickup.rigidbody.angularVelocity = Vector3.zero;
                        pickup.rigidbody.linearVelocity = Vector3.zero;
                    }
                    pickup = null;
                }
                else
                {
                    plane.Raycast(ray, out float enter);
                    Vector3 pos = ray.GetPoint(enter);
                    pickup.transform.position = Vector3.SmoothDamp(pickup.transform.position, pos, ref velocity, SmoothTime);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    foreach (Pawn pawn in spawned)
                    {
                        bool hit = pawn.PickupCollider.Raycast(ray, out RaycastHit hitInfo, 1000.0f);
                        if (hit)
                        {
                            pickup = await PickupOrInspect(pawn, plane, ray);
                            break;
                        }
                    }
                }
            }

            ReturnIfOutside();

            await Awaitable.NextFrameAsync();
        }

        for (int i = Collection.Count - 1; i >= 0; i--)
        {
            if (Collection[i] == null)
                Collection.RemoveAt(i);
        }

        TrashCan.gameObject.SetActive(false);

    }

    public async Awaitable PickTeam()
    {
        ReadyButton.interactable = false;
        Anim.SetBool("Shown", true);
        await Awaitable.WaitForSecondsAsync(1);

        if(Collection.Count > MaxPawns)
        {
            await PruneTeam();
        }

        Team = new();
        Pawn pickup = null;
        bool done = false;
        ReadyButton.gameObject.SetActive(true);
        ReadyButton.onClick.AddListener(() => done = true);
        while (!done)
        {
            ReadyButton.interactable = Team.Count == 5;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, HoverPlanePoint.position);

            if (pickup)
            {
                (bool valid, Vector3 point) = PlaceableAreas.Valid(pickup.transform.position);
                pickup.GetComponentInChildren<DropShadow>().Renderer.enabled = valid;

                if (!Input.GetMouseButton(0))
                {
                    if(valid)
                    {
                        // Make sure you dont place teammates too close to eachother
                        foreach(Pawn teamMate in Team)
                        {
                            if (Vector3.Distance(teamMate.transform.position, point) < pickup.PickupCollider.radius)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                    
                    if (valid && Team.Count < 5)
                    {
                        pickup.transform.SetParent(null);
                        pickup.transform.rotation = Quaternion.identity;
                        pickup.transform.position = point;
                        Team.Add(pickup);
                    }
                    else
                    {
                        pickup.Drop();
                        pickup.rigidbody.linearVelocity = velocity;
                    }
                    pickup.GetComponentInChildren<DropShadow>().Renderer.enabled = false;
                    pickup = null;
                }
                else
                {
                    plane.Raycast(ray, out float enter);
                    Vector3 pos = ray.GetPoint(enter);
                    pickup.transform.position = Vector3.SmoothDamp(pickup.transform.position, pos, ref velocity, SmoothTime);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    foreach (Pawn pawn in spawned)
                    {
                        bool hit = pawn.PickupCollider.Raycast(ray, out RaycastHit hitInfo, 1000.0f);
                        if (hit)
                        {
                            pickup = await PickupOrInspect(pawn, plane, ray);
                            break;
                        }
                    }
                }
            }

            ReturnIfOutside();

            await Awaitable.NextFrameAsync();
        }

        ReadyButton.onClick.RemoveAllListeners();
        ReadyButton.gameObject.SetActive(false);

        Anim.SetBool("Shown", false);

        await Awaitable.WaitForSecondsAsync(1.0f);

        foreach(Pawn pawn in spawned)
        {
            if(pawn != null && !Team.Contains(pawn))
            {
                Destroy(pawn.gameObject);
            }
        }

        spawned.Clear();
    }

    private async Awaitable<Pawn> PickupOrInspect(Pawn pawn, Plane plane, Ray ray)
    {
        float t = 0.0f;
        while (t < InspectMaxHoldTime)
        {
            if(!Input.GetMouseButton(0))
            {
                await Inspector.Inspect(pawn, false, false);
                return null;
            }
            t += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        pawn.PickUp();
        velocity = Vector3.zero;
        plane.Raycast(ray, out float enter);
        pawn.transform.position = ray.GetPoint(enter);

        if (Team.Contains(pawn))
        {
            Team.Remove(pawn);
            pawn.transform.SetParent(transform);
        }
        return pawn;
    }
}

public static class BoundExtensions
{
    public static Vector3 RandomPointInBounds(this Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
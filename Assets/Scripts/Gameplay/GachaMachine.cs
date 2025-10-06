using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GachaMachine : MonoBehaviour
{
    public int Tokens = 5;

    public int Cost = 1;

    public float SpawnForce = 250;
    public List<Pawn> Prefabs;
    public GachaBall GachaPrefab;
    public Transform SpawnPoint;
    public PawnInspector Inspector;
    public Shoebox ShoeBox;
    public Animator Anim;
    public float SpinAnimationDuration;
    public ParticleSystem BallPresentationCelebration;
    public SphereCollider LeverInteractionCollider;
    public Button DoneButton;
    public Notepad Notepad;

    private List<(Pawn, PawnRarity)> prefabPool;
    private List<GachaBall> gachaBalls = new();

    private void Start()
    {
        InitPool();
    }

    private void InitPool()
    {
        prefabPool = new();

        foreach(Pawn pawn in Prefabs)
        {
            //Add all rarities to pool
            for (int i = 0; i < 15; i++)
                prefabPool.Add((pawn, PawnRarity.Common));
            for (int i = 0; i < 8; i++)
                prefabPool.Add((pawn, PawnRarity.Uncommon));
            for (int i = 0; i < 5; i++)
                prefabPool.Add((pawn, PawnRarity.Rare));
            for (int i = 0; i < 3; i++)
                prefabPool.Add((pawn, PawnRarity.Epic));  
        }
    }

    public async Awaitable RunGacha()
    {

        Anim.SetBool("Shown", true);
        await Awaitable.WaitForSecondsAsync(1);

        bool done = false;
        DoneButton.onClick.AddListener(() => done = true);

        while(!done)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                if(LeverInteractionCollider.Raycast(ray, out RaycastHit _, 10000.0f))
                {
                    await Roll();
                }
                else
                {
                    int i = 0;
                    for (; i < gachaBalls.Count; i++)
                    {
                        GachaBall gacha = gachaBalls[i];
                        bool hit = gacha.ClickCollider.Raycast(ray, out _, 1000.0f);
                        if (hit)
                        {
                            await Inspect(gacha);
                            Destroy(gacha.gameObject);

                            break;
                        }
                    }
                    if (i < gachaBalls.Count)
                    {
                        gachaBalls.RemoveAt(i);
                    }
                }
            }

            await Awaitable.NextFrameAsync();
        }
        DoneButton.onClick.RemoveAllListeners();

        foreach(GachaBall gachaBall in gachaBalls)
        {
            Destroy(gachaBall.gameObject);
        }
        gachaBalls.Clear();

        Anim.SetBool("Shown", false);
        await Awaitable.WaitForSecondsAsync(1);
    }

    public async Awaitable Roll()
    {
        if(Tokens < Cost)
        {
            return;
        }
        Tokens -= Cost;
        Notepad.PlayerCard.UpdateCoins(Tokens);

        if(prefabPool.Count == 0)
        {
            InitPool();
        }

        Anim.SetTrigger("Spin");
        await Awaitable.WaitForSecondsAsync(SpinAnimationDuration);
        int index = Random.Range(0, prefabPool.Count);
        (Pawn, PawnRarity) prefab = prefabPool[index];
        prefabPool.RemoveAt(index);

        GachaBall ball = Instantiate(GachaPrefab, SpawnPoint.position + Random.insideUnitSphere * 0.1f, SpawnPoint.rotation);
        ball.GetComponent<Rigidbody>().AddForce(SpawnPoint.forward * SpawnForce);
        ball.Prefab = prefab.Item1;
        ball.Rarity = prefab.Item2;
        gachaBalls.Add(ball);
        BallPresentationCelebration.Play();
    }

    public async Awaitable Inspect(GachaBall ball)
    {
        Pawn pawn = Instantiate(ball.Prefab);
        pawn.GetComponent<Rigidbody>().isKinematic = true;
        pawn.Rarity = ball.Rarity;
        pawn.ColorValue = Random.Range(0, 256);
        pawn.InitializeVisuals();
        await Inspector.Inspect(pawn);

        // TODO: add to collection and stuff

        bool newEntry = Notepad.AddToCollection(Prefabs.IndexOf(ball.Prefab), (int)ball.Rarity);
        // TODO: do something with this?

        ShoeBox.Collection.Add(Prefabs.IndexOf(ball.Prefab));
    }

}

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
    public TextMeshProUGUI TokensText;
    public Shoebox ShoeBox;
    public Animator Anim;
    public float SpinAnimationDuration;
    public ParticleSystem BallPresentationCelebration;
    public SphereCollider LeverInteractionCollider;
    public Button DoneButton;

    private List<Pawn> prefabPool;
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
            int amount;
            switch (pawn.Rarity)
            {
                case PawnRarity.Uncommon:
                    amount = 8;
                    break;
                case PawnRarity.Rare:
                    amount = 5;
                    break;
                case PawnRarity.Epic:
                    amount = 3;
                    break;
                case PawnRarity.Shiny:
                    amount = 1;
                    break;

                case PawnRarity.Common:
                default:
                    amount = 15;
                    break;
            }

            for(int i = 0; i < amount; i++)
            {
                prefabPool.Add(pawn);
            }
        }
    }

    public async Awaitable RunGacha()
    {
        TokensText.text = $"Tokens: {Tokens}";

        Anim.SetBool("Shown", true);
        await Awaitable.WaitForSecondsAsync(1);

        bool done = false;
        DoneButton.onClick.AddListener(() => done = true);

        while(!done)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            TokensText.text = $"Tokens: {Tokens}";

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
                            await Inspect(gacha.Prefab);
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

        if(prefabPool.Count == 0)
        {
            InitPool();
        }

        Anim.SetTrigger("Spin");
        await Awaitable.WaitForSecondsAsync(SpinAnimationDuration);

        int index = Random.Range(0, prefabPool.Count);
        Pawn prefab = prefabPool[index];
        prefabPool.RemoveAt(index);

        GachaBall ball = Instantiate(GachaPrefab, SpawnPoint.position + Random.insideUnitSphere * 0.1f, SpawnPoint.rotation);
        ball.GetComponent<Rigidbody>().AddForce(SpawnPoint.forward * SpawnForce);
        ball.Prefab = prefab;
        gachaBalls.Add(ball);
        BallPresentationCelebration.Play();
    }

    public async Awaitable Inspect(Pawn prefab)
    {
        Pawn pawn = Instantiate(prefab);
        pawn.GetComponent<Rigidbody>().isKinematic = true;
        await Inspector.Inspect(pawn);

        // TODO: add to collection and stuff
        ShoeBox.Collection.Add(Prefabs.IndexOf(prefab));
    }

}

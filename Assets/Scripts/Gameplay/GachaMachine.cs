using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class GachaMachine : MonoBehaviour
{
    public int Tokens = 5;
    public int Cost = 1;

    public float SpawnForce = 250;
    public Pawn[] Prefabs;
    public GachaBall GachaPrefab;
    public Transform SpawnPoint;
    public PawnInspector Inspector;
    public TextMeshProUGUI TokensText;
    public Shoebox ShoeBox;
    public Animator Anim;
    public float SpinAnimationDuration;

    private List<Pawn> prefabPool;
    private List<GachaBall> gachaBalls = new();
    private bool rolling;

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

    private void Update()
    {
        TokensText.text = $"Tokens: {Tokens}";

        if(!rolling && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Roll());
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int i = 0;
            for (; i < gachaBalls.Count; i++)
            {
                GachaBall gacha = gachaBalls[i];
                bool hit = gacha.ClickCollider.Raycast(ray, out _, 1000.0f);
                if (hit)
                {
                    Inspect(gacha.Prefab);
                    Destroy(gacha.gameObject);
                    break;
                }
            }
            if(i < gachaBalls.Count)
            {
                gachaBalls.RemoveAt(i);
            }
        }
    }

    public IEnumerator Roll()
    {
        rolling = true;

        if(Tokens < Cost)
        {
            yield break;
        }
        Tokens -= Cost;

        if(prefabPool.Count == 0)
        {
            InitPool();
        }

        Anim.SetTrigger("Spin");
        yield return new WaitForSeconds(SpinAnimationDuration);

        int index = Random.Range(0, prefabPool.Count);
        Pawn prefab = prefabPool[index];
        prefabPool.RemoveAt(index);

        GachaBall ball = Instantiate(GachaPrefab, SpawnPoint.position + Random.insideUnitSphere * 0.1f, SpawnPoint.rotation);
        ball.GetComponent<Rigidbody>().AddForce(SpawnPoint.forward * SpawnForce);
        ball.Prefab = prefab;
        gachaBalls.Add(ball);

        rolling = false;
    }

    public void Inspect(Pawn prefab)
    {

        IEnumerator Inspect()
        {
            yield return Inspector.Inspect(Instantiate(prefab));

            // TODO: add to collection and stuff
            ShoeBox.Collection.Add(prefab);
        }

        StartCoroutine(Inspect());
    }

}

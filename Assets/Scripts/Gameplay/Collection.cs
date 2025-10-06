using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class Collection : MonoBehaviour
{
    public Button LeftButton, RightButton;
    public TextMeshProUGUI PageText;
    public RectTransform[] Points;
    public CollectionEntry EntryPrefab;
    public CollectionEntry[] DiscoveredEntryPrefabs;

    public AudioEvent PageTurning;
    public AudioEvent PenScribble;

    public Notepad Notepad;

    public const int EntriesPerPage = 3;
    public const int Pages = 7;
    private List<CollectionEntry> entries = new();

    private int currentPage;
    private bool turningPage;

    private void Start()
    {
        RightButton.onClick.AddListener(() => StartCoroutine(TurnPage(1)));
        LeftButton.onClick.AddListener(() => StartCoroutine(TurnPage(-1)));
    }

    public void OnEnable()
    {
        StartCoroutine(TurnPage(0));
    }

    private async Awaitable TurnPage(int direction)
    {
        if(turningPage)
        {
            return;
        }

        if (PageTurning != null)
            AudioManager.Play(PageTurning, Vector3.zero);
        currentPage += direction;
        if (currentPage < 0)
        {
            currentPage = 0;
            return;
        }
        if (currentPage >= Pages)
        {
            currentPage = Pages - 1;
            return;
        }
        
        turningPage = true;

        PageText.text = $"{currentPage + 1}/{Pages}";

        foreach(CollectionEntry entry in entries)
        {
            Destroy(entry.gameObject);
        }
        entries = new();

        for(int i = 0; i < EntriesPerPage; i++)
        {
            // TODO: check if is discovered, and how discovered it is
            int index = currentPage * 3 + i;

            bool[] discovered = new bool[4];
            for(int j = 0; j < 4; j++)
            {
                discovered[j] = Notepad.PlayerData.Collection.Contains((byte)(index * 4 + j));
            }
            CollectionEntry entry;
            if (discovered.Any(b => b))
            {
                entry = Instantiate(DiscoveredEntryPrefabs[index], Points[i]);
                entry.Init(Notepad.Gacha.Prefabs[index], discovered);
            }
            else
            {
                entry = Instantiate(EntryPrefab, Points[i]);
                entry.Init(null, discovered);
            }
            entries.Add(entry);
            await Awaitable.WaitForSecondsAsync(0.1f);
        }
        turningPage = false;
    }
}

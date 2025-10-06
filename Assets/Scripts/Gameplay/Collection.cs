using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Collection : MonoBehaviour
{
    public Button LeftButton, RightButton;
    public TextMeshProUGUI PageText;
    public RectTransform[] Points;
    public CollectionEntry EntryPrefab;
    public Notepad Notepad;

    public const int EntriesPerPage = 3;
    public const int Pages = 7;
    private List<CollectionEntry> entries = new();

    private int currentPage;
    private bool turningPage;

    private void Start()
    {
        StartCoroutine(TurnPage(0));
        RightButton.onClick.AddListener(() => StartCoroutine(TurnPage(1)));
        LeftButton.onClick.AddListener(() => StartCoroutine(TurnPage(-1)));
    }
    
    private async Awaitable TurnPage(int direction)
    {
        if(turningPage)
        {
            return;
        }
        turningPage = true;
        
        currentPage += direction;
        if (currentPage < 0)
            currentPage = 0;
        if (currentPage >= Pages)
            currentPage = Pages - 1;
        PageText.text = $"{currentPage + 1}/{Pages}";

        foreach(CollectionEntry entry in entries)
        {
            Destroy(entry.gameObject);
        }
        entries = new();

        for(int i = 0; i < EntriesPerPage; i++)
        {
            CollectionEntry entry = Instantiate(EntryPrefab, Points[i]);
            entries.Add(entry);
            await Awaitable.WaitForSecondsAsync(0.1f);
            // TODO: set info
        }
        turningPage = false;
    }
}

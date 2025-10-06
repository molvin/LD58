using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static Database;

public class PlayerCard : MonoBehaviour
{
    public string Name;
    public TMP_InputField InputField;

    public List<Sprite> Bordrers;
    public List<TMP_FontAsset> Fonts;
    public Image Border;
    public Transform StickerArea;
    public Button StartButton;
    public Button QuitButton;
    public Button AbandonButton;
    public Button RightBorderButton;
    public Button LeftBorderButton;
    public Notepad Notepad;
    public GameObject Editor;
    public bool IsOpponent;

    public TextMeshProUGUI CoinText;
    public GameObject[] Hearts;

    public Collider StickerAreaCollider;
    public Collider StickerDiscardArea;
    public Sticker[] StickerSpawners;
    public TextMeshProUGUI CollectionText;

    public AudioEvent PageTurning;
    public AudioEvent PenScribble;

    private Sticker heldSticker;

    private int borderIndex;
    private List<Sticker> stickers = new();

    
    // TODO: stickers
    // TODO: font
    // TODO: lives
    // TODO: coins

    private void Awake()
    {
        if (IsOpponent)
            return;

        // TODO read name from playerprefs, or server
        UpdateName("");

        InputField.onValueChanged.AddListener(UpdateName);
        StartButton.onClick.AddListener(StartGame);
        QuitButton.onClick.AddListener(QuitGame);
        AbandonButton.onClick.AddListener(Notepad.BackToMenu);
        RightBorderButton.onClick.AddListener(() => SwitchBorder(1));
        LeftBorderButton.onClick.AddListener(() => SwitchBorder(-1));
    }

    public void Init(PlayerCardDB card)
    {
        InputField.text = card.Name;

        if(card.Font < 0 || card.Font >= Fonts.Count)
        {
            Debug.LogError($"Server gave wrong font {card.Font}");
            InputField.fontAsset = Fonts[0];
        }
        else
        {
            InputField.fontAsset = Fonts[card.Font];
        }


        //border
        borderIndex = card.Boarder;
        Border.sprite = Bordrers[borderIndex];

        foreach(Sticker sticker in stickers)
        {
            Destroy(sticker.gameObject);
        }
        stickers.Clear();

        foreach(StickerDB stickerDb in card.Stickers)
        {
            if(stickerDb.StickerType >= StickerSpawners.Length)
            {
                continue;
            }
            Sticker sticker = Instantiate(StickerSpawners[stickerDb.StickerType], StickerArea);
            sticker.gameObject.SetActive(true);
            sticker.transform.localPosition = stickerDb.Location;
            sticker.IsSpawner = false;
            sticker.GetComponentInChildren<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            stickers.Add(sticker);
        }
    }

    private void Update()
    {
        if (Notepad == null)
            return;

        if (Notepad.PlayerData != null && Notepad.PlayerData.Collection != null)
        {
            CollectionText.text = $"Collected: {Notepad.PlayerData.Collection.Count}/84";
        }

        if (Notepad.InGame)
            return;

        if (heldSticker == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                foreach (Sticker stickerSpawner in StickerSpawners)
                {
                    if (stickerSpawner.Hovering())
                    {
                        heldSticker = Instantiate(stickerSpawner, StickerArea);
                        heldSticker.transform.position = stickerSpawner.transform.position;
                        heldSticker.transform.rotation = stickerSpawner.transform.rotation;
                        heldSticker.IsSpawner = false;
                    }
                }
                for (int i = stickers.Count - 1; i >= 0; i--)
                {
                    Sticker sticker = stickers[i];
                    if (sticker.Hovering())
                    {
                        heldSticker = sticker;
                        stickers.RemoveAt(i);
                    }
                }
            }
        }
        else
        {
            Plane plane = new Plane(StickerArea.forward, StickerArea.transform.position + StickerArea.forward * 0.1f);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            plane.Raycast(ray, out float enter);
            Vector3 point = ray.GetPoint(enter);

            heldSticker.transform.position = point;

            bool hoveringCover = StickerAreaCollider.Raycast(ray, out RaycastHit _, 10000.0f);
            heldSticker.GetComponentInChildren<SpriteRenderer>().maskInteraction = hoveringCover ? SpriteMaskInteraction.VisibleInsideMask :SpriteMaskInteraction.None;

            if(Input.GetMouseButtonUp(0))
            {
                if(hoveringCover)
                {
                    stickers.Add(heldSticker);
                }
                else
                {
                    Destroy(heldSticker.gameObject);
                }
                heldSticker = null;
            }
        }
    }

    public PlayerCardDB GetPlayerCard()
    {
        List<StickerDB> dbStickers = new();
        foreach(Sticker sticker in stickers)
        {
            StickerDB stickerDB = new StickerDB()
            {
                Location = sticker.transform.localPosition,
                Rotation = 0,
                StickerType = sticker.Index
            };
            dbStickers.Add(stickerDB);
        }

        return new PlayerCardDB()
        {
            Name = Name,
            Boarder = borderIndex,
            Font = borderIndex,
            Stickers = dbStickers
        };
    }

    private void UpdateName(string name)
    {
        Name = name;
        bool valid = Name.Length >= 3;

        // TODO: update in server or something
        StartButton.interactable = valid;
    }

    public void SetInteractable(bool interactable)
    {
        InputField.interactable = interactable;
    }

    public void Show(bool inGame)
    {
        AbandonButton.gameObject.SetActive(inGame);

        StartButton.gameObject.SetActive(!inGame);
        QuitButton.gameObject.SetActive(!inGame);
        Editor.SetActive(!inGame);
        RightBorderButton.gameObject.SetActive(!inGame);
        LeftBorderButton.gameObject.SetActive(!inGame);
    }

    private void QuitGame()
    {
        if (PageTurning != null)
            AudioManager.Play(PageTurning, Vector3.zero);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void StartGame()
    {
        if (PageTurning != null)
            AudioManager.Play(PageTurning, Vector3.zero);
        Notepad.StartGame();
    }

    private void SwitchBorder(int direction)
    {
        borderIndex += direction;
        if (PageTurning != null)
            AudioManager.Play(PageTurning, Vector3.zero);
        if (borderIndex < 0)
        {
            borderIndex = Bordrers.Count - 1;
        }
        else if(borderIndex >= Bordrers.Count)
        {
            borderIndex = 0;
        }

        InputField.fontAsset = Fonts[borderIndex];
        Border.sprite = Bordrers[borderIndex];
    }

    public void UpdateHealth(int health)
    {
        for(int i = 0; i < 3; i++)
        {
            Hearts[i].SetActive(health > i);
        }
    }

    public void UpdateCoins(int coins)
    {
        CoinText.text = coins.ToString();
    }
}

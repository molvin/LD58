using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static Database;

public class PlayerCard : MonoBehaviour
{
    public string Name;
    public TMP_InputField InputField;

    public List<Sprite> Stickers, Bordrers;
    public List<TMP_FontAsset> Fonts;
    public Image Border;
    public Transform StickerArea;
    public SpriteRenderer StickerPrefab;
    public Button StartButton;
    public Button QuitButton;
    public Button AbandonButton;
    public Button RightBorderButton;
    public Button LeftBorderButton;
    public Notepad Notepad;
    public GameObject Editor;
    public bool IsOpponent;

    private int borderIndex;
    
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
        InputField.fontAsset = Fonts[card.Font];

        //border
        borderIndex = card.Boarder;
        Border.sprite = Bordrers[borderIndex];

        // TODO: set stickers

    }

    public PlayerCardDB GetPlayerCard()
    {
        return new PlayerCardDB()
        {
            Name = Name,
            Boarder = borderIndex,
            Font = 0,
            Stickers = new()
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void StartGame()
    {
        Notepad.StartGame();
    }

    private void SwitchBorder(int direction)
    {
        borderIndex += direction;
        if (borderIndex < 0)
        {
            borderIndex = Bordrers.Count - 1;
        }
        else if(borderIndex >= Bordrers.Count)
        {
            borderIndex = 0;
        }

        Border.sprite = Bordrers[borderIndex];
    }
}

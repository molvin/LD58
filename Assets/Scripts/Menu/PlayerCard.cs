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
    public SpriteRenderer Border;
    public Transform StickerArea;
    public SpriteRenderer StickerPrefab;
    public Button StartButton;
    public Button QuitButton;
    public Button AbandonButton;
    public Notepad Notepad;
    public GameObject Editor;
    public bool IsOpponent;
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
    }

    public void Init(PlayerCardDB card)
    {
        InputField.text = card.Name;
        InputField.fontAsset = Fonts[card.Font];

        //border
        Border.sprite = Bordrers[card.Boarder];

        // TODO: set stickers

    }

    public PlayerCardDB GetPlayerCard()
    {
        return new PlayerCardDB()
        {
            Name = Name,
            Boarder = 0,
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
        StartButton.gameObject.SetActive(!inGame);
        QuitButton.gameObject.SetActive(!inGame);
        AbandonButton.gameObject.SetActive(inGame);
        Editor.SetActive(!inGame);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartGame()
    {
        Notepad.StartGame();
    }
}

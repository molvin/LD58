using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class PlayerCard : MonoBehaviour
{
    public TMP_InputField NameInput;

    public Database.PlayerCardDB playerCardTEST;

    public string Name;
    public System.Action<bool> OnNameChanged;
    public TMP_InputField InputField;

    public TMP_Text NameText;
    public List<Sprite> Stickers, Bordrers;
    public List<TMP_FontAsset> Fonts;
    public SpriteRenderer Border;
    public SpriteRenderer StickerPrefab;

    private List<SpriteRenderer> strickerPrefabs;
    private void Start()
    {
        // TODO read name from playerprefs, or server
        UpdateName("");
        NameInput.onValueChanged.AddListener(UpdateName);
    }
    [ContextMenu("test")]
    public void test()
    {
        UpdateVisuals(playerCardTEST);
    }
    public void UpdateVisuals(Database.PlayerCardDB card)
    {
        NameText.text = card.Name;
        NameText.font = Fonts[card.Font];

        //border
        Border.sprite = GetBorderSprite(card);

        //Have the right amount of renderers
        int delta = card.Stickers.Count - strickerPrefabs.Count;
        if (delta > 0)
        {
            for (int i = 0; i < delta; i++)
                strickerPrefabs.Add(Instantiate(StickerPrefab));
        }
        else if(delta < 0)
        {
            for (int i = 0; i < -delta; i++)
                strickerPrefabs.RemoveAt(i);
        }
          
        //Update all stickers
        for (int i = 0;i<card.Stickers.Count;i++)
        {
            Database.StickerDB stick = card.Stickers[i];
            Sprite s = GetSprite(stick);
            strickerPrefabs[i].sprite = s;
            strickerPrefabs[i].transform.localPosition = stick.Location;
            strickerPrefabs[i].transform.rotation = Quaternion.Euler(0, stick.Rotation, 0);
        }
    }

    private Sprite GetSprite(Database.StickerDB sticker)
    {
        if (sticker.StickerType < 0 || sticker.StickerType >= Stickers.Count)
            return Stickers[sticker.StickerType % Stickers.Count];
        else
            return Stickers[sticker.StickerType];
    }
    private Sprite GetBorderSprite(Database.PlayerCardDB card)
    {
        if (card.Boarder < 0 || card.Boarder >= Bordrers.Count)
            return Bordrers[card.Boarder % Bordrers.Count];
        else
            return Bordrers[card.Boarder];
    }

    private void UpdateName(string name)
    {
        Name = name;
        bool valid = Name.Length >= 3;

        // TODO: update in server or something

        OnNameChanged?.Invoke(valid);
    }

    public void SetInteractable(bool interactable)
    {
        NameInput.interactable = interactable;
    }
}

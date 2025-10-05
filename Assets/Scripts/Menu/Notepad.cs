using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using static Database;

public class Notepad : MonoBehaviour
{
    public Button MainButton;
    public Button SettingsButton;
    public Button CollectionButton;
    public PlayerCard PlayerCard;

    public MainMenu Main;
    public GameObject Settings;
    public GameObject Collection;

    public Animator Anim;
    public BoxCollider SelectionCollider;
    public GachaMachine Gacha;
    public Shoebox Shoebox;
    public ForceYeet GameManager;
    public Database Database;

    public bool InGame;
    private bool hidden = true;

    private async void Awake()
    {
        MainButton.onClick.AddListener(ToMain);
        SettingsButton.onClick.AddListener(ToSettings);
        CollectionButton.onClick.AddListener(ToCollection);

        PlayerCard.OnNameChanged += Main.SetCanPlay;

        await InitGame();
    }

    private async Awaitable InitGame()
    {
        await Database.Init();

        bool hasPlayer = await Database.HasPlayerCard();

        if (hasPlayer)
        {
            PlayerDataDB card = await Database.GetPlayer();
            Debug.Log($"Got player: {card.PlayerCard.Name}");
        }
        else
        {
            // TODO: actually take input from player
            /*
            while (true)
            {
                Debug.Log("TODO: create a player");

                await Awaitable.EndOfFrameAsync();
            }
            */

            PlayerCardDB playerCard = new()
            {
                Name = "Per",
                Font = 0,
                Boarder = 0,
                Stickers = new()
            };
            PlayerDataDB playerData = await Database.CreatePlayer(playerCard);
            Debug.Log($"Created Player: {playerData.PlayerCard.Name}");
        }

        ToMain();
    }

    public void StartGame()
    {
        InGame = true;
        Anim.SetTrigger("ToGame");
        SetHidden(true);

        StartCoroutine(GachamachineState());
    }

    public void ToggleHidden()
    {
        SetHidden(!hidden);
    }

    public void SetHidden(bool hide)
    {
        hidden = hide;
        Anim.SetBool("Show", !hidden);
    }

    public void ToMain()
    {
        Main.gameObject.SetActive(true);
        Main.Show(InGame);
        PlayerCard.SetInteractable(!InGame);
        Settings.SetActive(false);
        Collection.SetActive(false);

        SetHidden(false);
    }

    public void ToSettings()
    {
        Main.gameObject.SetActive(false);
        Main.Show(InGame);
        Settings.SetActive(true);
        Collection.SetActive(false);

        SetHidden(false);
    }

    public void ToCollection()
    {
        Main.gameObject.SetActive(false);
        Main.Show(InGame);
        Settings.SetActive(false);
        Collection.SetActive(true);

        SetHidden(false);
    }

    public void BackToMenu()
    {
        // TODO: reset state and stuff

        InGame = false;
        Anim.SetTrigger("ToMenu");

        ToMain();
    }

    private void Update()
    {
        if (InGame && !hidden)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(!SelectionCollider.Raycast(ray, out _, 1000.0f))
                {
                    SetHidden(true);
                }
            }
        }
    }

    // Game states

    private IEnumerator GachamachineState()
    {
        yield return Gacha.RunGacha();

        StartCoroutine(GameplayState());
    }

    public IEnumerator GameplayState()
    {
        // TODO: Show players (VS splash)
        // TODO: Spawn enemy team

        Shoebox.RespawnAll();

        yield return Shoebox.PickTeam();

        yield return GameManager.Play(Shoebox.Team, new List<Pawn>());

        // TODO: show result of game
        yield return new WaitForSeconds(1);

        // TODO: get actual rewards
        Gacha.Tokens = 5;

        StartCoroutine(GachamachineState());
    }
}

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
    public CameraManager CameraManager;
    public PlaceableAreas PlaceableAreas;

    public bool InGame;
    private bool hidden = true;
    private PlayerDataDB playerData = null;
    private int currentLevel;

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
            playerData = await Database.GetPlayer();
            Debug.Log($"Got player: {playerData.PlayerCard.Name}");
        }
        else
        {
            // TODO: actually take input from player
            /*
            while (true)
            {
                Debug.Log("TODO: create a player");

                await Awaitable.NextFrameAsync();
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

    private async Awaitable GachamachineState()
    {
        await CameraManager.Gacha();
        await Gacha.RunGacha();
        await CameraManager.Idle();

        StartCoroutine(GameplayState());
    }

    public async Awaitable GameplayState()
    {
        OpponentDB opponent = await Database.GetOpponent(currentLevel);

        if (opponent == null)
        {
            opponent = GenerateOpponent(currentLevel);
        }

        // TODO: Show players (VS splash)

        List<Pawn> opponentTeam = new();
        foreach(PawnDB pawnDb in opponent.Board)
        {
            Pawn prefab = Gacha.Prefabs[pawnDb.PawnType];
            Vector3 playerCenter = PlaceableAreas.GetCenter(true);
            Vector3 opponentCenter = PlaceableAreas.GetCenter(false);
            Pawn pawn = Instantiate(prefab, (pawnDb.Location - playerCenter) + opponentCenter, Quaternion.identity);
            pawn.Team = 1;
            pawn.enabled = false;
            pawn.rigidbody.isKinematic = true;
            pawn.Rarity = (PawnRarity)pawnDb.Rarity;
            pawn.ColorValue = pawnDb.Color;
            pawn.InitializeVisuals();
            opponentTeam.Add(pawn);
        }

        Shoebox.RespawnAll();

        await CameraManager.Placing();

        await Shoebox.PickTeam();

        await CameraManager.Idle();

        List<PawnDB> dbPawns = new();
        foreach(Pawn pawn in Shoebox.Team)
        {
            dbPawns.Add(new PawnDB()
            {
                Location = pawn.transform.position,
                PawnType = pawn.PrefabId,
                Color = (byte)pawn.ColorValue,
                Rarity = (byte)pawn.Rarity

            });
        }
        await Database.CreateOpponent(currentLevel, playerData.PlayerCard, dbPawns);

        await GameManager.Play(Shoebox.Team, opponentTeam);

        // TODO: show result of game
        await Awaitable.WaitForSecondsAsync(1.0f);

        playerData.Lives = 3; // TODO: lives
        playerData.Level = currentLevel;
        playerData.Box = new List<PawnDB>();
        await Database.UpdatePlayer(playerData);

        // Next level
        // TODO: rewards
        Gacha.Tokens = 5;
        currentLevel += 1;

        StartCoroutine(GachamachineState());
    }

    private OpponentDB GenerateOpponent(int level)
    {
        Debug.LogWarning("Using generated opponent");

        PlayerCardDB card = new PlayerCardDB()
        {
            Name = "Generated",
            Boarder = 0,
            Font = 0,
            Stickers = new()
        };

        List<PawnDB> board = new();

        for(int i = 0; i < 5; i++)
        {
            // TODO: generate pawns based on level
            PawnDB pawn = new()
            {
                PawnType = Random.Range(0, Gacha.Prefabs.Count),
                Location = PlaceableAreas.GetRandomPointInPlaceableArea()
            };
            board.Add(pawn);
        }

        return new OpponentDB()
        {
            PlayerCard = card,
            Board = board
        };
    }
}

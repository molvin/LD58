using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject Settings;
    public GameObject Collection;
    public GameObject Cover;

    public Animator Anim;
    public BoxCollider SelectionCollider;
    public GachaMachine Gacha;
    public Shoebox Shoebox;
    public ForceYeet GameManager;
    public Database Database;
    public CameraManager CameraManager;
    public PlaceableAreas PlaceableAreas;

    public OpponentNotepad OpponentNotepad;
    public PlayerDataDB PlayerData { get; private set; }

    public bool InGame;
    private bool hidden = true;
    private int currentLevel;

    private async void Awake()
    {
        PlayerCard.UpdateCoins(0);
        PlayerCard.UpdateHealth(0);

        await InitGame();

        MainButton.onClick.AddListener(ToMain);
        SettingsButton.onClick.AddListener(ToSettings);
        CollectionButton.onClick.AddListener(ToCollection);
    }

    private async Awaitable InitGame()
    {
        await Database.Init();

        bool hasPlayer = await Database.HasPlayerCard();

        if (hasPlayer)
        {
            PlayerData = await Database.GetPlayer();
            Debug.Log($"Got player: {PlayerData.PlayerCard.Name}");
        }
        else
        {
            PlayerCardDB playerCard = new()
            {
                Name = "",
                Font = 0,
                Boarder = 0,
                Stickers = new()
            };
            PlayerDataDB playerData = await Database.CreatePlayer(playerCard);
            Debug.Log($"Created Player: {playerData.PlayerCard.Name}");
        }

        PlayerCard.Init(PlayerData.PlayerCard);

        ToMain();
    }

    public async void StartGame()
    {
        PlayerData = await Database.NewGame();
        PlayerData.PlayerCard = PlayerCard.GetPlayerCard();
        await Database.UpdatePlayer(PlayerData);

        InGame = true;
        PlayerCard.Show(InGame);
        PlayerCard.SetInteractable(!InGame);
        Anim.SetTrigger("ToGame");
        SetHidden(true);

        Gacha.Tokens = 5;
        PlayerData.Lives = 3;
        PlayerCard.UpdateCoins(Gacha.Tokens);
        PlayerCard.UpdateHealth(PlayerData.Lives);

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
        PlayerCard.gameObject.SetActive(true);
        Cover.SetActive(true);
        PlayerCard.Show(InGame);
        PlayerCard.SetInteractable(!InGame);
        Settings.SetActive(false);
        Collection.SetActive(false);

        SetHidden(false);
    }

    public void ToSettings()
    {
        PlayerCard.gameObject.SetActive(false);
        Cover.SetActive(false);
        PlayerCard.Show(InGame);
        Settings.SetActive(true);
        Collection.SetActive(false);

        SetHidden(false);
    }

    public void ToCollection()
    {
        PlayerCard.gameObject.SetActive(false);
        Cover.SetActive(false);
        PlayerCard.Show(InGame);
        Settings.SetActive(false);
        Collection.SetActive(true);

        SetHidden(false);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public async Awaitable<bool> AddToCollection(int prefabIndex, int rarity)
    {
        byte id = (byte)(prefabIndex * 4 + rarity);
        if(PlayerData.Collection == null)
        {
            PlayerData.Collection = new();
        }
        if(PlayerData.Collection.Contains(id))
            return false;

        PlayerData.Collection.Add(id);
        await Database.UpdatePlayer(PlayerData);

        return true;
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

        OpponentNotepad.PlayerCard.Init(opponent.PlayerCard);
        Anim.SetBool("Versus", true);
        OpponentNotepad.Anim.SetBool("Versus", true);

        await Awaitable.WaitForSecondsAsync(1.5f);

        Anim.SetBool("Versus", false);
        OpponentNotepad.Anim.SetBool("Versus", false);


        List<Pawn> opponentTeam = new();
        foreach(PawnDB pawnDb in opponent.Board)
        {
            Pawn prefab = Gacha.Prefabs[pawnDb.PawnType];

            // Transform location from player field to opponent field
            Vector3 localPoint = PlaceableAreas.PlayerSide[0].transform.InverseTransformPoint(pawnDb.Location);
            Vector3 worldPoint = PlaceableAreas.OpponentSide[0].transform.TransformPoint(localPoint);

            Pawn pawn = Instantiate(prefab, worldPoint, Quaternion.Euler(0, 180, 0));
            pawn.Team = 1;
            pawn.enabled = false;
            pawn.rigidbody.isKinematic = true;
            pawn.Rarity = (PawnRarity)pawnDb.Rarity;
            pawn.ColorValue = pawnDb.Color;
            pawn.InitializeVisuals();
            opponentTeam.Add(pawn);
        }

        await Awaitable.WaitForSecondsAsync(1.5f);

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
        await Database.CreateOpponent(currentLevel, PlayerData.PlayerCard, dbPawns);

        ForceYeet.GameState result = await GameManager.Play(Shoebox.Team, opponentTeam);

        // TODO: show result of game
        
        await Awaitable.WaitForSecondsAsync(1.0f);

        PlayerData.Level = currentLevel;
        PlayerData.Box = new List<PawnDB>();
        await Database.UpdatePlayer(PlayerData);

        if (result == ForceYeet.GameState.OpponentWon)
        {
            PlayerData.Lives--;
            PlayerCard.UpdateHealth(PlayerData.Lives);

            if (PlayerData.Lives == 0)
            {
                Debug.Log("GAME OVER");
                await Awaitable.WaitForSecondsAsync(1.0f);
                SceneManager.LoadScene(0);
                return;
            }
        }
        Gacha.Tokens += 5;
        PlayerCard.UpdateCoins(Gacha.Tokens);
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

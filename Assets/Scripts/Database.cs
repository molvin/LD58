using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using static Database;
using System.IO;
using System.ComponentModel;
using Unity.VisualScripting;

public class Database : MonoBehaviour
{
    public Action<FirebaseUser> UserChanged;
    private FirebaseUser user = null;
    private FirebaseAuth auth = null;
    FirebaseFirestore firestore = null;
    private bool updatingProfile = false;
    private DocumentReference playerDocRef;
    private CollectionReference opponentCollection;
    private DocumentReference opponentReferenceRef;

    [FirestoreData]
    public class PlayerData
    {
        [FirestoreProperty]
        public PlayerCard PlayerCard { get; set; }
        [FirestoreProperty]
        public int Lives { get; set; }
        [FirestoreProperty]
        public int Level { get; set; }
        [FirestoreProperty]
        public List<PawnDB> Box { get; set; }
    }
    [FirestoreData]
    public class Opponent
    {
        [FirestoreProperty]
        public PlayerCard PlayerCard { get; set; }
        [FirestoreProperty]
        public List<PawnDB> Board { get; set; }
    }
    [FirestoreData]
    public class PawnDB
    {
        [FirestoreProperty]
        public int PawnType { get; set; }
        [FirestoreProperty(ConverterType = typeof(Vector3Converter))]
        public Vector3 Location { get; set; }
    }
    [FirestoreData]
    public class PlayerCard
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public int Font { get; set; }
        [FirestoreProperty]
        public int Boarder { get; set; }
        [FirestoreProperty]
        public List<Sticker> Stickers { get; set; }
    }
    [FirestoreData]
    public class Sticker
    {
        [FirestoreProperty]
        public int StickerType { get; set; }
        [FirestoreProperty(ConverterType = typeof(Vector2Converter))]
        public Vector2 Location { get; set; }
        [FirestoreProperty]
        public int Rotation { get; set; }
    }

    private async void Awake()
    {
        //Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", "no");
        InitializeFirebase();

        //Sign in
        AuthResult result = await auth.SignInAnonymouslyAsync();
        
        InitializeFireStore();

        //Create player card
        PlayerCard card = new PlayerCard();
        card.Name = "Kvasir";
        card.Stickers = new List<Sticker>();
        await CreatePlayer(card);

        //Make opponent
        List<PawnDB> board = new List<PawnDB>();
        board.Add(new PawnDB());
        await CreateOpponent(board);
        board.Add(new PawnDB());
        await CreateOpponent(board);
        board.Add(new PawnDB());
        await CreateOpponent(board);


        //Get opp
        Opponent ha = await GetOpponent();
        Debug.Log(ha.PlayerCard.Name);
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void InitializeFireStore()
    {
        // Reference to the 'users' collection
        playerDocRef = firestore.Collection("users").Document(user.UserId);
        opponentCollection = firestore.Collection("opponents");
        opponentReferenceRef = opponentCollection.Document("opponent_reference");
    }

    public async Task<PlayerData> GetPlayer()
    {
        DocumentSnapshot playerSnap = await playerDocRef.GetSnapshotAsync();
        return playerSnap.ConvertTo<PlayerData>();
    }

    public async Task<PlayerData> CreatePlayer(PlayerCard card)
    {
        PlayerData playerData = new PlayerData();
        playerData.PlayerCard = card;
        playerData.Box = new List<PawnDB>();
        playerData.Lives = 3;
        playerData.Level = 0;
        await playerDocRef?.SetAsync(playerData);
        return playerData;
    }

    public async Awaitable UpdatePlayer(PlayerData data)
    {
        await playerDocRef?.SetAsync(data);
    }

    public async Awaitable CreateOpponent(List<PawnDB> board)
    {
        await firestore.RunTransactionAsync(async transaction =>
        {
            PlayerData player = await GetPlayer();
            string level = player.Level.ToString();

            //Inc opponent reference
            DocumentSnapshot snap = await opponentReferenceRef.GetSnapshotAsync();
            int currentOpponentCount = snap.GetValue<int>(level) + 1;
            Dictionary<string, int> opponentRefUpdate = new Dictionary<string, int> { { level, currentOpponentCount } };

            //Add the table
            DocumentReference opRef = opponentCollection.Document($"opponent_{level}:{currentOpponentCount}");
            Opponent op = new Opponent();
            op.PlayerCard = player.PlayerCard;
            op.Board = board;

            //Set dataA§1qaAA
            transaction.Set(opRef, op);
            transaction.Set(opponentReferenceRef, opponentRefUpdate);
        });
    }
    public async Task<Opponent> GetOpponent()
    {
        PlayerData player = await GetPlayer();
        string level = player.Level.ToString();
        DocumentSnapshot snap = await opponentReferenceRef.GetSnapshotAsync();
        int currentOpponentCount = snap.GetValue<int>(level);
        System.Random random = new System.Random();
        DocumentReference opRef = opponentCollection.Document($"opponent_{level}:{random.Next(1, currentOpponentCount)}");
        DocumentSnapshot opSnap = await opRef.GetSnapshotAsync();
        return opSnap.ConvertTo<Opponent>();
    }

    public async Task<bool> HasPlayerCard()
    {
        DocumentSnapshot playerSnap = await playerDocRef.GetSnapshotAsync();
        return playerSnap.Exists;
    }

    public async Awaitable NewGame()
    {
        PlayerData player = await GetPlayer();
        player.Box = new List<PawnDB>();
        player.Lives = 3;
        player.Level = 0;
        await playerDocRef.SetAsync(player);
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                UserChanged?.Invoke(null);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user);
                UserChanged?.Invoke(user);
            }
        }
    }

    private void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
public class Vector2Converter : FirestoreConverter<Vector2>
{
    public override Vector2 FromFirestore(object bytes)
    {
        Blob blob = (Blob)bytes;
        byte[] byteArray = blob.ToBytes();
        return new Vector2(BitConverter.ToSingle(byteArray, 0), BitConverter.ToSingle(byteArray, 4));
    }

    public override object ToFirestore(Vector2 value)
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(BitConverter.GetBytes(value.x));
        stream.Write(BitConverter.GetBytes(value.y));
        return stream.ToArray();
    }
}
public class Vector3Converter : FirestoreConverter<Vector3>
{
    public override Vector3 FromFirestore(object bytes)
    {
        Blob blob = (Blob)bytes;
        byte[] byteArray = blob.ToBytes();
        return new Vector3(BitConverter.ToSingle(byteArray, 0), BitConverter.ToSingle(byteArray, 4), BitConverter.ToSingle(byteArray, 8));
    }

    public override object ToFirestore(Vector3 value)
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(BitConverter.GetBytes(value.x));
        stream.Write(BitConverter.GetBytes(value.y));
        stream.Write(BitConverter.GetBytes(value.z));
        return stream.ToArray();
    }
}
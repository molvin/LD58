using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.IO;

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
    public class PlayerDataDB
    {
        [FirestoreProperty]
        public PlayerCardDB PlayerCard { get; set; }
        [FirestoreProperty]
        public int Lives { get; set; }
        [FirestoreProperty]
        public int Level { get; set; }
        [FirestoreProperty]
        public List<PawnDB> Box { get; set; }
    }
    [FirestoreData]
    public class OpponentDB
    {
        [FirestoreProperty]
        public PlayerCardDB PlayerCard { get; set; }
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
    public class PlayerCardDB
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public int Font { get; set; }
        [FirestoreProperty]
        public int Boarder { get; set; }
        [FirestoreProperty]
        public List<StickerDB> Stickers { get; set; }
    }
    [FirestoreData]
    public class StickerDB
    {
        [FirestoreProperty]
        public int StickerType { get; set; }
        [FirestoreProperty(ConverterType = typeof(Vector2Converter))]
        public Vector2 Location { get; set; }
        [FirestoreProperty]
        public int Rotation { get; set; }
    }

    public async Awaitable Init()
    {
        //Environment.SetEnvironmentVariable("USE_AUTH_EMULATOR", "no");
        InitializeFirebase();

        //Sign in
        AuthResult result = await auth.SignInAnonymouslyAsync();
        
        InitializeFireStore();

        /*
        //Create player card
        PlayerCardDB card = new PlayerCardDB();
        card.Name = "Kvasir";
        card.Stickers = new List<StickerDB>();
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
        OpponentDB ha = await GetOpponent();
        Debug.Log(ha.PlayerCard.Name);

        */

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

    public async Task<PlayerDataDB> GetPlayer()
    {
        DocumentSnapshot playerSnap = await playerDocRef.GetSnapshotAsync();
        return playerSnap.ConvertTo<PlayerDataDB>();
    }

    public async Task<PlayerDataDB> CreatePlayer(PlayerCardDB card)
    {
        PlayerDataDB playerData = new PlayerDataDB();
        playerData.PlayerCard = card;
        playerData.Box = new List<PawnDB>();
        playerData.Lives = 3;
        playerData.Level = 0;
        await playerDocRef?.SetAsync(playerData);
        return playerData;
    }

    public async Awaitable UpdatePlayer(PlayerDataDB data)
    {
        await playerDocRef?.SetAsync(data);
    }

    public async Awaitable CreateOpponent(int level, PlayerCardDB playerCard, List<PawnDB> board)
    {
        await firestore.RunTransactionAsync(async transaction =>
        {
            //Inc opponent reference
            DocumentSnapshot snap = await opponentReferenceRef.GetSnapshotAsync();

            bool found = snap.TryGetValue(level.ToString(), out int currentOpponentCount);
            if(!found)
            {
                currentOpponentCount = 1;
            }
            else
            {
                currentOpponentCount += 1;
            }

            Dictionary<string, int> opponentRefUpdate = new Dictionary<string, int> { { level.ToString(), currentOpponentCount } };

            //Add the table
            DocumentReference opRef = opponentCollection.Document($"opponent_{level}:{currentOpponentCount}");
            OpponentDB op = new OpponentDB();
            op.PlayerCard = playerCard;
            op.Board = board;

            //Set dataA§1qaAA
            transaction.Set(opRef, op);
            transaction.Set(opponentReferenceRef, opponentRefUpdate);
        });
    }
    public async Task<OpponentDB> GetOpponent(int level)
    {
        DocumentSnapshot snap = await opponentReferenceRef.GetSnapshotAsync();
        bool found = snap.TryGetValue(level.ToString(), out int currentOpponentCount);
        if(!found)
        {
            return null;
        }
        
        System.Random random = new System.Random();
        DocumentReference opRef = opponentCollection.Document($"opponent_{level}:{random.Next(1, currentOpponentCount)}");
        DocumentSnapshot opSnap = await opRef.GetSnapshotAsync();
        return opSnap.ConvertTo<OpponentDB>();
    }

    public async Task<bool> HasPlayerCard()
    {
        DocumentSnapshot playerSnap = await playerDocRef.GetSnapshotAsync();
        return playerSnap.Exists;
    }

    public async Awaitable NewGame()
    {
        PlayerDataDB player = await GetPlayer();
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
                Debug.Log("Signed in " + user.UserId);
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
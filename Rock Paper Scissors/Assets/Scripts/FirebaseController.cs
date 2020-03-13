using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseController : MonoBehaviour
{
    public static FirebaseController Instance;

    private FirebaseApp app;
    private DatabaseReference dbRef;

    private string dbLink = ""; // Database URL-link

    private int playerID;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
                InitFB();
            else
                Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
        });
    }

    private void InitFB()
    {
        app = FirebaseApp.DefaultInstance;
        app.SetEditorDatabaseUrl(dbLink);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("gameroom");

        dbRef.ValueChanged += GameRoomValueChanged;
        RegisterForGame();
    }

    private void RegisterForGame()
    {
        dbRef.Child("player1").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("fail");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (snapshot.Value.ToString() == "-")
                playerID = 1;
            else
                playerID = 2;

            dbRef.Child("player" + playerID).SetValueAsync("+");
        });
    }

    public void SendChoose(int choose)
    {
        string letter = choose == 0 ? "r" : choose == 1 ? "p" : "s";
        dbRef.Child("player" + playerID + "choose").SetValueAsync(letter);
    }

    private void GameRoomValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError(e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot.Child("player1").Value.ToString() != "-" &&
            e.Snapshot.Child("player2").Value.ToString() != "-")
        {
            string p1Choose = e.Snapshot.Child("player1choose").Value.ToString();
            string p2Choose = e.Snapshot.Child("player2choose").Value.ToString();

            if (!GameController.Instance.IsStarted)
                GameController.Instance.StartGame();
            else if (p1Choose != "-" && p2Choose != "-")
            {
                int winnerId = 0;

                if (p1Choose == "r")
                    winnerId = p2Choose == "s" ? 1 : 2;
                else if (p1Choose == "s")
                    winnerId = p2Choose == "p" ? 1 : 2;
                else
                    winnerId = p2Choose == "r" ? 1 : 2;

                GameController.Instance.SetResult(winnerId == playerID);
            }
        }
    }
}

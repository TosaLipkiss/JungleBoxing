using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Linq;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    //Singleton
    public static FirebaseManager Instance { get; private set; }

    //Delegates
    public delegate void OnLoadedDelegate(string jsonData);
    public delegate void OnSaveDelegate();

    FirebaseDatabase db;

    private void Awake()
    {
        //Singleton setup
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        db = FirebaseDatabase.DefaultInstance;

      //  FirebaseAuth.DefaultInstance.SignOut();
    }

    //loads the data at "path" then returns json result to the delegate/callback function
    public IEnumerator LoadData(string path, OnLoadedDelegate onLoadedDelegate)
    {
        var dataTask = db.RootReference.Child(path).GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
            Debug.LogWarning(dataTask.Exception);

        string jsonData = dataTask.Result.GetRawJsonValue();

        onLoadedDelegate(jsonData);
    }

    //This load gives one callback for each result in the database.
    public IEnumerator LoadDataMultiple(string path, OnLoadedDelegate onLoadedDelegate)
    {
        var dataTask = db.RootReference.Child(path).GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);
        string jsonData = dataTask.Result.GetRawJsonValue();

        if (dataTask.Exception != null)
            Debug.LogWarning(dataTask.Exception);

        foreach (var item in dataTask.Result.Children)
        {
            onLoadedDelegate(item.GetRawJsonValue());
        }
    }

    //Save data to our database.
    public IEnumerator SaveData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    {
        var dataTask = db.RootReference.Child(path).SetRawJsonValueAsync(data);
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
            Debug.LogWarning(dataTask.Exception);

        if (onSaveDelegate != null)
        {
            onSaveDelegate();
        }
    }

    public IEnumerator CheckForGame(string path, OnLoadedDelegate onLoadedDelegate = null)
    {
        Debug.Log("checking for game");
        //var dataTask = db.GetReference("games").OrderByChild("status").EqualTo("new").GetValueAsync();
        var dataTask = db.GetReference("games").GetValueAsync();

        yield return new WaitUntil(() => dataTask.IsCompleted);

        // string jsonData = dataTask.Result.GetRawJsonValue();

        //Debug.Log("game data: " + jsonData);

        if (dataTask.Exception != null)
            Debug.LogWarning(dataTask.Exception);

        var games = dataTask.Result.Children.Where(x => (string)x.Child("status").Value == "new").ToArray();
        if (games.Length > 0)
        {
            foreach (var item in games)
            {
                Debug.Log("multiple data found: " + item.GetRawJsonValue());

                onLoadedDelegate(item.GetRawJsonValue());
                break;
            }
        }
        else
        {
            onLoadedDelegate("");
        }
    }
}

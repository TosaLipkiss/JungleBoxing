using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System.Collections;
using Firebase.Auth;

public class FirebaseTest : MonoBehaviour
{
    FirebaseDatabase db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }

           // StartCoroutine(RegUser("test@test.test", "test123!"));
            StartCoroutine(SignIn("test@test.test", "test123!"));
        });
    }

    private IEnumerator RegUser(string email, string password)
    {
        Debug.Log("Starting Registration");
        var auth = FirebaseAuth.DefaultInstance;
        var regTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        //show loading animation

        yield return new WaitUntil(() => regTask.IsCompleted);

        //remove loading animation

        if (regTask.Exception != null)
            Debug.LogWarning(regTask.Exception);
        else
            Debug.Log("Registration Complete");
    }

    private IEnumerator SignIn(string email, string password)
    {
        Debug.Log("Atempting to log in");
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);


        if (loginTask.Exception != null)
            Debug.LogWarning(loginTask.Exception);
        else
            Debug.Log("login completed");

       // StartCoroutine(DataTest(FirebaseAuth.DefaultInstance.CurrentUser.UserId, "TestWrite"));
    }

    private IEnumerator DataTest(string userID, string data)
    {
        Debug.Log("Trying to write data");
        var db = FirebaseDatabase.DefaultInstance;
        var dataTask = db.RootReference.Child("users").Child(userID).SetValueAsync(data);

        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
            Debug.LogWarning(dataTask.Exception);
        else
            Debug.Log("DataTestWrite: Complete");
    }
}
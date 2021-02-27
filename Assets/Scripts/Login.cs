using System.Collections;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;

public class Login : MonoBehaviour
{
    public InputField username;
    public InputField password;

    public Button playButton;

    public Text outputText;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }
        });

        playButton.interactable = false;

        playButton.onClick.AddListener(() => ActiveUser.Instance.LoadUserData());
    }

    public void LoginUser()
    {
        StartCoroutine(SignIn(username.text, password.text));
    }

    public IEnumerator SignIn(string email, string password)
    {
        Debug.Log("Attempting to login....");
        outputText.text = "Attempting to log in";
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if(loginTask.Exception != null)
        {
            Debug.LogWarning(loginTask.Exception);
            outputText.text = loginTask.Exception.InnerExceptions[0].InnerException.Message;
        }
        else
        {
            Debug.Log("login completed");
        }

        outputText.text = "Logged in as: " + loginTask.Result.Email;
        playButton.interactable = true;
    }

    public void RegUser()
    {
        StartCoroutine(RegUser(username.text, password.text));
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

}
